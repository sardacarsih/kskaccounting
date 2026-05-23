using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Contracts;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Domain;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.FixedAssets.Infrastructure.Oracle;

public sealed class OracleFixedAssetLifecycleRepository : IFixedAssetLifecycleRepository
{
    private readonly string _connectionString;

    public OracleFixedAssetLifecycleRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<string> GenerateDocumentNoAsync(string idData, string docType, AccountingPeriod period, CancellationToken cancellationToken)
    {
        const string mergeSql = """
            MERGE INTO ACCT_FA_DOC_SEQ T
            USING (SELECT :p_iddata IDDATA, :p_doc_type DOC_TYPE, :p_year YYYY, :p_month MM FROM DUAL) S
            ON (T.IDDATA = S.IDDATA AND T.DOC_TYPE = S.DOC_TYPE AND T.YYYY = S.YYYY AND T.MM = S.MM)
            WHEN MATCHED THEN UPDATE SET T.LAST_NO = T.LAST_NO + 1, T.MODIFIED_DATE = SYSTIMESTAMP
            WHEN NOT MATCHED THEN INSERT (IDDATA, DOC_TYPE, YYYY, MM, LAST_NO, CREATED_DATE)
                 VALUES (S.IDDATA, S.DOC_TYPE, S.YYYY, S.MM, 1, SYSTIMESTAMP)
            """;

        const string getNoSql = """
            SELECT LAST_NO
            FROM ACCT_FA_DOC_SEQ
            WHERE IDDATA = :p_iddata
              AND DOC_TYPE = :p_doc_type
              AND YYYY = :p_year
              AND MM = :p_month
            """;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using OracleTransaction trx = conn.BeginTransaction();
        try
        {
            CommandDefinition mergeCmd = new(
                mergeSql,
                new { p_iddata = idData, p_doc_type = docType, p_year = period.Year, p_month = period.Month },
                trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            await conn.ExecuteAsync(mergeCmd).ConfigureAwait(false);

            CommandDefinition getCmd = new(
                getNoSql,
                new { p_iddata = idData, p_doc_type = docType, p_year = period.Year, p_month = period.Month },
                trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            int lastNo = await conn.ExecuteScalarAsync<int>(getCmd).ConfigureAwait(false);
            await trx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return $"{docType}/{idData}/{period.Year:0000}{period.Month:00}/{lastNo:000000}";
        }
        catch
        {
            await trx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<long> CreateTransactionAsync(FixedAssetTransactionCreateRequest request, string documentNo, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO ACCT_FA_TRX_HDR
            (
                IDDATA, DOC_NO, TRX_TYPE, DOC_DATE, PERIOD, PERIOD_KEY, ASSET_ID,
                AMOUNT_BASE, OLD_AMOUNT_BASE, NEW_AMOUNT_BASE, CURRENCY_CODE, EXCHANGE_RATE,
                STATUS, SOURCE_REF_NO, REMARKS, CREATED_BY, CREATED_DATE
            )
            VALUES
            (
                :p_iddata, :p_doc_no, :p_trx_type, :p_doc_date, :p_period, :p_period_key, :p_asset_id,
                :p_amount_base, :p_old_amount_base, :p_new_amount_base, :p_currency_code, :p_exchange_rate,
                'DRAFT', :p_source_ref_no, :p_remarks, :p_created_by, SYSTIMESTAMP
            )
            RETURNING TRX_ID INTO :p_trx_id
            """;

        AccountingPeriod period = AccountingPeriod.Parse(request.Period);

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        DynamicParameters p = new();
        p.Add("p_iddata", request.IdData);
        p.Add("p_doc_no", documentNo);
        p.Add("p_trx_type", MapType(request.TransactionType));
        p.Add("p_doc_date", request.DocumentDate);
        p.Add("p_period", period.PeriodeString);
        p.Add("p_period_key", period.Year * 100 + period.Month);
        p.Add("p_asset_id", request.AssetId);
        p.Add("p_amount_base", request.AmountBase);
        p.Add("p_old_amount_base", request.OldAmountBase);
        p.Add("p_new_amount_base", request.NewAmountBase);
        p.Add("p_currency_code", request.CurrencyCode);
        p.Add("p_exchange_rate", request.ExchangeRate);
        p.Add("p_source_ref_no", request.SourceReferenceNo);
        p.Add("p_remarks", request.Remarks);
        p.Add("p_created_by", request.UserId);
        p.Add("p_trx_id", dbType: DbType.Int64, direction: ParameterDirection.Output);

        CommandDefinition cmd = new(sql, p, commandType: CommandType.Text, cancellationToken: cancellationToken);
        await conn.ExecuteAsync(cmd).ConfigureAwait(false);
        long trxId = p.Get<long>("p_trx_id");

        const string auditSql = """
            INSERT INTO ACCT_FA_AUDIT_LOG
            (
                IDDATA, ENTITY_NAME, ENTITY_ID, ACTION_TYPE, DOC_NO, PERIOD, ACTION_BY, ACTION_TS
            )
            VALUES
            (
                :p_iddata, 'ACCT_FA_TRX_HDR', :p_entity_id, 'CREATE', :p_doc_no, :p_period, :p_user, SYSTIMESTAMP
            )
            """;
        CommandDefinition auditCmd = new(
            auditSql,
            new
            {
                p_iddata = request.IdData,
                p_entity_id = trxId.ToString(),
                p_doc_no = documentNo,
                p_period = period.PeriodeString,
                p_user = request.UserId
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        await conn.ExecuteAsync(auditCmd).ConfigureAwait(false);
        return trxId;
    }

    public async Task SubmitForApprovalAsync(string idData, long transactionId, string userId, CancellationToken cancellationToken)
    {
        const string updateSql = """
            UPDATE ACCT_FA_TRX_HDR
               SET STATUS = 'SUBMITTED',
                   MODIFIED_BY = :p_user,
                   MODIFIED_DATE = SYSTIMESTAMP
             WHERE IDDATA = :p_iddata
               AND TRX_ID = :p_trx_id
               AND STATUS = 'DRAFT'
            """;

        const string insertApprovalSql = """
            INSERT INTO ACCT_FA_APPROVAL_DTL
            (
                TRX_ID, STEP_NO, ROLE_CODE, STATUS, ACTION_BY, ACTION_DATE, ACTION_COMMENT, CREATED_DATE
            )
            VALUES
            (
                :p_trx_id, 1, 'CHECKER', 'SUBMITTED', :p_user, SYSTIMESTAMP, 'Auto submit', SYSTIMESTAMP
            )
            """;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using OracleTransaction trx = conn.BeginTransaction();
        try
        {
            CommandDefinition updateCmd = new(updateSql, new { p_user = userId, p_iddata = idData, p_trx_id = transactionId }, trx, commandType: CommandType.Text, cancellationToken: cancellationToken);
            int affected = await conn.ExecuteAsync(updateCmd).ConfigureAwait(false);
            if (affected == 0)
            {
                throw new DataException("Transaksi tidak dapat disubmit.");
            }

            CommandDefinition approvalCmd = new(insertApprovalSql, new { p_trx_id = transactionId, p_user = userId }, trx, commandType: CommandType.Text, cancellationToken: cancellationToken);
            await conn.ExecuteAsync(approvalCmd).ConfigureAwait(false);

            await InsertAuditLogAsync(
                conn,
                trx,
                idData,
                transactionId,
                "SUBMIT",
                userId,
                "STATUS",
                "DRAFT",
                "SUBMITTED",
                string.Empty,
                string.Empty,
                "Submitted for approval",
                cancellationToken).ConfigureAwait(false);
            await trx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await trx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task ApproveAsync(ApprovalActionRequest request, CancellationToken cancellationToken)
    {
        await ProcessApprovalActionAsync(request, "APPROVED", cancellationToken).ConfigureAwait(false);
    }

    public async Task RejectAsync(ApprovalActionRequest request, CancellationToken cancellationToken)
    {
        await ProcessApprovalActionAsync(request, "REJECTED", cancellationToken).ConfigureAwait(false);
    }

    public async Task<LifecyclePostingActionResult> PostApprovedAsync(LifecyclePostingActionRequest request, CancellationToken cancellationToken)
    {
        const string getTrxSql = """
            SELECT
                TRX_ID,
                IDDATA,
                DOC_NO,
                PERIOD,
                TRX_TYPE,
                STATUS,
                ASSET_ID,
                AMOUNT_BASE,
                OLD_AMOUNT_BASE,
                NEW_AMOUNT_BASE,
                NOJURNAL,
                JURNALID
            FROM ACCT_FA_TRX_HDR
            WHERE IDDATA = :p_iddata
              AND TRX_ID = :p_trx_id
            FOR UPDATE
            """;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using OracleTransaction trx = conn.BeginTransaction();
        try
        {
            CommandDefinition getCmd = new(
                getTrxSql,
                new { p_iddata = request.IdData, p_trx_id = request.TransactionId },
                trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            LifecycleTrxRow? trxRow = await conn.QuerySingleOrDefaultAsync<LifecycleTrxRow>(getCmd).ConfigureAwait(false);
            if (trxRow is null)
            {
                throw new DataException($"Transaction {request.TransactionId} tidak ditemukan.");
            }

            bool isLocked = await IsPeriodLockedAsync(conn, trx, request.IdData, trxRow.PERIOD, cancellationToken).ConfigureAwait(false);
            if (isLocked)
            {
                throw new DataException($"Periode {trxRow.PERIOD} sudah locked.");
            }

            if (string.Equals(trxRow.STATUS, "POSTED", StringComparison.OrdinalIgnoreCase))
            {
                await trx.CommitAsync(cancellationToken).ConfigureAwait(false);
                return new LifecyclePostingActionResult
                {
                    TransactionId = trxRow.TRX_ID,
                    Status = "POSTED",
                    NoJurnal = trxRow.NOJURNAL ?? string.Empty,
                    JurnalId = trxRow.JURNALID
                };
            }

            if (!string.Equals(trxRow.STATUS, "APPROVED", StringComparison.OrdinalIgnoreCase))
            {
                throw new DataException($"Transaction {request.TransactionId} harus dalam status APPROVED untuk diposting.");
            }

            LifecycleJournalPostResult? postResult = await TryPostLifecycleJournalAsync(conn, trx, trxRow, request.UserId, cancellationToken).ConfigureAwait(false);
            if (postResult is null)
            {
                throw new DataException($"Transaction {request.TransactionId} tidak memiliki template posting otomatis.");
            }

            const string updatePostedSql = """
                UPDATE ACCT_FA_TRX_HDR
                   SET STATUS = 'POSTED',
                       NOJURNAL = :p_nojurnal,
                       JURNALID = :p_jurnalid,
                       MODIFIED_BY = :p_user,
                       MODIFIED_DATE = SYSTIMESTAMP
                 WHERE IDDATA = :p_iddata
                   AND TRX_ID = :p_trx_id
                   AND STATUS = 'APPROVED'
                """;
            CommandDefinition updateCmd = new(
                updatePostedSql,
                new
                {
                    p_nojurnal = postResult.NoJurnal,
                    p_jurnalid = postResult.JurnalId,
                    p_user = request.UserId,
                    p_iddata = request.IdData,
                    p_trx_id = request.TransactionId
                },
                trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            int affected = await conn.ExecuteAsync(updateCmd).ConfigureAwait(false);
            if (affected == 0)
            {
                throw new DataException($"Gagal post transaksi {request.TransactionId}. Status telah berubah.");
            }

            await InsertAuditLogAsync(
                conn,
                trx,
                request.IdData,
                request.TransactionId,
                "POST",
                request.UserId,
                "STATUS",
                "APPROVED",
                "POSTED",
                trxRow.DOC_NO,
                trxRow.PERIOD,
                string.IsNullOrWhiteSpace(request.Comment) ? $"Manual post journal {postResult.NoJurnal}" : request.Comment,
                cancellationToken).ConfigureAwait(false);

            await trx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return new LifecyclePostingActionResult
            {
                TransactionId = trxRow.TRX_ID,
                Status = "POSTED",
                NoJurnal = postResult.NoJurnal,
                JurnalId = postResult.JurnalId
            };
        }
        catch
        {
            await trx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task ReversePostedAsync(LifecyclePostingActionRequest request, CancellationToken cancellationToken)
    {
        const string getTrxSql = """
            SELECT
                TRX_ID,
                IDDATA,
                DOC_NO,
                PERIOD,
                TRX_TYPE,
                STATUS,
                ASSET_ID,
                AMOUNT_BASE,
                OLD_AMOUNT_BASE,
                NEW_AMOUNT_BASE,
                NOJURNAL,
                JURNALID
            FROM ACCT_FA_TRX_HDR
            WHERE IDDATA = :p_iddata
              AND TRX_ID = :p_trx_id
            FOR UPDATE
            """;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using OracleTransaction trx = conn.BeginTransaction();
        try
        {
            CommandDefinition getCmd = new(
                getTrxSql,
                new { p_iddata = request.IdData, p_trx_id = request.TransactionId },
                trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            LifecycleTrxRow? trxRow = await conn.QuerySingleOrDefaultAsync<LifecycleTrxRow>(getCmd).ConfigureAwait(false);
            if (trxRow is null)
            {
                throw new DataException($"Transaction {request.TransactionId} tidak ditemukan.");
            }

            if (!string.Equals(trxRow.STATUS, "POSTED", StringComparison.OrdinalIgnoreCase))
            {
                throw new DataException($"Transaction {request.TransactionId} harus status POSTED untuk reverse.");
            }

            bool isLocked = await IsPeriodLockedAsync(conn, trx, request.IdData, trxRow.PERIOD, cancellationToken).ConfigureAwait(false);
            if (isLocked)
            {
                throw new DataException($"Periode {trxRow.PERIOD} sudah locked.");
            }

            if (!trxRow.JURNALID.HasValue || trxRow.JURNALID.Value <= 0d)
            {
                throw new DataException($"Transaction {request.TransactionId} tidak memiliki JURNALID.");
            }

            await DeleteJurnalByIdAsync(conn, trx, request.IdData, trxRow.PERIOD, trxRow.JURNALID.Value, cancellationToken).ConfigureAwait(false);

            const string updateReverseSql = """
                UPDATE ACCT_FA_TRX_HDR
                   SET STATUS = 'REVERSED',
                       MODIFIED_BY = :p_user,
                       MODIFIED_DATE = SYSTIMESTAMP
                 WHERE IDDATA = :p_iddata
                   AND TRX_ID = :p_trx_id
                   AND STATUS = 'POSTED'
                """;
            CommandDefinition updateCmd = new(
                updateReverseSql,
                new
                {
                    p_user = request.UserId,
                    p_iddata = request.IdData,
                    p_trx_id = request.TransactionId
                },
                trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            int affected = await conn.ExecuteAsync(updateCmd).ConfigureAwait(false);
            if (affected == 0)
            {
                throw new DataException($"Gagal reverse transaksi {request.TransactionId}. Status telah berubah.");
            }

            await InsertAuditLogAsync(
                conn,
                trx,
                request.IdData,
                request.TransactionId,
                "REVERSE",
                request.UserId,
                "STATUS",
                "POSTED",
                "REVERSED",
                trxRow.DOC_NO,
                trxRow.PERIOD,
                string.IsNullOrWhiteSpace(request.Comment) ? $"Reverse journal {trxRow.NOJURNAL}" : request.Comment,
                cancellationToken).ConfigureAwait(false);

            await trx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await trx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<bool> ExistsActiveAssetAsync(string idData, long assetId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM ACCT_FA_ASSET
            WHERE IDDATA = :p_iddata
              AND ASSET_ID = :p_asset_id
              AND STATUS = 'ACTIVE'
              AND IS_DELETED = 'N'
            """;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        CommandDefinition cmd = new(sql, new { p_iddata = idData, p_asset_id = assetId }, commandType: CommandType.Text, cancellationToken: cancellationToken);
        int count = await conn.ExecuteScalarAsync<int>(cmd).ConfigureAwait(false);
        return count > 0;
    }

    private async Task ProcessApprovalActionAsync(ApprovalActionRequest request, string nextStatus, CancellationToken cancellationToken)
    {
        const string getTrxSql = """
            SELECT
                TRX_ID,
                IDDATA,
                DOC_NO,
                PERIOD,
                TRX_TYPE,
                STATUS,
                ASSET_ID,
                AMOUNT_BASE,
                OLD_AMOUNT_BASE,
                NEW_AMOUNT_BASE
            FROM ACCT_FA_TRX_HDR
            WHERE IDDATA = :p_iddata
              AND TRX_ID = :p_trx_id
            FOR UPDATE
            """;

        const string insertApprovalLog = """
            INSERT INTO ACCT_FA_APPROVAL_DTL
            (
                TRX_ID, STEP_NO, ROLE_CODE, STATUS, ACTION_BY, ACTION_DATE, ACTION_COMMENT, CREATED_DATE
            )
            VALUES
            (
                :p_trx_id,
                NVL((SELECT MAX(STEP_NO) FROM ACCT_FA_APPROVAL_DTL WHERE TRX_ID = :p_trx_id), 0) + 1,
                :p_role_code,
                :p_status,
                :p_user,
                SYSTIMESTAMP,
                :p_comment,
                SYSTIMESTAMP
            )
            """;

        const string updateHdr = """
            UPDATE ACCT_FA_TRX_HDR
               SET STATUS = :p_status,
                   MODIFIED_BY = :p_user,
                   MODIFIED_DATE = SYSTIMESTAMP
             WHERE IDDATA = :p_iddata
               AND TRX_ID = :p_trx_id
               AND STATUS = 'SUBMITTED'
            """;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using OracleTransaction trx = conn.BeginTransaction();
        try
        {
            CommandDefinition trxCmd = new(
                getTrxSql,
                new { p_iddata = request.IdData, p_trx_id = request.TransactionId },
                trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            LifecycleTrxRow? trxRow = await conn.QuerySingleOrDefaultAsync<LifecycleTrxRow>(trxCmd).ConfigureAwait(false);
            if (trxRow is null)
            {
                throw new DataException($"Transaction {request.TransactionId} tidak ditemukan.");
            }

            if (!string.Equals(trxRow.STATUS, "SUBMITTED", StringComparison.OrdinalIgnoreCase))
            {
                throw new DataException($"Transaction {request.TransactionId} tidak dalam status SUBMITTED.");
            }

            CommandDefinition logCmd = new(
                insertApprovalLog,
                new
                {
                    p_trx_id = request.TransactionId,
                    p_role_code = request.RoleCode,
                    p_status = nextStatus,
                    p_user = request.UserId,
                    p_comment = request.Comment
                },
                trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            await conn.ExecuteAsync(logCmd).ConfigureAwait(false);

            CommandDefinition updateHdrCmd = new(
                updateHdr,
                new
                {
                    p_status = nextStatus,
                    p_user = request.UserId,
                    p_iddata = request.IdData,
                    p_trx_id = request.TransactionId
                },
                trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            int affected = await conn.ExecuteAsync(updateHdrCmd).ConfigureAwait(false);
            if (affected == 0)
            {
                throw new DataException("Status transaksi berubah sebelum approval diproses.");
            }

            if (string.Equals(nextStatus, "APPROVED", StringComparison.OrdinalIgnoreCase))
            {
                await ApplyApprovedTransactionImpactAsync(conn, trx, trxRow, request.UserId, cancellationToken).ConfigureAwait(false);

                LifecycleJournalPostResult? postResult = await TryPostLifecycleJournalAsync(conn, trx, trxRow, request.UserId, cancellationToken).ConfigureAwait(false);
                if (postResult is not null)
                {
                    const string updatePostedSql = """
                        UPDATE ACCT_FA_TRX_HDR
                           SET STATUS = 'POSTED',
                               NOJURNAL = :p_nojurnal,
                               JURNALID = :p_jurnalid,
                               MODIFIED_BY = :p_user,
                               MODIFIED_DATE = SYSTIMESTAMP
                         WHERE IDDATA = :p_iddata
                           AND TRX_ID = :p_trx_id
                           AND STATUS = 'APPROVED'
                        """;
                    CommandDefinition postedCmd = new(
                        updatePostedSql,
                        new
                        {
                            p_nojurnal = postResult.NoJurnal,
                            p_jurnalid = postResult.JurnalId,
                            p_user = request.UserId,
                            p_iddata = trxRow.IDDATA,
                            p_trx_id = trxRow.TRX_ID
                        },
                        trx,
                        commandType: CommandType.Text,
                        cancellationToken: cancellationToken);
                    int postedAffected = await conn.ExecuteAsync(postedCmd).ConfigureAwait(false);
                    if (postedAffected == 0)
                    {
                        throw new DataException($"Gagal menandai transaksi {trxRow.TRX_ID} sebagai POSTED.");
                    }

                    await InsertAuditLogAsync(
                        conn,
                        trx,
                        request.IdData,
                        request.TransactionId,
                        "POST",
                        request.UserId,
                        "STATUS",
                        "APPROVED",
                        "POSTED",
                        trxRow.DOC_NO,
                        trxRow.PERIOD,
                        $"Auto-post journal {postResult.NoJurnal}",
                        cancellationToken).ConfigureAwait(false);
                }
            }

            await InsertAuditLogAsync(
                conn,
                trx,
                request.IdData,
                request.TransactionId,
                nextStatus,
                request.UserId,
                "STATUS",
                trxRow.STATUS,
                nextStatus,
                trxRow.DOC_NO,
                trxRow.PERIOD,
                string.IsNullOrWhiteSpace(request.Comment) ? nextStatus : request.Comment,
                cancellationToken).ConfigureAwait(false);
            await trx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await trx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private static async Task ApplyApprovedTransactionImpactAsync(
        OracleConnection conn,
        OracleTransaction trx,
        LifecycleTrxRow trxRow,
        string userId,
        CancellationToken cancellationToken)
    {
        if (!trxRow.ASSET_ID.HasValue || trxRow.ASSET_ID.Value <= 0)
        {
            return;
        }

        const string lockAssetSql = """
            SELECT
                ASSET_ID,
                IDDATA,
                STATUS,
                ACQUISITION_COST,
                IMPROVEMENT_TOTAL,
                REVALUATION_DELTA_TOTAL
            FROM ACCT_FA_ASSET
            WHERE IDDATA = :p_iddata
              AND ASSET_ID = :p_asset_id
              AND IS_DELETED = 'N'
            FOR UPDATE
            """;

        CommandDefinition lockCmd = new(
            lockAssetSql,
            new { p_iddata = trxRow.IDDATA, p_asset_id = trxRow.ASSET_ID.Value },
            trx,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        AssetValueRow? asset = await conn.QuerySingleOrDefaultAsync<AssetValueRow>(lockCmd).ConfigureAwait(false);
        if (asset is null)
        {
            throw new DataException($"Asset {trxRow.ASSET_ID.Value} tidak ditemukan.");
        }

        string trxType = trxRow.TRX_TYPE?.Trim().ToUpperInvariant() ?? string.Empty;
        switch (trxType)
        {
            case "IMPROVEMENT":
            {
                EnsureActiveAssetForMutation(asset.STATUS, trxType, asset.ASSET_ID);
                decimal amount = trxRow.AMOUNT_BASE;
                if (amount <= 0m)
                {
                    throw new DataException("Nilai improvement harus lebih besar dari nol.");
                }

                decimal newCost = asset.ACQUISITION_COST + amount;
                decimal newImprovementTotal = asset.IMPROVEMENT_TOTAL + amount;
                await UpdateAssetFinancialsAsync(
                    conn,
                    trx,
                    trxRow.IDDATA,
                    asset.ASSET_ID,
                    userId,
                    newCost,
                    newImprovementTotal,
                    asset.REVALUATION_DELTA_TOTAL,
                    cancellationToken).ConfigureAwait(false);

                await InsertAssetFieldAuditAsync(conn, trx, trxRow, userId, asset.ASSET_ID, "ACQUISITION_COST", asset.ACQUISITION_COST, newCost, cancellationToken).ConfigureAwait(false);
                await InsertAssetFieldAuditAsync(conn, trx, trxRow, userId, asset.ASSET_ID, "IMPROVEMENT_TOTAL", asset.IMPROVEMENT_TOTAL, newImprovementTotal, cancellationToken).ConfigureAwait(false);
                break;
            }
            case "REVALUATION":
            {
                EnsureActiveAssetForMutation(asset.STATUS, trxType, asset.ASSET_ID);
                decimal oldAmount = trxRow.OLD_AMOUNT_BASE ?? asset.ACQUISITION_COST;
                decimal newAmount = trxRow.NEW_AMOUNT_BASE ?? (oldAmount + trxRow.AMOUNT_BASE);
                if (newAmount <= 0m)
                {
                    throw new DataException("Nilai baru revaluation harus lebih besar dari nol.");
                }

                decimal delta = newAmount - oldAmount;
                decimal newRevalTotal = asset.REVALUATION_DELTA_TOTAL + delta;

                await UpdateAssetFinancialsAsync(
                    conn,
                    trx,
                    trxRow.IDDATA,
                    asset.ASSET_ID,
                    userId,
                    newAmount,
                    asset.IMPROVEMENT_TOTAL,
                    newRevalTotal,
                    cancellationToken).ConfigureAwait(false);

                await InsertAssetFieldAuditAsync(conn, trx, trxRow, userId, asset.ASSET_ID, "ACQUISITION_COST", asset.ACQUISITION_COST, newAmount, cancellationToken).ConfigureAwait(false);
                await InsertAssetFieldAuditAsync(conn, trx, trxRow, userId, asset.ASSET_ID, "REVALUATION_DELTA_TOTAL", asset.REVALUATION_DELTA_TOTAL, newRevalTotal, cancellationToken).ConfigureAwait(false);
                break;
            }
            case "FULL_DISPOSAL":
                EnsureActiveAssetForMutation(asset.STATUS, trxType, asset.ASSET_ID);
                await UpdateAssetStatusAsync(conn, trx, trxRow, userId, asset, "DISPOSED", cancellationToken).ConfigureAwait(false);
                break;
            case "SALE":
                EnsureActiveAssetForMutation(asset.STATUS, trxType, asset.ASSET_ID);
                await UpdateAssetStatusAsync(conn, trx, trxRow, userId, asset, "SOLD", cancellationToken).ConfigureAwait(false);
                break;
            case "WRITE_OFF":
                EnsureActiveAssetForMutation(asset.STATUS, trxType, asset.ASSET_ID);
                await UpdateAssetStatusAsync(conn, trx, trxRow, userId, asset, "WRITTEN_OFF", cancellationToken).ConfigureAwait(false);
                break;
        }
    }

    private static async Task<LifecycleJournalPostResult?> TryPostLifecycleJournalAsync(
        OracleConnection conn,
        OracleTransaction trx,
        LifecycleTrxRow trxRow,
        string userId,
        CancellationToken cancellationToken)
    {
        string trxType = trxRow.TRX_TYPE?.Trim().ToUpperInvariant() ?? string.Empty;
        if (trxType is not ("IMPROVEMENT" or "REVALUATION" or "FULL_DISPOSAL" or "WRITE_OFF" or "SALE"))
        {
            return null;
        }

        if (!trxRow.ASSET_ID.HasValue || trxRow.ASSET_ID.Value <= 0)
        {
            throw new DataException($"Transaction {trxRow.TRX_ID} tidak memiliki AssetId.");
        }

        string noJurnal = GenerateLifecycleNoJurnal(trxRow.PERIOD, trxRow.TRX_ID);
        double? existingJurnalId = await GetExistingLifecycleJurnalIdAsync(conn, trx, trxRow.IDDATA, trxRow.PERIOD, noJurnal, cancellationToken).ConfigureAwait(false);
        if (existingJurnalId.HasValue)
        {
            return new LifecycleJournalPostResult { NoJurnal = noJurnal, JurnalId = existingJurnalId };
        }

        LifecyclePostingContext context = await GetLifecyclePostingContextAsync(conn, trx, trxRow.IDDATA, trxRow.ASSET_ID.Value, cancellationToken).ConfigureAwait(false);
        List<LifecycleJournalLine> lines = BuildLifecycleJournalLines(trxRow, context);
        if (lines.Count == 0)
        {
            return null;
        }

        bool hasSumberColumn = await HasTmpSumberColumnAsync(conn, trx, cancellationToken).ConfigureAwait(false);
        await ClearTmpByScopeAsync(conn, trx, trxRow.IDDATA, trxRow.PERIOD, userId, cancellationToken).ConfigureAwait(false);
        await InsertLifecycleTmpAsync(conn, trx, trxRow, userId, noJurnal, lines, hasSumberColumn, cancellationToken).ConfigureAwait(false);

        AccountingPeriod period = AccountingPeriod.Parse(trxRow.PERIOD);
        int status = await ImportTmpToJurnalAsync(conn, trx, trxRow.IDDATA, period, userId, cancellationToken).ConfigureAwait(false);
        if (status is not 1 and not 99)
        {
            double? fallback = await GetExistingLifecycleJurnalIdAsync(conn, trx, trxRow.IDDATA, trxRow.PERIOD, noJurnal, cancellationToken).ConfigureAwait(false);
            if (!fallback.HasValue)
            {
                throw new DataException($"Posting jurnal lifecycle gagal. Status import: {status}");
            }

            return new LifecycleJournalPostResult { NoJurnal = noJurnal, JurnalId = fallback };
        }

        double? jurnalId = await GetExistingLifecycleJurnalIdAsync(conn, trx, trxRow.IDDATA, trxRow.PERIOD, noJurnal, cancellationToken).ConfigureAwait(false);
        return new LifecycleJournalPostResult { NoJurnal = noJurnal, JurnalId = jurnalId };
    }

    private static async Task<LifecyclePostingContext> GetLifecyclePostingContextAsync(
        OracleConnection conn,
        OracleTransaction trx,
        string idData,
        long assetId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                A.ASSET_ID,
                A.ASSET_CODE,
                A.ACQUISITION_COST,
                NVL(H.ACC_DEPR, 0) AS ACC_DEPR,
                M.ASSET_ACCT,
                M.ACC_DEPR_ACCT,
                M.GAIN_DISP_ACCT,
                M.LOSS_DISP_ACCT,
                M.REVAL_SURPLUS_ACCT,
                M.REVAL_DEFICIT_ACCT,
                M.CIP_ACCT
            FROM ACCT_FA_ASSET A
            JOIN (
                SELECT X.*
                FROM (
                    SELECT
                        MAP.*,
                        ROW_NUMBER() OVER (
                            PARTITION BY MAP.IDDATA, MAP.CATEGORY_ID
                            ORDER BY MAP.EFFECTIVE_FROM DESC, MAP.MAP_ID DESC
                        ) AS RN
                    FROM ACCT_FA_ACCOUNT_MAP MAP
                    WHERE MAP.IS_ACTIVE = 'Y'
                ) X
                WHERE X.RN = 1
            ) M
              ON M.IDDATA = A.IDDATA
             AND M.CATEGORY_ID = A.CATEGORY_ID
            LEFT JOIN (
                SELECT IDDATA, ASSET_ID, SUM(DEPR_AMOUNT) AS ACC_DEPR
                FROM ACCT_FA_DEPR_HISTORY
                GROUP BY IDDATA, ASSET_ID
            ) H
              ON H.IDDATA = A.IDDATA
             AND H.ASSET_ID = A.ASSET_ID
            WHERE A.IDDATA = :p_iddata
              AND A.ASSET_ID = :p_asset_id
            """;
        CommandDefinition cmd = new(
            sql,
            new { p_iddata = idData, p_asset_id = assetId },
            trx,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        LifecyclePostingRow? row = await conn.QuerySingleOrDefaultAsync<LifecyclePostingRow>(cmd).ConfigureAwait(false);
        if (row is null)
        {
            throw new DataException($"Context posting untuk asset {assetId} tidak ditemukan.");
        }

        return new LifecyclePostingContext
        {
            AssetId = row.ASSET_ID,
            AssetCode = row.ASSET_CODE,
            AcquisitionCost = row.ACQUISITION_COST,
            AccumulatedDepreciation = row.ACC_DEPR,
            AssetAccount = row.ASSET_ACCT,
            AccumulatedDepreciationAccount = row.ACC_DEPR_ACCT,
            GainOnDisposalAccount = row.GAIN_DISP_ACCT,
            LossOnDisposalAccount = row.LOSS_DISP_ACCT,
            RevaluationSurplusAccount = row.REVAL_SURPLUS_ACCT,
            RevaluationDeficitAccount = row.REVAL_DEFICIT_ACCT,
            ClearingAccount = row.CIP_ACCT
        };
    }

    private static List<LifecycleJournalLine> BuildLifecycleJournalLines(LifecycleTrxRow trxRow, LifecyclePostingContext context)
    {
        string trxType = trxRow.TRX_TYPE?.Trim().ToUpperInvariant() ?? string.Empty;
        List<LifecycleJournalLine> lines = new();

        switch (trxType)
        {
            case "IMPROVEMENT":
            {
                decimal amount = trxRow.AMOUNT_BASE;
                if (amount <= 0m)
                {
                    throw new DataException("Amount improvement harus lebih besar dari nol.");
                }

                EnsureAccount(context.AssetAccount, "ASSET_ACCT", trxType);
                EnsureAccount(context.ClearingAccount, "CIP_ACCT", trxType);
                lines.Add(new LifecycleJournalLine(context.AssetAccount, "Asset Account", amount, 0m));
                lines.Add(new LifecycleJournalLine(context.ClearingAccount, "CIP/Clearing Account", 0m, amount));
                break;
            }
            case "REVALUATION":
            {
                decimal oldAmount = trxRow.OLD_AMOUNT_BASE ?? context.AcquisitionCost;
                decimal newAmount = trxRow.NEW_AMOUNT_BASE ?? (oldAmount + trxRow.AMOUNT_BASE);
                decimal delta = newAmount - oldAmount;
                if (delta == 0m)
                {
                    return lines;
                }

                EnsureAccount(context.AssetAccount, "ASSET_ACCT", trxType);
                if (delta > 0m)
                {
                    EnsureAccount(context.RevaluationSurplusAccount, "REVAL_SURPLUS_ACCT", trxType);
                    lines.Add(new LifecycleJournalLine(context.AssetAccount, "Asset Account", delta, 0m));
                    lines.Add(new LifecycleJournalLine(context.RevaluationSurplusAccount, "Revaluation Surplus", 0m, delta));
                }
                else
                {
                    decimal absDelta = Math.Abs(delta);
                    EnsureAccount(context.RevaluationDeficitAccount, "REVAL_DEFICIT_ACCT", trxType);
                    lines.Add(new LifecycleJournalLine(context.RevaluationDeficitAccount, "Revaluation Deficit", absDelta, 0m));
                    lines.Add(new LifecycleJournalLine(context.AssetAccount, "Asset Account", 0m, absDelta));
                }

                break;
            }
            case "FULL_DISPOSAL":
            case "WRITE_OFF":
            {
                BuildDisposalLines(0m, trxType, context, lines);
                break;
            }
            case "SALE":
            {
                decimal proceeds = trxRow.AMOUNT_BASE;
                if (proceeds < 0m)
                {
                    throw new DataException("Amount sale tidak boleh negatif.");
                }

                BuildDisposalLines(proceeds, trxType, context, lines);
                break;
            }
        }

        ValidateBalanced(lines, trxType);
        return lines;
    }

    private static void BuildDisposalLines(decimal proceeds, string trxType, LifecyclePostingContext context, List<LifecycleJournalLine> lines)
    {
        EnsureAccount(context.AssetAccount, "ASSET_ACCT", trxType);
        EnsureAccount(context.AccumulatedDepreciationAccount, "ACC_DEPR_ACCT", trxType);
        EnsureAccount(context.GainOnDisposalAccount, "GAIN_DISP_ACCT", trxType);
        EnsureAccount(context.LossOnDisposalAccount, "LOSS_DISP_ACCT", trxType);

        decimal cost = context.AcquisitionCost;
        decimal accDep = Math.Min(context.AccumulatedDepreciation, cost);
        decimal netBookValue = cost - accDep;
        decimal gainOrLoss = proceeds - netBookValue;

        lines.Add(new LifecycleJournalLine(context.AccumulatedDepreciationAccount, "Accumulated Depreciation", accDep, 0m));
        if (proceeds > 0m)
        {
            EnsureAccount(context.ClearingAccount, "CIP_ACCT", trxType);
            lines.Add(new LifecycleJournalLine(context.ClearingAccount, "Disposal Clearing", proceeds, 0m));
        }

        if (gainOrLoss >= 0m)
        {
            if (gainOrLoss > 0m)
            {
                lines.Add(new LifecycleJournalLine(context.GainOnDisposalAccount, "Gain on Disposal", 0m, gainOrLoss));
            }
        }
        else
        {
            lines.Add(new LifecycleJournalLine(context.LossOnDisposalAccount, "Loss on Disposal", Math.Abs(gainOrLoss), 0m));
        }

        lines.Add(new LifecycleJournalLine(context.AssetAccount, "Asset Account", 0m, cost));
    }

    private static void ValidateBalanced(List<LifecycleJournalLine> lines, string trxType)
    {
        decimal debit = 0m;
        decimal credit = 0m;
        foreach (LifecycleJournalLine line in lines)
        {
            debit += line.Debit;
            credit += line.Credit;
        }

        if (Math.Round(debit - credit, 2, MidpointRounding.AwayFromZero) != 0m)
        {
            throw new DataException($"Jurnal {trxType} tidak balance. Debit={debit} Credit={credit}");
        }
    }

    private static void EnsureAccount(string account, string accountField, string trxType)
    {
        if (string.IsNullOrWhiteSpace(account))
        {
            throw new DataException($"Mapping account {accountField} belum diatur untuk transaksi {trxType}.");
        }
    }

    private static async Task InsertLifecycleTmpAsync(
        OracleConnection conn,
        OracleTransaction trx,
        LifecycleTrxRow trxRow,
        string userId,
        string noJurnal,
        IReadOnlyList<LifecycleJournalLine> lines,
        bool hasSumberColumn,
        CancellationToken cancellationToken)
    {
        string sql = hasSumberColumn
            ? """
              INSERT INTO ACCT_JURNAL_TMP
              (
                  NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT,
                  KETERANGAN, POSTED, PERIODE, IDDATA, USERID, GLYEAR, GLMONTH, SUMBER
              )
              VALUES
              (
                  :NOJURNAL, :TANGGAL, :BARIS, :KODE, :REKENING, :DEBET, :KREDIT,
                  :KETERANGAN, :POSTED, :PERIODE, :IDDATA, :USERID, :GLYEAR, :GLMONTH, :SUMBER
              )
              """
            : """
              INSERT INTO ACCT_JURNAL_TMP
              (
                  NOJURNAL, TANGGAL, BARIS, KODE, REKENING, DEBET, KREDIT,
                  KETERANGAN, POSTED, PERIODE, IDDATA, USERID, GLYEAR, GLMONTH
              )
              VALUES
              (
                  :NOJURNAL, :TANGGAL, :BARIS, :KODE, :REKENING, :DEBET, :KREDIT,
                  :KETERANGAN, :POSTED, :PERIODE, :IDDATA, :USERID, :GLYEAR, :GLMONTH
              )
              """;

        AccountingPeriod period = AccountingPeriod.Parse(trxRow.PERIOD);
        List<object> parameters = new(lines.Count);
        int baris = 1;
        foreach (LifecycleJournalLine line in lines)
        {
            parameters.Add(new
            {
                NOJURNAL = noJurnal,
                TANGGAL = period.EndDate,
                BARIS = baris++,
                KODE = line.AccountCode,
                REKENING = line.AccountName,
                DEBET = line.Debit,
                KREDIT = line.Credit,
                KETERANGAN = $"{trxRow.TRX_TYPE} {trxRow.DOC_NO}",
                POSTED = "TRUE",
                PERIODE = trxRow.PERIOD,
                IDDATA = trxRow.IDDATA,
                USERID = userId,
                GLYEAR = period.Year,
                GLMONTH = period.Month,
                SUMBER = "FA"
            });
        }

        CommandDefinition cmd = new(sql, parameters, trx, commandType: CommandType.Text, cancellationToken: cancellationToken);
        await conn.ExecuteAsync(cmd).ConfigureAwait(false);
    }

    private static async Task ClearTmpByScopeAsync(
        OracleConnection conn,
        OracleTransaction trx,
        string idData,
        string period,
        string userId,
        CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM ACCT_JURNAL_TMP WHERE IDDATA=:p_iddata AND PERIODE=:p_periode AND USERID=:p_userid";
        CommandDefinition cmd = new(sql, new { p_iddata = idData, p_periode = period, p_userid = userId }, trx, commandType: CommandType.Text, cancellationToken: cancellationToken);
        await conn.ExecuteAsync(cmd).ConfigureAwait(false);
    }

    private static async Task<bool> HasTmpSumberColumnAsync(
        OracleConnection conn,
        OracleTransaction trx,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM USER_TAB_COLUMNS
            WHERE TABLE_NAME = 'ACCT_JURNAL_TMP'
              AND COLUMN_NAME = 'SUMBER'
            """;
        CommandDefinition cmd = new(sql, transaction: trx, commandType: CommandType.Text, cancellationToken: cancellationToken);
        int count = await conn.ExecuteScalarAsync<int>(cmd).ConfigureAwait(false);
        return count > 0;
    }

    private static async Task<int> ImportTmpToJurnalAsync(
        OracleConnection conn,
        OracleTransaction trx,
        string idData,
        AccountingPeriod period,
        string userId,
        CancellationToken cancellationToken)
    {
        await using OracleCommand cmd = new("ACCT_JURNAL_IMPORT_V2.ImportJurnalParsial", conn)
        {
            Transaction = trx,
            CommandType = CommandType.StoredProcedure,
            BindByName = true
        };
        cmd.Parameters.Add("ISSUKSES", OracleDbType.Int16).Direction = ParameterDirection.ReturnValue;
        cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = idData;
        cmd.Parameters.Add(":p_bulan", OracleDbType.Int16).Value = period.Month;
        cmd.Parameters.Add(":p_tahun", OracleDbType.Int16).Value = period.Year;
        cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = period.PeriodeString;
        cmd.Parameters.Add(":p_userid", OracleDbType.Varchar2, 20).Value = userId;
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(cmd.Parameters["ISSUKSES"].Value);
    }

    private static async Task<double?> GetExistingLifecycleJurnalIdAsync(
        OracleConnection conn,
        OracleTransaction trx,
        string idData,
        string period,
        string noJurnal,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT JURNALID
            FROM ACCT_JURNAL_HDR
            WHERE IDDATA = :p_iddata
              AND PERIODE = :p_periode
              AND NOJURNAL = :p_nojurnal
            ORDER BY NVL(MODIFIED_DATE, CREATED_DATE) DESC, JURNALID DESC
            FETCH FIRST 1 ROWS ONLY
            """;
        CommandDefinition cmd = new(
            sql,
            new { p_iddata = idData, p_periode = period, p_nojurnal = noJurnal },
            trx,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        return await conn.ExecuteScalarAsync<double?>(cmd).ConfigureAwait(false);
    }

    private static async Task<bool> IsPeriodLockedAsync(
        OracleConnection conn,
        OracleTransaction trx,
        string idData,
        string period,
        CancellationToken cancellationToken)
    {
        await using OracleCommand cmd = new("ACCT_JURNAL.GetStatusLock", conn)
        {
            Transaction = trx,
            CommandType = CommandType.StoredProcedure,
            BindByName = true
        };
        cmd.Parameters.Add("LockStatus", OracleDbType.Varchar2, 20).Direction = ParameterDirection.ReturnValue;
        cmd.Parameters.Add(":p_IDDATA", OracleDbType.Varchar2, 20).Value = idData;
        cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = period;
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        string result = Convert.ToString(cmd.Parameters["LockStatus"].Value) ?? "N";
        return string.Equals(result.Trim(), "Y", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task DeleteJurnalByIdAsync(
        OracleConnection conn,
        OracleTransaction trx,
        string idData,
        string period,
        double jurnalId,
        CancellationToken cancellationToken)
    {
        const string deleteDtlSql = "DELETE FROM ACCT_JURNAL_DTL WHERE REFFID = :p_jurnalid";
        const string deleteHdrSql = """
            DELETE FROM ACCT_JURNAL_HDR
            WHERE JURNALID = :p_jurnalid
              AND IDDATA = :p_iddata
              AND PERIODE = :p_periode
            """;

        CommandDefinition dtlCmd = new(
            deleteDtlSql,
            new { p_jurnalid = jurnalId },
            trx,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        await conn.ExecuteAsync(dtlCmd).ConfigureAwait(false);

        CommandDefinition hdrCmd = new(
            deleteHdrSql,
            new { p_jurnalid = jurnalId, p_iddata = idData, p_periode = period },
            trx,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        int hdrAffected = await conn.ExecuteAsync(hdrCmd).ConfigureAwait(false);
        if (hdrAffected == 0)
        {
            throw new DataException($"Jurnal {jurnalId} tidak ditemukan untuk dihapus.");
        }
    }

    private static string GenerateLifecycleNoJurnal(string period, long trxId)
    {
        AccountingPeriod p = AccountingPeriod.Parse(period);
        return $"FA/{p.Year:0000}{p.Month:00}/TX{trxId:000000}";
    }

    private static void EnsureActiveAssetForMutation(string assetStatus, string trxType, long assetId)
    {
        if (!string.Equals(assetStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            throw new DataException($"Asset {assetId} status {assetStatus} tidak valid untuk transaksi {trxType}.");
        }
    }

    private static async Task UpdateAssetFinancialsAsync(
        OracleConnection conn,
        OracleTransaction trx,
        string idData,
        long assetId,
        string userId,
        decimal acquisitionCost,
        decimal improvementTotal,
        decimal revaluationDeltaTotal,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE ACCT_FA_ASSET
               SET ACQUISITION_COST = :p_acquisition_cost,
                   IMPROVEMENT_TOTAL = :p_improvement_total,
                   REVALUATION_DELTA_TOTAL = :p_revaluation_total,
                   MODIFIED_BY = :p_user,
                   MODIFIED_DATE = SYSTIMESTAMP
             WHERE IDDATA = :p_iddata
               AND ASSET_ID = :p_asset_id
               AND IS_DELETED = 'N'
            """;
        CommandDefinition cmd = new(
            sql,
            new
            {
                p_acquisition_cost = acquisitionCost,
                p_improvement_total = improvementTotal,
                p_revaluation_total = revaluationDeltaTotal,
                p_user = userId,
                p_iddata = idData,
                p_asset_id = assetId
            },
            trx,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        int affected = await conn.ExecuteAsync(cmd).ConfigureAwait(false);
        if (affected == 0)
        {
            throw new DataException($"Asset {assetId} gagal diperbarui.");
        }
    }

    private static async Task UpdateAssetStatusAsync(
        OracleConnection conn,
        OracleTransaction trx,
        LifecycleTrxRow trxRow,
        string userId,
        AssetValueRow asset,
        string nextAssetStatus,
        CancellationToken cancellationToken)
    {
        if (string.Equals(asset.STATUS, nextAssetStatus, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        const string sql = """
            UPDATE ACCT_FA_ASSET
               SET STATUS = :p_status,
                   MODIFIED_BY = :p_user,
                   MODIFIED_DATE = SYSTIMESTAMP
             WHERE IDDATA = :p_iddata
               AND ASSET_ID = :p_asset_id
               AND IS_DELETED = 'N'
            """;
        CommandDefinition cmd = new(
            sql,
            new
            {
                p_status = nextAssetStatus,
                p_user = userId,
                p_iddata = trxRow.IDDATA,
                p_asset_id = asset.ASSET_ID
            },
            trx,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        int affected = await conn.ExecuteAsync(cmd).ConfigureAwait(false);
        if (affected == 0)
        {
            throw new DataException($"Asset {asset.ASSET_ID} gagal diubah statusnya.");
        }

        await InsertAssetFieldAuditAsync(
            conn,
            trx,
            trxRow,
            userId,
            asset.ASSET_ID,
            "STATUS",
            asset.STATUS,
            nextAssetStatus,
            cancellationToken).ConfigureAwait(false);
    }

    private static string MapType(FixedAssetTransactionType type)
    {
        return type switch
        {
            FixedAssetTransactionType.Acquisition => "ACQUISITION",
            FixedAssetTransactionType.Activation => "ACTIVATION",
            FixedAssetTransactionType.Transfer => "TRANSFER",
            FixedAssetTransactionType.Revaluation => "REVALUATION",
            FixedAssetTransactionType.Improvement => "IMPROVEMENT",
            FixedAssetTransactionType.Impairment => "IMPAIRMENT",
            FixedAssetTransactionType.PartialDisposal => "PARTIAL_DISPOSAL",
            FixedAssetTransactionType.FullDisposal => "FULL_DISPOSAL",
            FixedAssetTransactionType.Sale => "SALE",
            FixedAssetTransactionType.WriteOff => "WRITE_OFF",
            FixedAssetTransactionType.Retirement => "RETIREMENT",
            FixedAssetTransactionType.Reclassification => "RECLASSIFICATION",
            FixedAssetTransactionType.CipCapitalization => "CIP_CAPITALIZATION",
            FixedAssetTransactionType.Depreciation => "DEPRECIATION",
            _ => "UNKNOWN"
        };
    }

    private static async Task InsertAuditLogAsync(
        OracleConnection conn,
        OracleTransaction trx,
        string idData,
        long trxId,
        string actionType,
        string userId,
        string fieldName,
        string oldValue,
        string newValue,
        string docNo,
        string period,
        string comment,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO ACCT_FA_AUDIT_LOG
            (
                IDDATA, ENTITY_NAME, ENTITY_ID, ACTION_TYPE, FIELD_NAME, OLD_VALUE, NEW_VALUE, DOC_NO, PERIOD, ACTION_BY, ACTION_TS
            )
            SELECT
                :p_iddata,
                'ACCT_FA_TRX_HDR',
                :p_entity_id,
                :p_action_type,
                :p_field_name,
                :p_old_value,
                :p_new_value,
                NVL(NULLIF(:p_doc_no, ''), T.DOC_NO),
                NVL(NULLIF(:p_period, ''), T.PERIOD),
                :p_user,
                SYSTIMESTAMP
            FROM ACCT_FA_TRX_HDR T
            WHERE T.IDDATA = :p_iddata
              AND T.TRX_ID = :p_trx_id
            """;
        CommandDefinition cmd = new(
            sql,
            new
            {
                p_iddata = idData,
                p_trx_id = trxId,
                p_entity_id = trxId.ToString(),
                p_action_type = actionType,
                p_field_name = fieldName,
                p_old_value = string.IsNullOrWhiteSpace(oldValue) ? comment : oldValue,
                p_new_value = string.IsNullOrWhiteSpace(newValue) ? comment : newValue,
                p_doc_no = docNo,
                p_period = period,
                p_user = userId
            },
            trx,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        await conn.ExecuteAsync(cmd).ConfigureAwait(false);
    }

    private static async Task InsertAssetFieldAuditAsync(
        OracleConnection conn,
        OracleTransaction trx,
        LifecycleTrxRow trxRow,
        string userId,
        long assetId,
        string fieldName,
        object oldValue,
        object newValue,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO ACCT_FA_AUDIT_LOG
            (
                IDDATA, ENTITY_NAME, ENTITY_ID, ACTION_TYPE, FIELD_NAME, OLD_VALUE, NEW_VALUE, DOC_NO, PERIOD, ACTION_BY, ACTION_TS
            )
            VALUES
            (
                :p_iddata, 'ACCT_FA_ASSET', :p_entity_id, 'UPDATE', :p_field_name, :p_old_value, :p_new_value, :p_doc_no, :p_period, :p_user, SYSTIMESTAMP
            )
            """;
        CommandDefinition cmd = new(
            sql,
            new
            {
                p_iddata = trxRow.IDDATA,
                p_entity_id = assetId.ToString(),
                p_field_name = fieldName,
                p_old_value = Convert.ToString(oldValue),
                p_new_value = Convert.ToString(newValue),
                p_doc_no = trxRow.DOC_NO,
                p_period = trxRow.PERIOD,
                p_user = userId
            },
            trx,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        await conn.ExecuteAsync(cmd).ConfigureAwait(false);
    }

    private sealed class LifecycleTrxRow
    {
        public long TRX_ID { get; init; }
        public string IDDATA { get; init; } = string.Empty;
        public string DOC_NO { get; init; } = string.Empty;
        public string PERIOD { get; init; } = string.Empty;
        public string TRX_TYPE { get; init; } = string.Empty;
        public string STATUS { get; init; } = string.Empty;
        public long? ASSET_ID { get; init; }
        public decimal AMOUNT_BASE { get; init; }
        public decimal? OLD_AMOUNT_BASE { get; init; }
        public decimal? NEW_AMOUNT_BASE { get; init; }
        public string NOJURNAL { get; init; } = string.Empty;
        public double? JURNALID { get; init; }
    }

    private sealed class AssetValueRow
    {
        public long ASSET_ID { get; init; }
        public string IDDATA { get; init; } = string.Empty;
        public string STATUS { get; init; } = string.Empty;
        public decimal ACQUISITION_COST { get; init; }
        public decimal IMPROVEMENT_TOTAL { get; init; }
        public decimal REVALUATION_DELTA_TOTAL { get; init; }
    }

    private sealed class LifecyclePostingRow
    {
        public long ASSET_ID { get; init; }
        public string ASSET_CODE { get; init; } = string.Empty;
        public decimal ACQUISITION_COST { get; init; }
        public decimal ACC_DEPR { get; init; }
        public string ASSET_ACCT { get; init; } = string.Empty;
        public string ACC_DEPR_ACCT { get; init; } = string.Empty;
        public string GAIN_DISP_ACCT { get; init; } = string.Empty;
        public string LOSS_DISP_ACCT { get; init; } = string.Empty;
        public string REVAL_SURPLUS_ACCT { get; init; } = string.Empty;
        public string REVAL_DEFICIT_ACCT { get; init; } = string.Empty;
        public string CIP_ACCT { get; init; } = string.Empty;
    }

    private sealed class LifecyclePostingContext
    {
        public long AssetId { get; init; }
        public string AssetCode { get; init; } = string.Empty;
        public decimal AcquisitionCost { get; init; }
        public decimal AccumulatedDepreciation { get; init; }
        public string AssetAccount { get; init; } = string.Empty;
        public string AccumulatedDepreciationAccount { get; init; } = string.Empty;
        public string GainOnDisposalAccount { get; init; } = string.Empty;
        public string LossOnDisposalAccount { get; init; } = string.Empty;
        public string RevaluationSurplusAccount { get; init; } = string.Empty;
        public string RevaluationDeficitAccount { get; init; } = string.Empty;
        public string ClearingAccount { get; init; } = string.Empty;
    }

    private sealed record LifecycleJournalLine(string AccountCode, string AccountName, decimal Debit, decimal Credit);

    private sealed class LifecycleJournalPostResult
    {
        public string NoJurnal { get; init; } = string.Empty;
        public double? JurnalId { get; init; }
    }
}
