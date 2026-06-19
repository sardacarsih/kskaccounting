using System;
using System.Collections.Generic;
using Accounting.CoaImport.Domain;

namespace Accounting.CoaImport.Application;

public sealed class ExecuteCoaImportUseCase
{
    private readonly ImportCoaAppService _appService;

    public ExecuteCoaImportUseCase(ImportCoaAppService appService)
    {
        _appService = appService;
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
        return _appService.Execute(scope, mode, rows, progress);
    }
}
