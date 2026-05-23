using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Contracts;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Domain.Entities;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.FixedAssets.Infrastructure.Oracle;

public sealed class OracleFixedAssetJournalGateway : IFixedAssetJournalGateway
{
    private readonly string _connectionString;

    public OracleFixedAssetJournalGateway(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<DepreciationJournalPostResponse> PostDepreciationAsync(
        string idData,
        AccountingPeriod period,
        long runId,
        string userId,
        IReadOnlyList<DepreciationPreviewLine> lines,
        CancellationToken cancellationToken)
    {
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Baris depresiasi kosong.");
        }

        string noJurnal = GenerateNoJurnal(period, runId);
        DateTime journalDate = period.EndDate;

        await using var conn = new OracleConnection(_connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using OracleTransaction trx = conn.BeginTransaction();
        try
        {
            double? existingJurnalId = await GetExistingJurnalIdAsync(conn, trx, idData, period.PeriodeString, noJurnal, cancellationToken).ConfigureAwait(false);
            if (existingJurnalId.HasValue)
            {
                await trx.CommitAsync(cancellationToken).ConfigureAwait(false);
                return new DepreciationJournalPostResponse
                {
                    NoJurnal = noJurnal,
                    JurnalId = existingJurnalId,
                    PostedLineCount = lines.Count * 2,
                    IsAlreadyPosted = true
                };
            }

            await ClearTmpByScopeAsync(conn, trx, idData, period.PeriodeString, userId, cancellationToken).ConfigureAwait(false);

            bool hasSumberColumn = await HasSumberColumnAsync(conn, trx, cancellationToken).ConfigureAwait(false);
            string insertTmpSql = hasSumberColumn
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

            List<object> rows = new(lines.Count * 2);
            int baris = 1;
            foreach (DepreciationPreviewLine line in lines)
            {
                string desc = string.IsNullOrWhiteSpace(line.Description)
                    ? $"Depresiasi asset {line.AssetCode} periode {period.PeriodeString}"
                    : line.Description;

                rows.Add(new
                {
                    NOJURNAL = noJurnal,
                    TANGGAL = journalDate,
                    BARIS = baris++,
                    KODE = line.DepreciationExpenseAccount,
                    REKENING = "Depreciation Expense",
                    DEBET = line.DepreciationAmount,
                    KREDIT = 0m,
                    KETERANGAN = desc,
                    POSTED = "TRUE",
                    PERIODE = period.PeriodeString,
                    IDDATA = idData,
                    USERID = userId,
                    GLYEAR = period.Year,
                    GLMONTH = period.Month,
                    SUMBER = "FA"
                });

                rows.Add(new
                {
                    NOJURNAL = noJurnal,
                    TANGGAL = journalDate,
                    BARIS = baris++,
                    KODE = line.AccumulatedDepreciationAccount,
                    REKENING = "Accumulated Depreciation",
                    DEBET = 0m,
                    KREDIT = line.DepreciationAmount,
                    KETERANGAN = desc,
                    POSTED = "TRUE",
                    PERIODE = period.PeriodeString,
                    IDDATA = idData,
                    USERID = userId,
                    GLYEAR = period.Year,
                    GLMONTH = period.Month,
                    SUMBER = "FA"
                });
            }

            CommandDefinition insertCmd = new(insertTmpSql, rows, trx, commandType: CommandType.Text, cancellationToken: cancellationToken);
            await conn.ExecuteAsync(insertCmd).ConfigureAwait(false);

            int importStatus = await ImportTmpToJurnalAsync(conn, trx, idData, period, userId, cancellationToken).ConfigureAwait(false);
            if (importStatus is not 1 and not 99)
            {
                double? fallbackJurnalId = await GetExistingJurnalIdAsync(conn, trx, idData, period.PeriodeString, noJurnal, cancellationToken).ConfigureAwait(false);
                if (fallbackJurnalId.HasValue)
                {
                    await trx.CommitAsync(cancellationToken).ConfigureAwait(false);
                    return new DepreciationJournalPostResponse
                    {
                        NoJurnal = noJurnal,
                        JurnalId = fallbackJurnalId,
                        PostedLineCount = rows.Count,
                        IsAlreadyPosted = true
                    };
                }

                throw new InvalidOperationException($"Posting jurnal depresiasi gagal. Status import: {importStatus}");
            }

            double? jurnalId = await GetExistingJurnalIdAsync(conn, trx, idData, period.PeriodeString, noJurnal, cancellationToken).ConfigureAwait(false);

            await trx.CommitAsync(cancellationToken).ConfigureAwait(false);

            return new DepreciationJournalPostResponse
            {
                NoJurnal = noJurnal,
                JurnalId = jurnalId,
                PostedLineCount = rows.Count,
                IsAlreadyPosted = false
            };
        }
        catch
        {
            await trx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
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

    private static async Task<bool> HasSumberColumnAsync(
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

    private static async Task<double?> GetExistingJurnalIdAsync(
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
            transaction: trx,
            commandType: CommandType.Text,
            cancellationToken: cancellationToken);
        return await conn.ExecuteScalarAsync<double?>(cmd).ConfigureAwait(false);
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

    private static string GenerateNoJurnal(AccountingPeriod period, long runId)
    {
        return $"FA/{period.Year:0000}{period.Month:00}/{runId:000000}";
    }
}
