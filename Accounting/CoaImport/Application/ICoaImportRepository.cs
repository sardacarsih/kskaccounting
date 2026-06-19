using System.Collections.Generic;
using System;
using Accounting.CoaImport.Domain;

namespace Accounting.CoaImport.Application;

public interface ICoaImportRepository
{
    IReadOnlyList<CoaImportValidationIssue> ValidateRows(
        CoaImportScope scope,
        IReadOnlyList<CoaImportRow> rows,
        IProgress<CoaImportProgress>? progress = null);

    int ImportRows(
        CoaImportScope scope,
        CoaImportMode mode,
        IReadOnlyList<CoaImportRow> rows,
        IProgress<CoaImportProgress>? progress = null);

    void EnsurePeriodExists(string idData, int year);
    void RecalculateSaldo(string idData, int year, string userId);
}
