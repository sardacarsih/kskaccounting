using System;
using System.Collections.Generic;
using System.Diagnostics;
using Accounting.CoaImport.Domain;

namespace Accounting.CoaImport.Application;

public sealed class ImportCoaAppService
{
    private readonly ICoaImportRepository _repository;

    public ImportCoaAppService(ICoaImportRepository repository)
    {
        _repository = repository;
    }

    public CoaImportResult Execute(CoaImportScope scope, CoaImportMode mode, IReadOnlyList<CoaImportRow> rows)
    {
        return Execute(scope, mode, rows, null);
    }

    public CoaImportResult Execute(
        CoaImportScope scope,
        CoaImportMode mode,
        IReadOnlyList<CoaImportRow> rows,
        IProgress<CoaImportProgress>? progress)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        int totalRows = rows.Count;
        progress?.Report(new CoaImportProgress(0, "Menyiapkan import COA...", 0, totalRows));

        if (rows.Count == 0)
        {
            return CoaImportResult.ValidationFailed(
                [new CoaImportValidationIssue("NO_ROWS", "Tidak ada data COA yang siap diimport.")],
                scope.BatchId,
                stopwatch.Elapsed);
        }

        progress?.Report(new CoaImportProgress(5, "Memvalidasi data COA...", 0, totalRows));
        IReadOnlyList<CoaImportValidationIssue> issues = _repository.ValidateRows(scope, rows, progress);
        if (issues.Count > 0)
        {
            return CoaImportResult.ValidationFailed(issues, scope.BatchId, stopwatch.Elapsed);
        }

        progress?.Report(new CoaImportProgress(10, "Validasi selesai. Mulai import COA...", 0, totalRows));
        int statusCode = _repository.ImportRows(scope, mode, rows, progress);
        if (statusCode != 1)
        {
            return CoaImportResult.Failed(
                statusCode,
                $"Import Chart Of Account gagal. Status import: {statusCode}.",
                scope.BatchId,
                stopwatch.Elapsed);
        }

        if (mode == CoaImportMode.Full)
        {
            progress?.Report(new CoaImportProgress(90, "Memastikan periode awal tahun tersedia...", totalRows, totalRows));
            _repository.EnsurePeriodExists(scope.IdData, scope.Year);
        }

        progress?.Report(new CoaImportProgress(95, "Menghitung ulang saldo detail...", totalRows, totalRows));
        _repository.RecalculateSaldo(scope.IdData, scope.Year, scope.UserId);
        progress?.Report(new CoaImportProgress(100, "Import COA selesai.", totalRows, totalRows));
        return CoaImportResult.Success(statusCode, scope.BatchId, stopwatch.Elapsed);
    }
}
