using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Accounting.BusinessLayer;
using Accounting.JurnalImport.Application;
using Accounting.JurnalImport.Domain;
using Accounting.Model;
using Accounting.Utilities;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Serilog;

namespace Accounting.JurnalImport.Infrastructure.Oracle;

public sealed class OracleJurnalImportDataStore : IJurnalImportDataStore
{
    private readonly object _replacePeriodSnapshotGate = new();
    private HashSet<string> _lastReplacePeriodAccountCodes = new(StringComparer.OrdinalIgnoreCase);
    private int _lastReplacePeriodImpactedRows;

    private sealed class ImportRecalcPosting
    {
        public double JurnalId { get; init; }
        public string Kode { get; init; } = string.Empty;
        public int RowCount { get; init; }
    }

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
        CaptureReplacePeriodSnapshot(scope);
        return JurnalServices.ImportJurnalClientSide(scope, rows, progress);
    }

    public JurnalImportRecalcQueueResult QueueRecalculation(JurnalImportScope scope)
    {
        ReplacePeriodSnapshot replaceSnapshot = ConsumeReplacePeriodSnapshot();
        List<ImportRecalcPosting> currentPostings = GetCurrentImpactedPostingsForImportedRows(scope);
        if (currentPostings.Count == 0 && replaceSnapshot.AccountCodes.Count == 0)
        {
            Log.Warning("Jurnal import recalc queue skipped: no impacted accounts found for period {Period}", scope.Period);
            return JurnalImportRecalcQueueResult.Empty();
        }

        List<long> jobIds = [];
        HashSet<string> allImpactedAccounts = new(StringComparer.OrdinalIgnoreCase);
        int impactedRows = replaceSnapshot.ImpactedRows + currentPostings.Sum(row => row.RowCount);
        string recalcScope = scope.Mode == JurnalImportMode.ReplacePeriod
            ? JurnalRecalcScopes.UpdateDelta
            : JurnalRecalcScopes.InsertDelta;

        bool replaceSnapshotQueued = false;
        foreach (IGrouping<double, ImportRecalcPosting> journalGroup in currentPostings.GroupBy(row => row.JurnalId).OrderBy(group => group.Key))
        {
            HashSet<string> journalAccounts = new(journalGroup.Select(row => row.Kode), StringComparer.OrdinalIgnoreCase);
            int journalRows = journalGroup.Sum(row => row.RowCount);

            if (!replaceSnapshotQueued)
            {
                journalAccounts.UnionWith(replaceSnapshot.AccountCodes);
                journalRows += replaceSnapshot.ImpactedRows;
                replaceSnapshotQueued = true;
            }

            foreach (string accountCode in journalAccounts)
            {
                allImpactedAccounts.Add(accountCode);
            }

            JurnalRecalcHint hint = JurnalRecalcHint.Create(
                recalcScope,
                journalRows,
                journalAccounts.Count,
                journalAccounts);
            long? jobId = JurnalInputOperationService.QueueRekalkulasiSaldo(
                scope.IdData,
                scope.Period,
                scope.UserId,
                journalGroup.Key,
                hint);
            if (jobId.HasValue)
            {
                jobIds.Add(jobId.Value);
            }
        }

        if (!replaceSnapshotQueued && replaceSnapshot.AccountCodes.Count > 0)
        {
            foreach (string accountCode in replaceSnapshot.AccountCodes)
            {
                allImpactedAccounts.Add(accountCode);
            }

            JurnalRecalcHint hint = JurnalRecalcHint.Create(
                recalcScope,
                replaceSnapshot.ImpactedRows,
                replaceSnapshot.AccountCodes.Count,
                replaceSnapshot.AccountCodes);
            long? jobId = JurnalInputOperationService.QueueRekalkulasiSaldo(
                scope.IdData,
                scope.Period,
                scope.UserId,
                0d,
                hint);
            if (jobId.HasValue)
            {
                jobIds.Add(jobId.Value);
            }
        }

        Log.Information(
            "Jurnal import recalc queued period={Period} mode={Mode} jobs={JobCount} impacted_accounts={ImpactedAccounts} impacted_rows={ImpactedRows}",
            scope.Period,
            scope.Mode,
            jobIds.Count,
            allImpactedAccounts.Count,
            impactedRows);

        return JurnalImportRecalcQueueResult.Create(jobIds, allImpactedAccounts, impactedRows);
    }

    private void CaptureReplacePeriodSnapshot(JurnalImportScope scope)
    {
        if (scope.Mode != JurnalImportMode.ReplacePeriod)
        {
            lock (_replacePeriodSnapshotGate)
            {
                _lastReplacePeriodAccountCodes.Clear();
                _lastReplacePeriodImpactedRows = 0;
            }

            return;
        }

        List<ImportRecalcPosting> existingPostings = GetCurrentImpactedPostingsForPeriod(scope);
        lock (_replacePeriodSnapshotGate)
        {
            _lastReplacePeriodAccountCodes = new HashSet<string>(
                existingPostings.Select(row => row.Kode),
                StringComparer.OrdinalIgnoreCase);
            _lastReplacePeriodImpactedRows = existingPostings.Sum(row => row.RowCount);
        }
    }

    private ReplacePeriodSnapshot ConsumeReplacePeriodSnapshot()
    {
        lock (_replacePeriodSnapshotGate)
        {
            ReplacePeriodSnapshot snapshot = new(
                _lastReplacePeriodAccountCodes.ToArray(),
                _lastReplacePeriodImpactedRows);
            _lastReplacePeriodAccountCodes.Clear();
            _lastReplacePeriodImpactedRows = 0;
            return snapshot;
        }
    }


    private static List<ImportRecalcPosting> GetCurrentImpactedPostingsForImportedRows(JurnalImportScope scope)
    {
        const string sql = @"
            WITH IMPORTED_JOURNALS AS (
                SELECT DISTINCT h.JURNALID
                FROM ACCT_JURNAL_HDR h
                JOIN ACCT_JURNAL_TMP t
                  ON t.IDDATA = h.IDDATA
                 AND t.PERIODE = h.PERIODE
                 AND t.NOJURNAL = h.NOJURNAL
                 AND t.TANGGAL = h.TANGGAL
                WHERE t.IDDATA = :idData
                  AND t.PERIODE = :period
                  AND t.USERID = :userId
            )
            SELECT h.JURNALID AS JurnalId,
                   d.KODE AS Kode,
                   COUNT(1) AS RowCount
            FROM IMPORTED_JOURNALS h
            JOIN ACCT_JURNAL_DTL d
              ON d.REFFID = h.JURNALID
            WHERE d.KODE IS NOT NULL
              AND (NVL(d.DEBET, 0) <> 0 OR NVL(d.KREDIT, 0) <> 0)
            GROUP BY h.JURNALID, d.KODE
            ORDER BY h.JURNALID, d.KODE";

        using OracleConnection connection = new(ConnectionManager.GetOracleConnection());
        connection.Open();
        return connection.Query<ImportRecalcPosting>(
            sql,
            new { idData = scope.IdData, period = scope.Period, userId = scope.UserId }).ToList();
    }
    private static List<ImportRecalcPosting> GetCurrentImpactedPostingsForPeriod(JurnalImportScope scope)
    {
        const string sql = @"
            SELECT h.JURNALID AS JurnalId,
                   d.KODE AS Kode,
                   COUNT(1) AS RowCount
            FROM ACCT_JURNAL_HDR h
            JOIN ACCT_JURNAL_DTL d
              ON d.REFFID = h.JURNALID
            WHERE h.IDDATA = :idData
              AND h.PERIODE = :period
              AND d.KODE IS NOT NULL
              AND (NVL(d.DEBET, 0) <> 0 OR NVL(d.KREDIT, 0) <> 0)
            GROUP BY h.JURNALID, d.KODE
            ORDER BY h.JURNALID, d.KODE";

        using OracleConnection connection = new(ConnectionManager.GetOracleConnection());
        connection.Open();
        return connection.Query<ImportRecalcPosting>(sql, new { idData = scope.IdData, period = scope.Period }).ToList();
    }

    private sealed record ReplacePeriodSnapshot(IReadOnlyList<string> AccountCodes, int ImpactedRows);
}