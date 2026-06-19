using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Accounting.BusinessLayer;
using Accounting.JurnalImport.Application;
using Accounting.JurnalImport.Domain;
using Accounting.Utilities;
using Oracle.ManagedDataAccess.Client;
using Serilog;

namespace Accounting.JurnalImport.Infrastructure.Oracle;

public sealed class OracleJurnalImportDataStore : IJurnalImportDataStore
{
    /// <summary>Maximum number of retry attempts when a deadlock (ORA-00060) is encountered.</summary>
    private const int MaxDeadlockRetries = 3;

    /// <summary>Base delay in milliseconds before retrying after a deadlock.</summary>
    private const int DeadlockRetryBaseDelayMs = 500;

    public string GetLockStatus(string idData, string period)
    {
        return JurnalServices.GetLockStatus(idData, period);
    }

    public int CountPeriod(string idData, string period)
    {
        return JurnalServices.CekPeriodeExist(idData, period);
    }

    public void CreateNextPeriod(string idData, int previousMonth, int year)
    {
        AccountServices.CreateNextPeriode(idData, previousMonth, year);
    }

    public void ClearStage(JurnalImportScope scope)
    {
        JurnalServices.DeleteJurnalTmpByScope(scope.IdData, scope.Period, scope.UserId);
    }

    public void StageRows(JurnalImportScope scope, IReadOnlyList<JurnalImportRow> rows)
    {
        JurnalServices.SaveUsingOracleBulkCopy("ACCT_JURNAL_TMP", JurnalImportRowMapper.ToStageDataTable(scope, rows));
    }

    public IReadOnlyList<JurnalImportValidationIssue> FindRowsWithNullKode(JurnalImportScope scope)
    {
        DataTable rows = JurnalServices.CekJurnal_KODENULL_Scoped(scope.IdData, scope.Period, scope.UserId);
        return rows.AsEnumerable()
            .Select(row => new JurnalImportValidationIssue(
                "KODE_NULL",
                "Kode Jurnal belum diisi sebanyak " + rows.Rows.Count.ToString("##,###") + " Nomor.",
                "NOJURNAL",
                row.Field<string>("NOJURNAL")))
            .ToList();
    }

    public IReadOnlyList<JurnalImportValidationIssue> FindExistingJournalNumbers(JurnalImportScope scope)
    {
        DataTable rows = JurnalServices.CekNoJurnalExistScoped(scope.IdData, scope.Period, scope.UserId);
        return rows.AsEnumerable()
            .Select(row => new JurnalImportValidationIssue(
                "NOJURNAL_EXISTS",
                "Duplikasi NoJurnal sebanyak " + rows.Rows.Count.ToString("##,###") + " Nomor.",
                "NOJURNAL",
                row.Field<string>("NOJURNAL")))
            .ToList();
    }

    public IReadOnlyList<JurnalImportValidationIssue> FindMissingAccounts(JurnalImportScope scope)
    {
        DataTable rows = JurnalServices.CekAkunMasterScoped(scope.CoaYear, scope.IdData, scope.Period, scope.UserId);
        return rows.AsEnumerable()
            .Select(row => new JurnalImportValidationIssue(
                "ACCOUNT_NOT_FOUND",
                "Jumlah Kode tidak terdaftar sebanyak " + rows.Rows.Count.ToString("##,###") + " Akun.",
                "ASAL",
                row.Field<string>("ASAL")))
            .ToList();
    }

    public int ImportPartial(JurnalImportScope scope, IReadOnlyList<JurnalImportRow> rows, IProgress<JurnalImportProgress>? progress)
    {
        return JurnalServices.ImportJurnalClientSide(scope, rows, progress);
    }

    /// <summary>
    /// Recalculates saldo for the imported period by processing each JurnalID individually
    /// using the V2 atomic recalculation procedure. This avoids the ORA-00060 deadlock that
    /// occurs when the legacy bulk <c>RecalkulasiSaldoDetail</c> updates all ACCT_COA rows
    /// in one large session while other sessions (or triggers) also lock the same rows.
    /// </summary>
    public void RecalculateSaldo(JurnalImportScope scope)
    {
        List<double> jurnalIds = GetJurnalIdsForPeriod(scope.IdData, scope.Period);

        if (jurnalIds.Count == 0)
        {
            Log.Warning("RecalculateSaldo: no journal IDs found for period {Period}, skipping recalc", scope.Period);
            return;
        }

        Log.Information(
            "RecalculateSaldo: processing {Count} journal(s) for period {Period} using per-JurnalID V2 recalc",
            jurnalIds.Count,
            scope.Period);

        foreach (double jurnalId in jurnalIds)
        {
            RecalculateWithDeadlockRetry(scope, jurnalId);
        }
    }

    /// <summary>
    /// Queries all JurnalIDs from ACCT_JURNAL_HDR for the given IDDATA and PERIODE.
    /// </summary>
    private static List<double> GetJurnalIdsForPeriod(string idData, string period)
    {
        const string sql = "SELECT JURNALID FROM ACCT_JURNAL_HDR WHERE IDDATA=:p_iddata AND PERIODE=:p_periode ORDER BY JURNALID";

        using OracleConnection connection = new(ConnectionManager.GetOracleConnection());
        connection.Open();
        using OracleCommand cmd = new(sql, connection) { CommandType = CommandType.Text };
        cmd.BindByName = true;
        cmd.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = idData;
        cmd.Parameters.Add(":p_periode", OracleDbType.Varchar2, 7).Value = period;

        List<double> ids = [];
        using OracleDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            ids.Add(reader.GetDouble(0));
        }

        return ids;
    }

    /// <summary>
    /// Calls the V2 per-JurnalID recalculation with retry on ORA-00060 deadlock.
    /// </summary>
    private static void RecalculateWithDeadlockRetry(JurnalImportScope scope, double jurnalId)
    {
        for (int attempt = 0; attempt <= MaxDeadlockRetries; attempt++)
        {
            try
            {
                AccountServices.RekalkulasiByJurnalID(
                    scope.IdData,
                    scope.Month,
                    scope.Year,
                    jurnalId,
                    scope.Period,
                    scope.UserId);
                return;
            }
            catch (OracleException ex) when (IsDeadlock(ex) && attempt < MaxDeadlockRetries)
            {
                int delayMs = DeadlockRetryBaseDelayMs * (1 << attempt);
                Log.Warning(
                    "RecalculateSaldo: deadlock (ORA-00060) on JurnalID={JurnalId} attempt {Attempt}/{MaxRetries}, retrying in {DelayMs}ms",
                    jurnalId,
                    attempt + 1,
                    MaxDeadlockRetries,
                    delayMs);
                Thread.Sleep(delayMs);
            }
        }
    }

    /// <summary>
    /// Checks whether an OracleException represents a deadlock (ORA-00060).
    /// </summary>
    private static bool IsDeadlock(OracleException ex)
    {
        return ex.Number == 60
            || (ex.Message?.Contains("ORA-00060", StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
