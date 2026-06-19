using System.Collections.Generic;
using System.Data;
using System;
using Accounting.JurnalImport.Application;
using Accounting.JurnalImport.Domain;

namespace Accounting.JurnalImport.Presentation;

public sealed class FrmJurnalModuleImportViewModel
{
    private readonly ExecuteJurnalImportUseCase _executeUseCase;
    private readonly string _idData;
    private readonly string _userId;

    public FrmJurnalModuleImportViewModel(ExecuteJurnalImportUseCase executeUseCase, string idData, string userId)
    {
        _executeUseCase = executeUseCase;
        _idData = idData;
        _userId = userId;
    }

    public JurnalImportResult Import(DataTable sourceData, int month, int year, int coaYear, string period, JurnalImportSource source)
    {
        return Import(sourceData, month, year, coaYear, period, source, null);
    }

    public JurnalImportResult Import(
        DataTable sourceData,
        int month,
        int year,
        int coaYear,
        string period,
        JurnalImportSource source,
        IProgress<JurnalImportProgress>? progress)
    {
        IReadOnlyList<JurnalImportRow> rows = JurnalImportRowMapper.FromDataTable(sourceData);
        JurnalImportScope scope = new(_idData, _userId, month, year, coaYear, period, source, JurnalImportMode.AddOnly);
        return _executeUseCase.Execute(scope, rows, progress);
    }
}
