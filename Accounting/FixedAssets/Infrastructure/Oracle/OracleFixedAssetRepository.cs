using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Contracts;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Domain;
using Accounting.FixedAssets.Domain.Entities;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.FixedAssets.Infrastructure.Oracle;

public sealed class OracleFixedAssetRepository : IFixedAssetRepository
{
    private readonly string _connectionString;

    public OracleFixedAssetRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<FixedAsset>> GetDepreciableAssetsAsync(string idData, AccountingPeriod period, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                A.ASSET_ID,
                A.IDDATA,
                A.ASSET_CODE,
                A.ASSET_NAME,
                A.CATEGORY_ID,
                A.GROUP_ID,
                A.ACQUISITION_DATE,
                A.IN_SERVICE_DATE,
                A.DEPRECIATION_START_DATE,
                A.ACQUISITION_COST,
                A.RESIDUAL_VALUE,
                A.USEFUL_LIFE_MONTHS,
                A.DEPR_METHOD,
                A.STATUS,
                A.CURRENCY_CODE,
                NVL(A.EXCHANGE_RATE, 1) AS EXCHANGE_RATE,
                NVL(A.IMPROVEMENT_TOTAL, 0) AS IMPROVEMENT_TOTAL,
                NVL(A.REVALUATION_DELTA_TOTAL, 0) AS REVALUATION_DELTA_TOTAL,
                NVL(A.IMPAIRMENT_TOTAL, 0) AS IMPAIRMENT_TOTAL,
                NVL(H.ACC_DEPR_POSTED, 0) AS ACC_DEPR_POSTED,
                MAP.DEPR_EXP_ACCT,
                MAP.ACC_DEPR_ACCT
            FROM ACCT_FA_ASSET A
            JOIN (
                SELECT M.*
                FROM (
                    SELECT
                        MAP.*,
                        ROW_NUMBER() OVER (
                            PARTITION BY MAP.IDDATA, MAP.CATEGORY_ID
                            ORDER BY MAP.EFFECTIVE_FROM DESC, MAP.MAP_ID DESC
                        ) AS RN
                    FROM ACCT_FA_ACCOUNT_MAP MAP
                    WHERE MAP.IS_ACTIVE = 'Y'
                ) M
                WHERE M.RN = 1
            ) MAP
              ON MAP.IDDATA = A.IDDATA
             AND MAP.CATEGORY_ID = A.CATEGORY_ID
            LEFT JOIN (
                SELECT IDDATA, ASSET_ID, SUM(DEPR_AMOUNT) AS ACC_DEPR_POSTED
                FROM ACCT_FA_DEPR_HISTORY
                WHERE IDDATA = :p_iddata
                  AND PERIOD_KEY <= :p_period_key
                GROUP BY IDDATA, ASSET_ID
            ) H
              ON H.IDDATA = A.IDDATA
             AND H.ASSET_ID = A.ASSET_ID
            WHERE A.IDDATA = :p_iddata
              AND A.IS_DELETED = 'N'
              AND A.STATUS = 'ACTIVE'
            ORDER BY A.ASSET_CODE
            """;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        CommandDefinition command = new(
            sql,
            new
            {
                p_iddata = idData,
                p_period_key = GetPeriodKey(period)
            },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);

        IEnumerable<FixedAssetRow> rows = await conn.QueryAsync<FixedAssetRow>(command).ConfigureAwait(false);
        return rows.Select(MapAsset).ToList();
    }

    public async Task<long> CreateDepreciationRunAsync(string idData, AccountingPeriod period, string userId, decimal totalAmount, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO ACCT_FA_DEPR_RUN
            (
                IDDATA, PERIOD, PERIOD_KEY, RUN_NO, STATUS,
                TOTAL_AMOUNT, CREATED_BY, CREATED_DATE
            )
            VALUES
            (
                :p_iddata, :p_period, :p_period_key, :p_run_no, 'DRAFT',
                :p_total_amount, :p_user_id, SYSTIMESTAMP
            )
            RETURNING RUN_ID INTO :p_run_id
            """;

        string runNo = $"FADEP/{period.Year:0000}{period.Month:00}/{DateTime.UtcNow:HHmmssfff}";

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        DynamicParameters p = new();
        p.Add("p_iddata", idData);
        p.Add("p_period", period.PeriodeString);
        p.Add("p_period_key", GetPeriodKey(period));
        p.Add("p_run_no", runNo);
        p.Add("p_total_amount", totalAmount);
        p.Add("p_user_id", userId);
        p.Add("p_run_id", dbType: DbType.Int64, direction: ParameterDirection.Output);

        CommandDefinition cmd = new(sql, p, commandType: CommandType.Text, cancellationToken: cancellationToken);
        await conn.ExecuteAsync(cmd).ConfigureAwait(false);
        return p.Get<long>("p_run_id");
    }

