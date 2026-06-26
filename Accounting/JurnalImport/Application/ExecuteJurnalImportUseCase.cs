using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Accounting.JurnalImport.Domain;

namespace Accounting.JurnalImport.Application;

public sealed class ExecuteJurnalImportUseCase
{
    private readonly IJurnalImportDataStore _dataStore;

    public ExecuteJurnalImportUseCase(IJurnalImportDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public JurnalImportResult Execute(JurnalImportScope scope, IReadOnlyList<JurnalImportRow> rows)
    {
        return Execute(scope, rows, null);
    }

    public JurnalImportResult Execute(JurnalImportScope scope, IReadOnlyList<JurnalImportRow> rows, IProgress<JurnalImportProgress>? progress)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool stagedRowsLoaded = false;
        int totalRows = rows.Count;
        progress?.Report(new JurnalImportProgress(0, "Menyiapkan import jurnal...", 0, totalRows));

        try
        {
            progress?.Report(new JurnalImportProgress(5, "Memvalidasi data jurnal...", 0, totalRows));
            IReadOnlyList<JurnalImportValidationIssue> preStageIssues = ValidateBeforeStage(scope, rows);
            if (preStageIssues.Count > 0)
            {
                return JurnalImportResult.ValidationFailed(preStageIssues, stopwatch);
            }

            IReadOnlyList<JurnalImportBalanceIssue> balanceIssues = GetBalanceIssues(rows);
            if (balanceIssues.Count > 0)
            {
                return JurnalImportResult.NotBalanced(balanceIssues, stopwatch);
            }

            if (_dataStore.GetLockStatus(scope.IdData, scope.Period) == "Y")
            {
                return JurnalImportResult.ValidationFailed(
                    [new JurnalImportValidationIssue("PERIOD_LOCKED", $"Periode Akuntansi : {scope.Period} Telah Dikunci.", "Periode", scope.Period)],
                    stopwatch);
            }

            if (_dataStore.CountPeriod(scope.IdData, scope.Period) == 0)
            {
                _dataStore.CreateNextPeriod(scope.IdData, scope.Month - 1, scope.Year);
            }

            progress?.Report(new JurnalImportProgress(15, "Menyiapkan staging import jurnal...", 0, totalRows));
            _dataStore.ClearStage(scope);
            _dataStore.StageRows(scope, rows);
            stagedRowsLoaded = true;

            progress?.Report(new JurnalImportProgress(30, "Memvalidasi staging jurnal...", 0, totalRows));
            IReadOnlyList<JurnalImportValidationIssue> stageIssues = ValidateStage(scope);
            if (stageIssues.Count > 0)
            {
                return JurnalImportResult.ValidationFailed(stageIssues, stopwatch);
            }

            progress?.Report(new JurnalImportProgress(40, "Menyimpan jurnal ke database...", 0, totalRows));
            int statusCode = _dataStore.ImportPartial(scope, rows, progress);
            if (statusCode == 0)
            {
                return JurnalImportResult.Failed(statusCode, "Import Jurnal di Batalkan \nCek Periode Pada Lembar Excel Double ", stopwatch);
            }

            if (statusCode == 1)
            {
                return JurnalImportResult.Failed(statusCode, "Import Jurnal di Batalkan \nJurnal Tidak Seimbang ", stopwatch);
            }

            progress?.Report(new JurnalImportProgress(95, "Menjadwalkan hitung ulang saldo jurnal...", totalRows, totalRows));
            try
            {
                JurnalImportRecalcQueueResult recalcQueueResult = _dataStore.QueueRecalculation(scope);
                progress?.Report(new JurnalImportProgress(100, "Import jurnal selesai.", totalRows, totalRows));
                return JurnalImportResult.Success(statusCode, stopwatch, recalcQueueResult);
            }
            catch (Exception ex)
            {
                return JurnalImportResult.SuccessWithRecalculationWarning(
                    statusCode,
                    BuildRecalculationWarning(scope, ex),
                    stopwatch);
            }
        }
        finally
        {
            if (stagedRowsLoaded)
            {
                try
                {
                    _dataStore.ClearStage(scope);
                }
                catch
                {
                    // Best-effort cleanup only.
                }
            }
        }
    }

