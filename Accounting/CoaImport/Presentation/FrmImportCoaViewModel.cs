using System;
using System.Collections.Generic;
using System.ComponentModel;
using Accounting.CoaImport.Application;
using Accounting.CoaImport.Domain;

namespace Accounting.CoaImport.Presentation;

public sealed class FrmImportCoaViewModel
{
    private readonly PreviewCoaImportUseCase _previewUseCase;
    private readonly ExecuteCoaImportUseCase _executeUseCase;
    private readonly string _idData;
    private readonly string _userId;

    public FrmImportCoaViewModel(
        PreviewCoaImportUseCase previewUseCase,
        ExecuteCoaImportUseCase executeUseCase,
        string idData,
        string userId,
        CoaImportKind accountingKind)
    {
        _previewUseCase = previewUseCase;
        _executeUseCase = executeUseCase;
        _idData = idData;
        _userId = userId;
        AccountingKind = accountingKind;
    }

    public string FilePath { get; private set; } = string.Empty;
    public IReadOnlyList<string> Sheets { get; private set; } = [];
    public string SelectedSheet { get; private set; } = string.Empty;
    public int Year { get; set; }
    public CoaImportMode Mode { get; set; } = CoaImportMode.Full;
    public CoaImportKind AccountingKind { get; }
    public BindingList<CoaImportRow> Rows { get; private set; } = [];
    public bool CanImport { get; private set; }
    public string StatusText { get; private set; } = "Pilih file Excel daftar perkiraan.";
    public IReadOnlyList<CoaImportValidationIssue> LastIssues { get; private set; } = [];

    public void LoadWorkbook(string path)
    {
        FilePath = path;
        Sheets = _previewUseCase.GetSheets(path);
        SelectedSheet = Sheets.Count > 0 ? Sheets[0] : string.Empty;
        Rows = [];
        LastIssues = [];
        CanImport = false;
        StatusText = Sheets.Count > 0 ? "Pilih sheet untuk preview import." : "Workbook tidak memiliki sheet.";
    }

    public void PreviewSheet(string sheetName)
    {
        SelectedSheet = sheetName;
        Rows = [];
        LastIssues = [];
        CanImport = false;

        try
        {
            IReadOnlyList<CoaImportRow> rows = _previewUseCase.Preview(FilePath, sheetName, AccountingKind);
            Rows = new BindingList<CoaImportRow>(new List<CoaImportRow>(rows));
            CanImport = Rows.Count > 0;
            StatusText = Rows.Count == 0 ? "Sheet tidak memiliki data COA." : $"{Rows.Count:##,###} Record";
        }
        catch (CoaImportValidationException ex)
        {
            LastIssues = ex.Issues;
            StatusText = ex.Message;
        }
    }

    public CoaImportResult Import()
    {
        return Import(null);
    }

    public CoaImportResult Import(IProgress<CoaImportProgress>? progress)
    {
        string batchId = Guid.NewGuid().ToString("N");
        CoaImportScope scope = new(_idData, Year, _userId, batchId, AccountingKind);
        CoaImportResult result = _executeUseCase.Execute(scope, Mode, new List<CoaImportRow>(Rows), progress);

        LastIssues = result.Issues;
        StatusText = result.Message;
        if (result.IsSuccess)
        {
            CanImport = false;
        }

        return result;
    }
}