    public async Task SaveDepreciationRunLinesAsync(long runId, IReadOnlyList<DepreciationPreviewLine> lines, string userId, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO ACCT_FA_DEPR_RUN_DTL
            (
                RUN_ID, BARIS, ASSET_ID, ASSET_CODE, PERIOD,
                OPENING_NBV, DEPR_AMOUNT, CLOSING_NBV, RESIDUAL_VALUE,
                DEPR_EXP_ACCT, ACC_DEPR_ACCT, DESCRIPTION, CREATED_BY, CREATED_DATE
            )
            VALUES
            (
                :RUN_ID, :BARIS, :ASSET_ID, :ASSET_CODE, :PERIOD,
                :OPENING_NBV, :DEPR_AMOUNT, :CLOSING_NBV, :RESIDUAL_VALUE,
                :DEPR_EXP_ACCT, :ACC_DEPR_ACCT, :DESCRIPTION, :CREATED_BY, SYSTIMESTAMP
            )
            """;

        if (lines.Count == 0)
        {
            return;
        }

        List<object> parameters = new(lines.Count);
        int baris = 1;
        foreach (DepreciationPreviewLine line in lines)
        {
            parameters.Add(new
            {
                RUN_ID = runId,
                BARIS = baris++,
                ASSET_ID = line.AssetId,
                ASSET_CODE = line.AssetCode,
                PERIOD = line.Period,
                OPENING_NBV = line.OpeningNetBookValue,
                DEPR_AMOUNT = line.DepreciationAmount,
                CLOSING_NBV = line.ClosingNetBookValue,
                RESIDUAL_VALUE = line.ResidualValue,
                DEPR_EXP_ACCT = line.DepreciationExpenseAccount,
                ACC_DEPR_ACCT = line.AccumulatedDepreciationAccount,
                DESCRIPTION = line.Description,
                CREATED_BY = userId
            });
        }

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        CommandDefinition cmd = new(sql, parameters, commandType: CommandType.Text, cancellationToken: cancellationToken);
        await conn.ExecuteAsync(cmd).ConfigureAwait(false);
    }

    public async Task<DepreciationRunSnapshot?> GetDepreciationRunSnapshotAsync(long runId, string idData, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                RUN_ID,
                IDDATA,
                PERIOD,
                STATUS,
                NVL(NOJURNAL, '') AS NOJURNAL,
                JURNALID
            FROM ACCT_FA_DEPR_RUN
            WHERE RUN_ID = :p_run_id
              AND IDDATA = :p_iddata
            """;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        CommandDefinition cmd = new(
            sql,
            new { p_run_id = runId, p_iddata = idData },
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        RunSnapshotRow? row = await conn.QuerySingleOrDefaultAsync<RunSnapshotRow>(cmd).ConfigureAwait(false);
        if (row is null)
        {
            return null;
        }

        return new DepreciationRunSnapshot
        {
            RunId = row.RUN_ID,
            IdData = row.IDDATA,
            Period = row.PERIOD,
            Status = row.STATUS,
            NoJurnal = row.NOJURNAL,
            JurnalId = row.JURNALID
        };
    }

    public async Task<IReadOnlyList<DepreciationPreviewLine>> GetDepreciationRunLinesAsync(long runId, string idData, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                D.ASSET_ID,
                D.ASSET_CODE,
                D.PERIOD,
                D.OPENING_NBV,
                D.DEPR_AMOUNT,
                D.CLOSING_NBV,
                D.RESIDUAL_VALUE,
                D.DEPR_EXP_ACCT,
                D.ACC_DEPR_ACCT,
                D.DESCRIPTION
            FROM ACCT_FA_DEPR_RUN_DTL D
            JOIN ACCT_FA_DEPR_RUN R
              ON R.RUN_ID = D.RUN_ID
            WHERE D.RUN_ID = :p_run_id
              AND R.IDDATA = :p_iddata
            ORDER BY D.BARIS
            """;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        CommandDefinition cmd = new(sql, new { p_run_id = runId, p_iddata = idData }, commandType: CommandType.Text, cancellationToken: cancellationToken);
        IEnumerable<RunLineRow> rows = await conn.QueryAsync<RunLineRow>(cmd).ConfigureAwait(false);

        AccountingPeriod p = AccountingPeriod.Parse(rows.FirstOrDefault()?.PERIOD ?? "01/1900");
        return rows.Select(x => new DepreciationPreviewLine
        {
            AssetId = x.ASSET_ID,
            AssetCode = x.ASSET_CODE,
            Period = x.PERIOD,
            PeriodStartDate = p.StartDate,
            PeriodEndDate = p.EndDate,
            OpeningNetBookValue = x.OPENING_NBV,
            DepreciationAmount = x.DEPR_AMOUNT,
            ClosingNetBookValue = x.CLOSING_NBV,
            ResidualValue = x.RESIDUAL_VALUE,
            DepreciationExpenseAccount = x.DEPR_EXP_ACCT,
            AccumulatedDepreciationAccount = x.ACC_DEPR_ACCT,
            Description = x.DESCRIPTION
        }).ToList();
    }

    public async Task MarkDepreciationRunPostedAsync(long runId, string noJurnal, double? jurnalId, string userId, CancellationToken cancellationToken)
    {
        const string updateRunSql = """
            UPDATE ACCT_FA_DEPR_RUN
               SET STATUS = 'POSTED',
                   NOJURNAL = :p_nojurnal,
                   JURNALID = :p_jurnalid,
                   POSTED_BY = :p_user,
                   POSTED_DATE = SYSTIMESTAMP,
                   MODIFIED_BY = :p_user,
                   MODIFIED_DATE = SYSTIMESTAMP
             WHERE RUN_ID = :p_run_id
               AND STATUS = 'DRAFT'
            """;

        const string insertHistorySql = """
            INSERT INTO ACCT_FA_DEPR_HISTORY
            (
                IDDATA, ASSET_ID, PERIOD, PERIOD_KEY, RUN_ID,
                DEPR_AMOUNT, OPENING_NBV, CLOSING_NBV,
                NOJURNAL, JURNALID, CREATED_BY, CREATED_DATE
            )
            SELECT
                R.IDDATA,
                D.ASSET_ID,
                R.PERIOD,
                R.PERIOD_KEY,
                D.RUN_ID,
                D.DEPR_AMOUNT,
                D.OPENING_NBV,
                D.CLOSING_NBV,
                :p_nojurnal,
                :p_jurnalid,
                :p_user,
                SYSTIMESTAMP
            FROM ACCT_FA_DEPR_RUN_DTL D
            JOIN ACCT_FA_DEPR_RUN R
              ON R.RUN_ID = D.RUN_ID
            WHERE D.RUN_ID = :p_run_id
              AND NOT EXISTS (
                    SELECT 1
                    FROM ACCT_FA_DEPR_HISTORY H
                    WHERE H.RUN_ID = D.RUN_ID
                      AND H.ASSET_ID = D.ASSET_ID
                      AND H.PERIOD = R.PERIOD
              )
            """;

        const string insertAuditSql = """
            INSERT INTO ACCT_FA_AUDIT_LOG
            (
                IDDATA, ENTITY_NAME, ENTITY_ID, ACTION_TYPE, DOC_NO, PERIOD, ACTION_BY, ACTION_TS
            )
            SELECT
                R.IDDATA,
                'ACCT_FA_DEPR_RUN',
                TO_CHAR(R.RUN_ID),
                'POST',
                :p_nojurnal,
                R.PERIOD,
                :p_user,
                SYSTIMESTAMP
            FROM ACCT_FA_DEPR_RUN R
            WHERE R.RUN_ID = :p_run_id
            """;

        const string runStateSql = """
            SELECT
                STATUS,
                NVL(NOJURNAL, '') AS NOJURNAL
            FROM ACCT_FA_DEPR_RUN
            WHERE RUN_ID = :p_run_id
            """;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using OracleTransaction trx = conn.BeginTransaction();
        try
        {
            CommandDefinition updateCmd = new(
                updateRunSql,
                new { p_run_id = runId, p_nojurnal = noJurnal, p_jurnalid = jurnalId, p_user = userId },
                transaction: trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            int affected = await conn.ExecuteAsync(updateCmd).ConfigureAwait(false);
            if (affected == 0)
            {
                CommandDefinition stateCmd = new(
                    runStateSql,
                    new { p_run_id = runId },
                    transaction: trx,
                    commandType: CommandType.Text,
                    cancellationToken: cancellationToken);
                RunStateRow? state = await conn.QuerySingleOrDefaultAsync<RunStateRow>(stateCmd).ConfigureAwait(false);
                if (state is not null
                    && string.Equals(state.STATUS, "POSTED", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(state.NOJURNAL, noJurnal, StringComparison.OrdinalIgnoreCase))
                {
                    await trx.CommitAsync(cancellationToken).ConfigureAwait(false);
                    return;
                }

                throw new InvalidOperationException($"Run {runId} tidak ditemukan atau status bukan DRAFT.");
            }

            CommandDefinition insertCmd = new(
                insertHistorySql,
                new { p_run_id = runId, p_nojurnal = noJurnal, p_jurnalid = jurnalId, p_user = userId },
                transaction: trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            await conn.ExecuteAsync(insertCmd).ConfigureAwait(false);

            CommandDefinition auditCmd = new(
                insertAuditSql,
                new { p_run_id = runId, p_nojurnal = noJurnal, p_user = userId },
                transaction: trx,
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);
            await conn.ExecuteAsync(auditCmd).ConfigureAwait(false);

            await trx.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await trx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private static int GetPeriodKey(AccountingPeriod period) => period.Year * 100 + period.Month;

    private static FixedAsset MapAsset(FixedAssetRow row)
    {
        return new FixedAsset
        {
            AssetId = row.ASSET_ID,
            IdData = row.IDDATA,
            AssetCode = row.ASSET_CODE,
            AssetName = row.ASSET_NAME,
            CategoryId = row.CATEGORY_ID,
            GroupId = row.GROUP_ID,
            AcquisitionDate = row.ACQUISITION_DATE,
            InServiceDate = row.IN_SERVICE_DATE,
            DepreciationStartDate = row.DEPRECIATION_START_DATE,
            AcquisitionCost = row.ACQUISITION_COST,
            ResidualValue = row.RESIDUAL_VALUE,
            UsefulLifeMonths = row.USEFUL_LIFE_MONTHS,
            DepreciationMethod = MapMethod(row.DEPR_METHOD),
            Status = MapStatus(row.STATUS),
            CurrencyCode = row.CURRENCY_CODE,
            ExchangeRate = row.EXCHANGE_RATE,
            TotalImprovementAmount = row.IMPROVEMENT_TOTAL,
            TotalRevaluationDelta = row.REVALUATION_DELTA_TOTAL,
            TotalImpairmentAmount = row.IMPAIRMENT_TOTAL,
            AccumulatedDepreciationPosted = row.ACC_DEPR_POSTED,
            DepreciationExpenseAccount = row.DEPR_EXP_ACCT,
            AccumulatedDepreciationAccount = row.ACC_DEPR_ACCT
        };
    }

    private static DepreciationMethod MapMethod(string method)
    {
        return method?.Trim().ToUpperInvariant() switch
        {
            "SL" => DepreciationMethod.StraightLine,
            "DB" => DepreciationMethod.DecliningBalance,
            _ => DepreciationMethod.NoDepreciation
        };
    }

    private static FixedAssetStatus MapStatus(string status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "DRAFT" => FixedAssetStatus.Draft,
            "ACTIVE" => FixedAssetStatus.Active,
            "UNDER_CONSTRUCTION" => FixedAssetStatus.UnderConstruction,
            "DISPOSED" => FixedAssetStatus.Disposed,
            "SOLD" => FixedAssetStatus.Sold,
            "TRANSFERRED" => FixedAssetStatus.Transferred,
            "WRITTEN_OFF" => FixedAssetStatus.WrittenOff,
            "RETIRED" => FixedAssetStatus.Retired,
            _ => FixedAssetStatus.Draft
        };
    }

    private sealed class FixedAssetRow
    {
        public long ASSET_ID { get; init; }
        public string IDDATA { get; init; } = string.Empty;
        public string ASSET_CODE { get; init; } = string.Empty;
        public string ASSET_NAME { get; init; } = string.Empty;
        public long CATEGORY_ID { get; init; }
        public long GROUP_ID { get; init; }
        public DateTime ACQUISITION_DATE { get; init; }
        public DateTime? IN_SERVICE_DATE { get; init; }
        public DateTime? DEPRECIATION_START_DATE { get; init; }
        public decimal ACQUISITION_COST { get; init; }
        public decimal RESIDUAL_VALUE { get; init; }
        public int USEFUL_LIFE_MONTHS { get; init; }
        public string DEPR_METHOD { get; init; } = string.Empty;
        public string STATUS { get; init; } = string.Empty;
        public string CURRENCY_CODE { get; init; } = "IDR";
        public decimal EXCHANGE_RATE { get; init; }
        public decimal IMPROVEMENT_TOTAL { get; init; }
        public decimal REVALUATION_DELTA_TOTAL { get; init; }
        public decimal IMPAIRMENT_TOTAL { get; init; }
        public decimal ACC_DEPR_POSTED { get; init; }
        public string DEPR_EXP_ACCT { get; init; } = string.Empty;
        public string ACC_DEPR_ACCT { get; init; } = string.Empty;
    }

    private sealed class RunLineRow
    {
        public long ASSET_ID { get; init; }
        public string ASSET_CODE { get; init; } = string.Empty;
        public string PERIOD { get; init; } = string.Empty;
        public decimal OPENING_NBV { get; init; }
        public decimal DEPR_AMOUNT { get; init; }
        public decimal CLOSING_NBV { get; init; }
        public decimal RESIDUAL_VALUE { get; init; }
        public string DEPR_EXP_ACCT { get; init; } = string.Empty;
        public string ACC_DEPR_ACCT { get; init; } = string.Empty;
        public string DESCRIPTION { get; init; } = string.Empty;
    }

    private sealed class RunSnapshotRow
    {
        public long RUN_ID { get; init; }
        public string IDDATA { get; init; } = string.Empty;
        public string PERIOD { get; init; } = string.Empty;
        public string STATUS { get; init; } = string.Empty;
        public string NOJURNAL { get; init; } = string.Empty;
        public double? JURNALID { get; init; }
    }

    private sealed class RunStateRow
    {
        public string STATUS { get; init; } = string.Empty;
        public string NOJURNAL { get; init; } = string.Empty;
    }
}