    private static string BuildRecalculationWarning(JurnalImportScope scope, Exception ex)
    {
        string technicalMessage = GetFullExceptionMessage(ex);
        return "Jurnal sudah berhasil diimport ke database, tetapi rekalkulasi saldo gagal." +
            Environment.NewLine + Environment.NewLine +
            "Silakan jalankan rekalkulasi saldo ulang untuk periode " + scope.Period + "." +
            Environment.NewLine + "Jika saldo laporan belum berubah, jangan import ulang file yang sama." +
            Environment.NewLine + Environment.NewLine +
            "Informasi teknis:" +
            Environment.NewLine + technicalMessage;
    }

    private static string GetFullExceptionMessage(Exception ex)
    {
        return ex.InnerException == null
            ? ex.Message
            : ex.Message + Environment.NewLine + ex.InnerException.Message;
    }

    private static IReadOnlyList<JurnalImportValidationIssue> ValidateBeforeStage(JurnalImportScope scope, IReadOnlyList<JurnalImportRow> rows)
    {
        List<JurnalImportValidationIssue> issues = [];
        if (rows.Count == 0)
        {
            issues.Add(new JurnalImportValidationIssue("NO_ROWS", "Data excel belum dipilih atau kosong."));
            return issues;
        }

        issues.AddRange(JurnalImportTemplateValidator.ValidateRows(rows));

        if (!JurnalImportTemplateValidator.TryGetSinglePeriod(rows, out string sourcePeriod, out JurnalImportValidationIssue? periodIssue))
        {
            issues.Add(periodIssue!);
        }
        else if (scope.Period != sourcePeriod)
        {
            issues.Add(new JurnalImportValidationIssue(
                "PERIOD_MISMATCH",
                "Import Jurnal di Batalkan \nPilihan Periode tidak sama dengan sumber data ",
                "Periode",
                sourcePeriod));
        }

        issues.AddRange(rows
            .GroupBy(row => new { row.NoJurnal, row.Tanggal.Date })
            .GroupBy(group => group.Key.NoJurnal)
            .Where(group => group.Select(item => item.Key.Date).Distinct().Count() > 1)
            .Select(group => new JurnalImportValidationIssue(
                "DUPLICATE_JOURNAL_DIFFERENT_DATE",
                "Duplikasi NoJurnal pada nomor jurnal yang sama tetapi beda tanggal.",
                "NoJurnal",
                group.Key)));

        return issues;
    }

    private static IReadOnlyList<JurnalImportBalanceIssue> GetBalanceIssues(IReadOnlyList<JurnalImportRow> rows)
    {
        return rows
            .GroupBy(row => new { row.NoJurnal, row.Tanggal.Date })
            .Select(group => new JurnalImportBalanceIssue(
                group.Key.NoJurnal,
                group.Key.Date,
                group.Sum(row => row.Debet),
                group.Sum(row => row.Kredit)))
            .Where(issue => issue.Selisih != 0m)
            .OrderBy(issue => issue.NoJurnal)
            .ThenBy(issue => issue.Tanggal)
            .ToList();
    }

    private IReadOnlyList<JurnalImportValidationIssue> ValidateStage(JurnalImportScope scope)
    {
        List<JurnalImportValidationIssue> issues = [];
        issues.AddRange(_dataStore.FindRowsWithNullKode(scope));
        if (scope.Mode == JurnalImportMode.AddOnly)
        {
            issues.AddRange(_dataStore.FindExistingJournalNumbers(scope));
        }

        issues.AddRange(_dataStore.FindMissingAccounts(scope));
        return issues;
    }
}
