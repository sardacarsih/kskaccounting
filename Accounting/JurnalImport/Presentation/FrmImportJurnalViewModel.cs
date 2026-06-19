using System;
using System.Collections.Generic;
using System.ComponentModel;
using Accounting.JurnalImport.Application;
using Accounting.JurnalImport.Domain;

namespace Accounting.JurnalImport.Presentation;

public sealed class FrmImportJurnalViewModel
{
    private readonly PreviewJurnalImportUseCase _previewUseCase;
    private readonly ExecuteJurnalImportUseCase _executeUseCase;
    private readonly string _idData;
    private readonly string _userId;

    public FrmImportJurnalViewModel(
        PreviewJurnalImportUseCase previewUseCase,
        ExecuteJurnalImportUseCase executeUseCase,
        string idData,
        string userId)
    {
        _previewUseCase = previewUseCase;
        _executeUseCase = executeUseCase;
        _idData = idData;
        _userId = userId;
    }

    public string FilePath { get; private set; } = string.Empty;
    public IReadOnlyList<string> Sheets { get; private set; } = [];
    public string SelectedSheet { get; private set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public int CoaYear { get; set; }
    public JurnalImportSource Source { get; set; } = JurnalImportSource.Excel;
    public JurnalImportMode Mode { get; set; } = JurnalImportMode.AddOnly;
    public BindingList<JurnalImportRow> Rows { get; private set; } = [];
    public bool CanImport { get; private set; }
    public string StatusText { get; private set; } = "Pilih file Excel jurnal.";
    public IReadOnlyList<JurnalImportValidationIssue> LastIssues { get; private set; } = [];

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
            IReadOnlyList<JurnalImportRow> rows = _previewUseCase.Preview(FilePath, sheetName);
            Rows = new BindingList<JurnalImportRow>(new List<JurnalImportRow>(rows));
            CanImport = Rows.Count > 0;
            StatusText = Rows.Count == 0 ? "Sheet tidak memiliki data jurnal." : $"{Rows.Count:##,###} Record";
        }
        catch (JurnalImportValidationException ex)
        {
            LastIssues = ex.Issues;
            StatusText = ex.Message;
        }
    }

    public JurnalImportResult Import()
    {
        return Import(null);
    }

    public JurnalImportResult Import(IProgress<JurnalImportProgress>? progress)
    {
        string period = $"{Month:00}/{Year}";
        JurnalImportScope scope = new(_idData, _userId, Month, Year, CoaYear <= 0 ? Year : CoaYear, period, Source, Mode);
        JurnalImportResult result = _executeUseCase.Execute(scope, new List<JurnalImportRow>(Rows), progress);

        LastIssues = result.Issues;
        StatusText = result.Message;
        if (result.IsSuccess)
        {
            CanImport = false;
        }

        return result;
    }
}
