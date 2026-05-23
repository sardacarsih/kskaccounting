using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Accounting.BusinessLayer;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using ExcelDataReader;
using OfficeOpenXml;

namespace Accounting.Form;

public sealed class FrmImportFixedAsset : XtraForm
{
    private const int TemplateValidationLastRow = 5000;

    private static readonly string[] TemplateColumns =
    [
        "AssetCode", "AssetName", "CategoryCode", "GroupCode",
        "AcquisitionDate", "InServiceDate", "DepreciationStartDate",
        "AcquisitionCost", "ResidualValue", "UsefulLifeMonths",
        "DepreciationMethod", "Status",
        "DepartmentId", "CostCenterId", "LocationId", "VendorId",
        "SerialNo", "CurrencyCode", "ExchangeRate", "Notes"
    ];

    private static readonly HashSet<string> ValidMethods = new(StringComparer.OrdinalIgnoreCase) { "SL", "DB", "NONE" };
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "DRAFT", "ACTIVE", "UNDER_CONSTRUCTION", "DISPOSED", "SOLD", "TRANSFERRED", "WRITTEN_OFF", "RETIRED"
    };

    private readonly TextEdit txtPath = new();
    private readonly ComboBoxEdit cboSheet = new();
    private readonly GridControl gridControl = new();
    private readonly GridView gridView = new();
    private readonly SimpleButton btnBrowse = new() { Text = "Pilih File Excel" };
    private readonly SimpleButton btnTemplate = new() { Text = "Unduh Template" };
    private readonly SimpleButton btnImport = new() { Text = "Import", Enabled = false };
    private readonly ProgressBarControl progressBar = new();
    private readonly LabelControl lblStatus = new() { Text = "Siap." };

    private DataTableCollection _tables;
    private List<ImportFixedAssetRow> _previewRows;

    private Dictionary<string, long> _categoryLookup;
    private Dictionary<string, (long GroupId, long CategoryId)> _groupLookup;

    public FrmImportFixedAsset()
    {
        Text = "Import Aset Tetap dari Excel";
        Width = 1100;
        Height = 700;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        BuildUi();
        LoadLookups();
    }

    private void BuildUi()
    {
        txtPath.SetBounds(16, 16, 600, 24);
        txtPath.Properties.ReadOnly = true;

        btnBrowse.SetBounds(624, 14, 130, 28);
        btnBrowse.Click += BtnBrowse_Click;

        LabelControl lblSheet = new() { Text = "Sheet:", Left = 16, Top = 52 };
        cboSheet.SetBounds(70, 48, 250, 24);
        cboSheet.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cboSheet.SelectedIndexChanged += CboSheet_SelectedIndexChanged;

        btnTemplate.SetBounds(770, 14, 130, 28);
        btnTemplate.Click += BtnTemplate_Click;

        btnImport.SetBounds(910, 14, 130, 28);
        btnImport.Click += BtnImport_Click;

        gridControl.SetBounds(16, 80, 1050, 500);
        gridControl.MainView = gridView;
        gridView.OptionsView.ShowGroupPanel = false;
        gridView.OptionsBehavior.Editable = false;

        gridView.RowStyle += (_, e) =>
        {
            if (e.RowHandle < 0) return;
            object val = gridView.GetRowCellValue(e.RowHandle, "ErrorMessage");
            if (val != null && val != DBNull.Value && !string.IsNullOrWhiteSpace(val.ToString()))
            {
                e.Appearance.BackColor = Color.MistyRose;
            }
        };

        progressBar.SetBounds(16, 592, 750, 22);
        progressBar.Properties.ShowTitle = true;
        progressBar.Properties.PercentView = true;
        progressBar.Properties.Maximum = 1;

        lblStatus.SetBounds(780, 596, 280, 18);

        Controls.AddRange([txtPath, btnBrowse, lblSheet, cboSheet, btnTemplate, btnImport, gridControl, progressBar, lblStatus]);
    }

    private void LoadLookups()
    {
        try
        {
            string idData = CompanyInfo.IDDATA?.Trim() ?? string.Empty;
            _categoryLookup = FixedAssetQueryServices.GetCategoryLookup(idData);
            _groupLookup = FixedAssetQueryServices.GetGroupLookup(idData);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show("Gagal memuat data referensi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _categoryLookup = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            _groupLookup = new Dictionary<string, (long, long)>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private void BtnBrowse_Click(object sender, EventArgs e)
    {
        try
        {
            ResetPreviewState();
            cboSheet.Properties.Items.Clear();
            cboSheet.Text = "";
            btnImport.Enabled = false;

            using OpenFileDialog ofd = new() { Filter = "Excel Workbook|*.xlsx|Excel 97-2003 Workbook|*.xls" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            txtPath.Text = ofd.FileName;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);

            DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration() { UseHeaderRow = true }
            });

            _tables = result.Tables;
            foreach (DataTable table in _tables)
            {
                cboSheet.Properties.Items.Add(table.TableName);
            }

            if (cboSheet.Properties.Items.Count > 0)
            {
                cboSheet.SelectedIndex = 0;
            }
            else
            {
                lblStatus.Text = "Workbook tidak memiliki sheet yang dapat dibaca.";
            }
        }
        catch (Exception ex)
        {
            btnImport.Enabled = false;
            ResetPreviewState("Gagal membaca file Excel.");
            XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CboSheet_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (_tables == null || cboSheet.SelectedIndex < 0) return;

            DataTable dt = _tables[cboSheet.SelectedItem.ToString()];
            if (dt == null || dt.Rows.Count == 0)
            {
                ResetPreviewState("Sheet kosong.");
                btnImport.Enabled = false;
                return;
            }

            string colError = ValidateColumnStructure(dt, TemplateColumns);
            if (colError != null)
            {
                ResetPreviewState("Struktur kolom sheet tidak valid.");
                btnImport.Enabled = false;
                XtraMessageBox.Show(colError, "Struktur Kolom Salah", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string idData = CompanyInfo.IDDATA?.Trim() ?? string.Empty;
            _previewRows = ParseAndValidate(dt, idData);
            if (_previewRows.Count == 0)
            {
                ResetPreviewState("Tidak ada baris data untuk diimport.");
                XtraMessageBox.Show("Sheet tidak memiliki baris data yang dapat diimport.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            gridControl.DataSource = _previewRows;
            gridView.BestFitColumns();

            bool hasError = _previewRows.Any(r => !string.IsNullOrWhiteSpace(r.ErrorMessage));
            btnImport.Enabled = !hasError;
            int errorCount = _previewRows.Count(r => !string.IsNullOrWhiteSpace(r.ErrorMessage));
            lblStatus.Text = hasError
                ? $"{_previewRows.Count} baris, {errorCount} baris error."
                : $"{_previewRows.Count} baris, siap import.";
        }
        catch (Exception ex)
        {
            btnImport.Enabled = false;
            ResetPreviewState("Gagal memproses sheet.");
            XtraMessageBox.Show("Error parsing: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnTemplate_Click(object sender, EventArgs e)
    {
        try
        {
            using SaveFileDialog sfd = new()
            {
                Filter = "Excel Workbook|*.xlsx",
                FileName = "Template_Import_FixedAsset.xlsx"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            string idData = CompanyInfo.IDDATA?.Trim() ?? string.Empty;
            DataTable categoryTable = FixedAssetQueryServices.GetAssetCategories(idData);
            DataTable groupTable = FixedAssetQueryServices.GetAssetGroups(idData, null);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            var ws = package.Workbook.Worksheets.Add("FixedAsset");

            for (int i = 0; i < TemplateColumns.Length; i++)
            {
                ws.Cells[1, i + 1].Value = TemplateColumns[i];
                ws.Cells[1, i + 1].Style.Font.Bold = true;
            }

            AddSampleRows(ws, categoryTable, groupTable);

            ws.View.FreezePanes(2, 1);
            ws.Cells[1, 1, 2, TemplateColumns.Length].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
            ws.Cells[1, 1, 2, TemplateColumns.Length].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            ws.Cells[1, 1, 1, TemplateColumns.Length].AutoFilter = true;
            ws.Cells.AutoFitColumns();

            AddInstructionWorksheet(package);
            AddCategoryLookupWorksheet(package, categoryTable);
            AddGroupLookupWorksheet(package, groupTable, categoryTable);
            string validationHelperSheetName = AddValidationHelperWorksheet(package, categoryTable, groupTable);
            AddTemplateValidations(ws, categoryTable, groupTable, validationHelperSheetName);

            package.SaveAs(new FileInfo(sfd.FileName));

            XtraMessageBox.Show("Template berhasil disimpan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void AddSampleRows(ExcelWorksheet worksheet, DataTable categoryTable, DataTable groupTable)
    {
        List<(string AssetName, decimal AcquisitionCost, decimal ResidualValue, int UsefulLifeMonths, string DepreciationMethod, string Status, string Notes)> sampleDefinitions =
        [
            ("Laptop Operasional", 15000000m, 1000000m, 48, "SL", "ACTIVE", "Contoh asset IT"),
            ("Kendaraan Operasional", 275000000m, 25000000m, 60, "SL", "ACTIVE", "Contoh asset kendaraan"),
            ("Aset Dalam Pembangunan", 50000000m, 0m, 24, "NONE", "UNDER_CONSTRUCTION", "Contoh asset CIP")
        ];

        Dictionary<long, string> categoryCodes = categoryTable.AsEnumerable()
            .Where(static row => row["CATEGORY_ID"] != DBNull.Value)
            .ToDictionary(
                static row => Convert.ToInt64(row["CATEGORY_ID"]),
                static row => row["CATEGORY_CODE"]?.ToString()?.Trim() ?? string.Empty);

        List<(string CategoryCode, string GroupCode)> categoryGroups = groupTable.AsEnumerable()
            .Where(static row => row["CATEGORY_ID"] != DBNull.Value)
            .Select(row =>
            {
                long categoryId = Convert.ToInt64(row["CATEGORY_ID"]);
                categoryCodes.TryGetValue(categoryId, out string categoryCode);
                return (
                    CategoryCode: categoryCode ?? string.Empty,
                    GroupCode: row["GROUP_CODE"]?.ToString()?.Trim() ?? string.Empty);
            })
            .Where(static item => !string.IsNullOrWhiteSpace(item.CategoryCode))
            .Distinct()
            .ToList();

        if (categoryGroups.Count == 0)
        {
            string fallbackCategoryCode = categoryTable.Rows.Count > 0
                ? categoryTable.Rows[0]["CATEGORY_CODE"]?.ToString()?.Trim() ?? string.Empty
                : "IT";

            categoryGroups.Add((fallbackCategoryCode, string.Empty));
        }

        int sampleCount = Math.Min(sampleDefinitions.Count, Math.Max(categoryGroups.Count, 1));
        for (int i = 0; i < sampleCount; i++)
        {
            (string CategoryCode, string GroupCode) categoryGroup = categoryGroups[Math.Min(i, categoryGroups.Count - 1)];
            (string AssetName, decimal AcquisitionCost, decimal ResidualValue, int UsefulLifeMonths, string DepreciationMethod, string Status, string Notes) sample = sampleDefinitions[i];
            int row = i + 2;

            worksheet.Cells[row, 1].Value = string.Empty;
            worksheet.Cells[row, 2].Value = sample.AssetName;
            worksheet.Cells[row, 3].Value = categoryGroup.CategoryCode;
            worksheet.Cells[row, 4].Value = categoryGroup.GroupCode;
            worksheet.Cells[row, 5].Value = DateTime.Today.AddDays(-(i * 7));
            worksheet.Cells[row, 6].Value = i == 2 ? null : DateTime.Today.AddDays(-(i * 7) + 1);
            worksheet.Cells[row, 7].Value = i == 2 ? null : DateTime.Today.AddDays(-(i * 7) + 1);
            worksheet.Cells[row, 8].Value = sample.AcquisitionCost;
            worksheet.Cells[row, 9].Value = sample.ResidualValue;
            worksheet.Cells[row, 10].Value = sample.UsefulLifeMonths;
            worksheet.Cells[row, 11].Value = sample.DepreciationMethod;
            worksheet.Cells[row, 12].Value = sample.Status;
            worksheet.Cells[row, 13].Value = string.Empty;
            worksheet.Cells[row, 14].Value = string.Empty;
            worksheet.Cells[row, 15].Value = string.Empty;
            worksheet.Cells[row, 16].Value = string.Empty;
            worksheet.Cells[row, 17].Value = string.Empty;
            worksheet.Cells[row, 18].Value = "IDR";
            worksheet.Cells[row, 19].Value = 1;
            worksheet.Cells[row, 20].Value = sample.Notes;
        }

        worksheet.Cells[2, 5, sampleCount + 1, 7].Style.Numberformat.Format = "dd-MMM-yyyy";
    }

    private static void AddTemplateValidations(ExcelWorksheet worksheet, DataTable categoryTable, DataTable groupTable, string validationHelperSheetName)
    {
        AddFixedListValidation(worksheet, $"K2:K{TemplateValidationLastRow}", ValidMethods, "Pilih metode penyusutan yang valid.");
        AddFixedListValidation(worksheet, $"L2:L{TemplateValidationLastRow}", ValidStatuses, "Pilih status asset yang valid.");
        AddDateValidation(worksheet, $"E2:E{TemplateValidationLastRow}", false, "Isi AcquisitionDate dengan tanggal yang valid.");
        AddDateValidation(worksheet, $"F2:F{TemplateValidationLastRow}", true, "Isi InServiceDate dengan tanggal yang valid atau kosongkan.");
        AddDateValidation(worksheet, $"G2:G{TemplateValidationLastRow}", true, "Isi DepreciationStartDate dengan tanggal yang valid atau kosongkan.");
        AddDecimalValidation(worksheet, $"H2:H{TemplateValidationLastRow}", false, 0d, null, "AcquisitionCost wajib angka dan tidak boleh negatif.");
        AddDecimalValidation(worksheet, $"I2:I{TemplateValidationLastRow}", true, 0d, null, "ResidualValue harus angka dan tidak boleh negatif.");
        AddDecimalValidation(worksheet, $"J2:J{TemplateValidationLastRow}", false, 1d, null, "UsefulLifeMonths wajib angka lebih besar dari 0 dan diisi bilangan bulat.");
        AddDecimalValidation(worksheet, $"S2:S{TemplateValidationLastRow}", true, 0.0000001d, null, "ExchangeRate harus angka lebih besar dari 0 atau kosongkan.");

        if (categoryTable.Rows.Count > 0)
        {
            AddSheetRangeValidation(
                worksheet,
                $"C2:C{TemplateValidationLastRow}",
                "ReferensiKategori",
                2,
                categoryTable.Rows.Count + 1,
                1,
                "Pilih CategoryCode dari sheet ReferensiKategori.");
        }

        if (groupTable.Rows.Count > 0)
        {
            AddDependentGroupValidation(
                worksheet,
                $"D2:D{TemplateValidationLastRow}",
                validationHelperSheetName,
                categoryTable.Rows.Count,
                "Pilih GroupCode dari sheet ReferensiGrup.");
        }
    }

    private static string AddValidationHelperWorksheet(ExcelPackage package, DataTable categoryTable, DataTable groupTable)
    {
        const string worksheetName = "ValidationLists";
        const string emptyGroupAlias = "FA_GRP_EMPTY";

        var worksheet = package.Workbook.Worksheets.Add(worksheetName);
        worksheet.Cells[1, 1].Value = "CategoryCode";
        worksheet.Cells[1, 2].Value = "GroupAlias";
        worksheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;

        var groupMap = groupTable.AsEnumerable()
            .Where(static row => row["CATEGORY_ID"] != DBNull.Value)
            .GroupBy(static row => Convert.ToInt64(row["CATEGORY_ID"]))
            .ToDictionary(
                static group => group.Key,
                static group => group
                    .Select(static row => row["GROUP_CODE"]?.ToString() ?? string.Empty)
                    .Where(static code => !string.IsNullOrWhiteSpace(code))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(static code => code, StringComparer.OrdinalIgnoreCase)
                    .ToList());

        int mappingRow = 2;
        int groupColumn = 4;

        foreach (DataRow categoryRow in categoryTable.Rows)
        {
            if (categoryRow["CATEGORY_ID"] == DBNull.Value)
            {
                continue;
            }

            long categoryId = Convert.ToInt64(categoryRow["CATEGORY_ID"]);
            string categoryCode = categoryRow["CATEGORY_CODE"]?.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(categoryCode))
            {
                continue;
            }

            string groupAlias = $"FA_GRP_{categoryId}";
            worksheet.Cells[mappingRow, 1].Value = categoryCode;
            worksheet.Cells[mappingRow, 2].Value = groupAlias;

            worksheet.Cells[1, groupColumn].Value = groupAlias;
            List<string> groups = groupMap.TryGetValue(categoryId, out List<string> categoryGroups)
                ? categoryGroups
                : [];

            if (groups.Count == 0)
            {
                worksheet.Cells[2, groupColumn].Value = string.Empty;
                package.Workbook.Names.Add(groupAlias, worksheet.Cells[2, groupColumn, 2, groupColumn]);
            }
            else
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    worksheet.Cells[i + 2, groupColumn].Value = groups[i];
                }

                package.Workbook.Names.Add(groupAlias, worksheet.Cells[2, groupColumn, groups.Count + 1, groupColumn]);
            }

            mappingRow++;
            groupColumn++;
        }

        worksheet.Cells[1, groupColumn].Value = emptyGroupAlias;
        worksheet.Cells[2, groupColumn].Value = string.Empty;
        package.Workbook.Names.Add(emptyGroupAlias, worksheet.Cells[2, groupColumn, 2, groupColumn]);

        worksheet.Hidden = OfficeOpenXml.eWorkSheetHidden.VeryHidden;
        return worksheetName;
    }

    private static void AddFixedListValidation(ExcelWorksheet worksheet, string address, IEnumerable<string> values, string prompt)
    {
        var validation = worksheet.DataValidations.AddListValidation(address);
        validation.AllowBlank = true;
        validation.ShowErrorMessage = true;
        validation.ErrorTitle = "Nilai Tidak Valid";
        validation.Error = prompt;
        validation.ShowInputMessage = true;
        validation.PromptTitle = "Pilihan Tersedia";
        validation.Prompt = prompt;

        foreach (string value in values)
        {
            validation.Formula.Values.Add(value);
        }
    }

    private static void AddSheetRangeValidation(
        ExcelWorksheet worksheet,
        string address,
        string sourceWorksheetName,
        int startRow,
        int endRow,
        int columnNumber,
        string prompt)
    {
        var validation = worksheet.DataValidations.AddListValidation(address);
        validation.AllowBlank = true;
        validation.ShowErrorMessage = true;
        validation.ErrorTitle = "Nilai Tidak Valid";
        validation.Error = prompt;
        validation.ShowInputMessage = true;
        validation.PromptTitle = "Referensi";
        validation.Prompt = prompt;
        validation.Formula.ExcelFormula = BuildSheetRangeFormula(sourceWorksheetName, startRow, endRow, columnNumber);
    }

    private static void AddDependentGroupValidation(ExcelWorksheet worksheet, string address, string helperWorksheetName, int categoryCount, string prompt)
    {
        var validation = worksheet.DataValidations.AddListValidation(address);
        validation.AllowBlank = true;
        validation.ShowErrorMessage = true;
        validation.ErrorTitle = "Nilai Tidak Valid";
        validation.Error = prompt;
        validation.ShowInputMessage = true;
        validation.PromptTitle = "Referensi";
        validation.Prompt = prompt;
        validation.Formula.ExcelFormula =
            $"INDIRECT(IFERROR(VLOOKUP($C2,'{helperWorksheetName}'!$A$2:$B${Math.Max(categoryCount + 1, 2)},2,FALSE),\"FA_GRP_EMPTY\"))";
    }

    private static string BuildSheetRangeFormula(string worksheetName, int startRow, int endRow, int columnNumber)
    {
        string columnLetter = GetExcelColumnLetter(columnNumber);
        return $"'{worksheetName}'!${columnLetter}${startRow}:${columnLetter}${endRow}";
    }

    private static string GetExcelColumnLetter(int columnNumber)
    {
        if (columnNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columnNumber));
        }

        string columnLetter = string.Empty;
        int current = columnNumber;
        while (current > 0)
        {
            int modulo = (current - 1) % 26;
            columnLetter = Convert.ToChar('A' + modulo) + columnLetter;
            current = (current - modulo) / 26;
        }

        return columnLetter;
    }

    private static void AddDateValidation(ExcelWorksheet worksheet, string address, bool allowBlank, string prompt)
    {
        var validation = worksheet.DataValidations.AddDateTimeValidation(address);
        validation.AllowBlank = allowBlank;
        validation.ShowErrorMessage = true;
        validation.ErrorTitle = "Tanggal Tidak Valid";
        validation.Error = prompt;
        validation.ShowInputMessage = true;
        validation.PromptTitle = "Format Tanggal";
        validation.Prompt = prompt;
        validation.Operator = OfficeOpenXml.DataValidation.ExcelDataValidationOperator.between;
        validation.Formula.Value = new DateTime(1900, 1, 1);
        validation.Formula2.Value = new DateTime(9999, 12, 31);
    }

    private static void AddDecimalValidation(ExcelWorksheet worksheet, string address, bool allowBlank, double? minValue, double? maxValue, string prompt)
    {
        var validation = worksheet.DataValidations.AddDecimalValidation(address);
        validation.AllowBlank = allowBlank;
        validation.ShowErrorMessage = true;
        validation.ErrorTitle = "Angka Tidak Valid";
        validation.Error = prompt;
        validation.ShowInputMessage = true;
        validation.PromptTitle = "Format Angka";
        validation.Prompt = prompt;

        if (minValue.HasValue && maxValue.HasValue)
        {
            validation.Operator = OfficeOpenXml.DataValidation.ExcelDataValidationOperator.between;
            validation.Formula.Value = minValue.Value;
            validation.Formula2.Value = maxValue.Value;
            return;
        }

        if (minValue.HasValue)
        {
            validation.Operator = OfficeOpenXml.DataValidation.ExcelDataValidationOperator.greaterThanOrEqual;
            validation.Formula.Value = minValue.Value;
            return;
        }

        if (maxValue.HasValue)
        {
            validation.Operator = OfficeOpenXml.DataValidation.ExcelDataValidationOperator.lessThanOrEqual;
            validation.Formula.Value = maxValue.Value;
        }
    }


    private static void AddInstructionWorksheet(ExcelPackage package)
    {
        var worksheet = package.Workbook.Worksheets.Add("Petunjuk");
        string[] instructions =
        [
            "Gunakan sheet 'FixedAsset' untuk import.",
            "Jangan ubah nama atau urutan kolom header.",
            "AssetCode boleh dikosongkan agar sistem generate otomatis.",
            "CategoryCode wajib diisi memakai kode dari sheet 'ReferensiKategori'.",
            "GroupCode opsional, tapi bila diisi harus cocok dengan kategori dan memakai kode dari sheet 'ReferensiGrup'.",
            "Baris kosong akan diabaikan saat preview import.",
            "Jika ada satu baris error, seluruh proses import akan diblok."
        ];

        worksheet.Cells[1, 1].Value = "Petunjuk Import Fixed Asset";
        worksheet.Cells[1, 1].Style.Font.Bold = true;
        worksheet.Cells[1, 1].Style.Font.Size = 14;

        for (int i = 0; i < instructions.Length; i++)
        {
            worksheet.Cells[i + 3, 1].Value = i + 1;
            worksheet.Cells[i + 3, 2].Value = instructions[i];
        }

        worksheet.Cells.AutoFitColumns();
    }

    private static void AddCategoryLookupWorksheet(ExcelPackage package, DataTable categoryTable)
    {
        var worksheet = package.Workbook.Worksheets.Add("ReferensiKategori");
        worksheet.Cells[1, 1].Value = "CategoryCode";
        worksheet.Cells[1, 2].Value = "CategoryName";
        worksheet.Cells[1, 3].Value = "DisplayName";
        worksheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;

        int rowIndex = 2;
        foreach (DataRow row in categoryTable.Rows)
        {
            worksheet.Cells[rowIndex, 1].Value = row["CATEGORY_CODE"]?.ToString();
            worksheet.Cells[rowIndex, 2].Value = row["CATEGORY_NAME"]?.ToString();
            worksheet.Cells[rowIndex, 3].Value = row["DISPLAY_NAME"]?.ToString();
            rowIndex++;
        }

        worksheet.Cells[1, 1, Math.Max(rowIndex - 1, 1), 3].AutoFitColumns();
        worksheet.View.FreezePanes(2, 1);
    }

    private static void AddGroupLookupWorksheet(ExcelPackage package, DataTable groupTable, DataTable categoryTable)
    {
        var worksheet = package.Workbook.Worksheets.Add("ReferensiGrup");
        worksheet.Cells[1, 1].Value = "GroupCode";
        worksheet.Cells[1, 2].Value = "GroupName";
        worksheet.Cells[1, 3].Value = "CategoryCode";
        worksheet.Cells[1, 4].Value = "CategoryName";
        worksheet.Cells[1, 5].Value = "DisplayName";
        worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;

        Dictionary<long, (string CategoryCode, string CategoryName)> categoryMap = [];
        foreach (DataRow row in categoryTable.Rows)
        {
            if (row["CATEGORY_ID"] == DBNull.Value)
            {
                continue;
            }

            long categoryId = Convert.ToInt64(row["CATEGORY_ID"]);
            categoryMap[categoryId] = (
                row["CATEGORY_CODE"]?.ToString() ?? string.Empty,
                row["CATEGORY_NAME"]?.ToString() ?? string.Empty);
        }

        int rowIndex = 2;
        foreach (DataRow row in groupTable.Rows)
        {
            string categoryCode = string.Empty;
            string categoryName = string.Empty;

            if (row["CATEGORY_ID"] != DBNull.Value)
            {
                long categoryId = Convert.ToInt64(row["CATEGORY_ID"]);
                if (categoryMap.TryGetValue(categoryId, out (string CategoryCode, string CategoryName) category))
                {
                    categoryCode = category.CategoryCode;
                    categoryName = category.CategoryName;
                }
            }

            worksheet.Cells[rowIndex, 1].Value = row["GROUP_CODE"]?.ToString();
            worksheet.Cells[rowIndex, 2].Value = row["GROUP_NAME"]?.ToString();
            worksheet.Cells[rowIndex, 3].Value = categoryCode;
            worksheet.Cells[rowIndex, 4].Value = categoryName;
            worksheet.Cells[rowIndex, 5].Value = row["DISPLAY_NAME"]?.ToString();
            rowIndex++;
        }

        worksheet.Cells[1, 1, Math.Max(rowIndex - 1, 1), 5].AutoFitColumns();
        worksheet.View.FreezePanes(2, 1);
    }

    private void BtnImport_Click(object sender, EventArgs e)
    {
        if (_previewRows == null || _previewRows.Count == 0)
        {
            XtraMessageBox.Show("Tidak ada data yang siap diimport.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_previewRows.Any(static row => !string.IsNullOrWhiteSpace(row.ErrorMessage)))
        {
            XtraMessageBox.Show("Masih ada baris yang error. Perbaiki file Excel terlebih dahulu.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            btnImport.Enabled = false;
            return;
        }

        if (XtraMessageBox.Show(
                $"Import {_previewRows.Count} baris data aset tetap?",
                "Konfirmasi Import",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        string idData = CompanyInfo.IDDATA?.Trim() ?? string.Empty;
        string userId = LoginInfo.userID;

        btnImport.Enabled = false;
        btnBrowse.Enabled = false;
        btnTemplate.Enabled = false;
        cboSheet.Enabled = false;

        progressBar.Properties.Maximum = _previewRows.Count;
        progressBar.EditValue = 0;

        int success = 0;
        var failures = new List<string>();

        try
        {
            for (int i = 0; i < _previewRows.Count; i++)
            {
                ImportFixedAssetRow row = _previewRows[i];
                try
                {
                    FixedAssetMasterSaveRequest request = BuildSaveRequest(row, idData);
                    FixedAssetQueryServices.SaveAsset(request, userId);
                    success++;
                }
                catch (Exception ex)
                {
                    failures.Add($"Baris {row.ExcelRow}: {ex.Message}");
                }

                progressBar.EditValue = i + 1;
                lblStatus.Text = $"Memproses {i + 1}/{_previewRows.Count}...";
                Application.DoEvents();
            }
        }
        finally
        {
            btnBrowse.Enabled = true;
            btnTemplate.Enabled = true;
            cboSheet.Enabled = true;
        }

        if (failures.Count == 0)
        {
            lblStatus.Text = $"Import selesai. {success} aset berhasil.";
            XtraMessageBox.Show($"Import selesai.\n{success} aset berhasil disimpan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            lblStatus.Text = $"Import selesai. {success} berhasil, {failures.Count} gagal.";
            string detail = string.Join("\n", failures.Take(20));
            if (failures.Count > 20)
                detail += $"\n... dan {failures.Count - 20} error lainnya.";
            XtraMessageBox.Show(
                $"Import selesai dengan error.\n{success} berhasil, {failures.Count} gagal.\n\n{detail}",
                "Import Parsial",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private List<ImportFixedAssetRow> ParseAndValidate(DataTable dt, string idData)
    {
        var rows = new List<ImportFixedAssetRow>(dt.Rows.Count);
        var codesSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var codesToCheck = new List<string>();

        // First pass: parse all rows
        for (int index = 0; index < dt.Rows.Count; index++)
        {
            DataRow dr = dt.Rows[index];
            if (IsRowEmpty(dr))
            {
                continue;
            }

            int excelRow = index + 2;
            var row = new ImportFixedAssetRow { ExcelRow = excelRow };
            var errors = new List<string>();

            // AssetCode (optional)
            row.AssetCode = ReadString(dr, "AssetCode");

            // AssetName (required)
            row.AssetName = ReadString(dr, "AssetName");
            if (string.IsNullOrWhiteSpace(row.AssetName))
                errors.Add("AssetName wajib diisi");

            // CategoryCode (required)
            row.CategoryCode = ReadString(dr, "CategoryCode");
            if (string.IsNullOrWhiteSpace(row.CategoryCode))
                errors.Add("CategoryCode wajib diisi");
            else if (!_categoryLookup.ContainsKey(row.CategoryCode))
                errors.Add($"CategoryCode '{row.CategoryCode}' tidak ditemukan");

            // GroupCode (optional)
            row.GroupCode = ReadString(dr, "GroupCode");
            if (!string.IsNullOrWhiteSpace(row.GroupCode))
            {
                if (!_groupLookup.TryGetValue(row.GroupCode, out var grp))
                    errors.Add($"GroupCode '{row.GroupCode}' tidak ditemukan");
                else if (!string.IsNullOrWhiteSpace(row.CategoryCode)
                         && _categoryLookup.TryGetValue(row.CategoryCode, out long catId)
                         && grp.CategoryId != catId)
                    errors.Add($"GroupCode '{row.GroupCode}' tidak sesuai dengan CategoryCode '{row.CategoryCode}'");
            }

            // AcquisitionDate (required)
            try
            {
                row.AcquisitionDate = ReadDateTime(dr, "AcquisitionDate", excelRow);
            }
            catch (FormatException ex)
            {
                errors.Add(ex.Message);
                row.AcquisitionDate = DateTime.MinValue;
            }

            // InServiceDate (optional)
            row.InServiceDate = ReadOptionalDateTime(dr, "InServiceDate", excelRow, errors);

            // DepreciationStartDate (optional)
            row.DepreciationStartDate = ReadOptionalDateTime(dr, "DepreciationStartDate", excelRow, errors);

            // AcquisitionCost (required)
            try
            {
                row.AcquisitionCost = ReadDecimal(dr, "AcquisitionCost", excelRow, required: true);
                if (row.AcquisitionCost < 0) errors.Add("AcquisitionCost tidak boleh negatif");
            }
            catch (FormatException ex)
            {
                errors.Add(ex.Message);
            }

            // ResidualValue (optional, default 0)
            try
            {
                row.ResidualValue = ReadOptionalDecimal(dr, "ResidualValue", excelRow);
                if (row.ResidualValue < 0) errors.Add("ResidualValue tidak boleh negatif");
            }
            catch (FormatException ex)
            {
                errors.Add(ex.Message);
            }

            // UsefulLifeMonths (required)
            try
            {
                row.UsefulLifeMonths = ReadInt32(dr, "UsefulLifeMonths", excelRow);
                if (row.UsefulLifeMonths <= 0) errors.Add("UsefulLifeMonths harus > 0");
            }
            catch (FormatException ex)
            {
                errors.Add(ex.Message);
            }

            // DepreciationMethod (optional, default SL)
            row.DepreciationMethod = ReadString(dr, "DepreciationMethod");
            if (string.IsNullOrWhiteSpace(row.DepreciationMethod))
                row.DepreciationMethod = "SL";
            else if (!ValidMethods.Contains(row.DepreciationMethod))
                errors.Add($"DepreciationMethod '{row.DepreciationMethod}' tidak valid (SL/DB/NONE)");

            // Status (optional, default ACTIVE)
            row.Status = ReadString(dr, "Status");
            if (string.IsNullOrWhiteSpace(row.Status))
                row.Status = "ACTIVE";
            else if (!ValidStatuses.Contains(row.Status))
                errors.Add($"Status '{row.Status}' tidak valid");

            // Optional string fields
            row.DepartmentId = ReadString(dr, "DepartmentId");
            row.CostCenterId = ReadString(dr, "CostCenterId");
            row.LocationId = ReadString(dr, "LocationId");
            row.VendorId = ReadString(dr, "VendorId");
            row.SerialNo = ReadString(dr, "SerialNo");

            // CurrencyCode (optional, default IDR)
            row.CurrencyCode = ReadString(dr, "CurrencyCode");
            if (string.IsNullOrWhiteSpace(row.CurrencyCode))
                row.CurrencyCode = "IDR";

            // ExchangeRate (optional, default 1)
            try
            {
                row.ExchangeRate = ReadOptionalDecimal(dr, "ExchangeRate", excelRow);
                if (row.ExchangeRate <= 0) row.ExchangeRate = 1m;
            }
            catch (FormatException ex)
            {
                errors.Add(ex.Message);
            }

            // Notes
            row.Notes = ReadString(dr, "Notes");

            // Check in-file duplicate AssetCode
            if (!string.IsNullOrWhiteSpace(row.AssetCode))
            {
                if (!codesSeen.Add(row.AssetCode.Trim().ToUpperInvariant()))
                    errors.Add($"AssetCode '{row.AssetCode}' duplikat dalam file");
                else
                    codesToCheck.Add(row.AssetCode.Trim().ToUpperInvariant());
            }

            row.ErrorMessage = errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
            rows.Add(row);
        }

        // Check DB duplicates for explicit asset codes
        if (codesToCheck.Count > 0)
        {
            HashSet<string> dbDuplicates = FixedAssetQueryServices.CheckAssetCodesExist(idData, codesToCheck);
            if (dbDuplicates.Count > 0)
            {
                foreach (var row in rows)
                {
                    if (!string.IsNullOrWhiteSpace(row.AssetCode)
                        && dbDuplicates.Contains(row.AssetCode.Trim().ToUpperInvariant()))
                    {
                        string msg = $"AssetCode '{row.AssetCode}' sudah ada di database";
                        row.ErrorMessage = string.IsNullOrWhiteSpace(row.ErrorMessage) ? msg : row.ErrorMessage + "; " + msg;
                    }
                }
            }
        }

        return rows;
    }

    private FixedAssetMasterSaveRequest BuildSaveRequest(ImportFixedAssetRow row, string idData)
    {
        long categoryId = _categoryLookup.TryGetValue(row.CategoryCode, out long cId) ? cId : 0;
        long? groupId = null;
        if (!string.IsNullOrWhiteSpace(row.GroupCode) && _groupLookup.TryGetValue(row.GroupCode, out var grp))
            groupId = grp.GroupId;

        return new FixedAssetMasterSaveRequest
        {
            AssetId = 0,
            IdData = idData,
            AssetCode = row.AssetCode?.Trim() ?? string.Empty,
            AssetName = row.AssetName?.Trim() ?? string.Empty,
            CategoryId = categoryId,
            GroupId = groupId,
            AcquisitionDate = row.AcquisitionDate,
            InServiceDate = row.InServiceDate,
            DepreciationStartDate = row.DepreciationStartDate,
            AcquisitionCost = row.AcquisitionCost,
            ResidualValue = row.ResidualValue,
            UsefulLifeMonths = row.UsefulLifeMonths,
            DepreciationMethod = row.DepreciationMethod?.Trim().ToUpperInvariant() ?? "SL",
            Status = row.Status?.Trim().ToUpperInvariant() ?? "ACTIVE",
            DepartmentId = row.DepartmentId ?? string.Empty,
            CostCenterId = row.CostCenterId ?? string.Empty,
            LocationId = row.LocationId ?? string.Empty,
            VendorId = row.VendorId ?? string.Empty,
            SerialNo = row.SerialNo ?? string.Empty,
            CurrencyCode = row.CurrencyCode?.Trim().ToUpperInvariant() ?? "IDR",
            ExchangeRate = row.ExchangeRate <= 0 ? 1m : row.ExchangeRate,
            Notes = row.Notes ?? string.Empty
        };
    }

    // ── Column Validation ────────────────────────────────────
    private static string ValidateColumnStructure(DataTable dt, string[] expectedColumns)
    {
        if (dt.Columns.Count != expectedColumns.Length)
        {
            return $"Jumlah kolom tidak sesuai. Diharapkan {expectedColumns.Length}, ditemukan {dt.Columns.Count}.";
        }

        for (int i = 0; i < expectedColumns.Length; i++)
        {
            string actual = dt.Columns[i].ColumnName?.Trim() ?? string.Empty;
            if (!string.Equals(actual, expectedColumns[i], StringComparison.OrdinalIgnoreCase))
                return $"Kolom ke-{i + 1} harus '{expectedColumns[i]}', ditemukan '{actual}'.";
        }
        return null;
    }

    private void ResetPreviewState(string status = "Siap.")
    {
        _previewRows = null;
        gridControl.DataSource = null;
        progressBar.Properties.Maximum = 1;
        progressBar.EditValue = 0;
        lblStatus.Text = status;
    }

    private static bool IsRowEmpty(DataRow row)
    {
        foreach (object value in row.ItemArray)
        {
            if (value != DBNull.Value && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                return false;
            }
        }

        return true;
    }

    // ── Read Helpers ─────────────────────────────────────────
    private static string ReadString(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName)) return string.Empty;
        object value = row[columnName];
        return value == DBNull.Value ? string.Empty : value.ToString()?.Trim() ?? string.Empty;
    }

    private static DateTime ReadDateTime(DataRow row, string columnName, int excelRow)
    {
        object value = row[columnName];
        if (value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
            throw new FormatException($"{columnName} wajib diisi");

        if (value is DateTime dt) return dt;
        if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed)) return parsed;
        if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)) return parsed;

        throw new FormatException($"{columnName} tidak valid ({value})");
    }

    private static DateTime? ReadOptionalDateTime(DataRow row, string columnName, int excelRow, List<string> errors)
    {
        if (!row.Table.Columns.Contains(columnName)) return null;
        object value = row[columnName];
        if (value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString())) return null;

        if (value is DateTime dt) return dt;
        if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed)) return parsed;
        if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)) return parsed;

        errors.Add($"{columnName} tidak valid ({value})");
        return null;
    }

    private static decimal ReadDecimal(DataRow row, string columnName, int excelRow, bool required = false)
    {
        object value = row[columnName];
        if (value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
        {
            if (required) throw new FormatException($"{columnName} wajib diisi");
            return 0m;
        }

        if (value is decimal dec) return dec;
        if (value is double dbl) return Convert.ToDecimal(dbl);
        if (value is float flt) return Convert.ToDecimal(flt);

        if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed)) return parsed;
        if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed)) return parsed;

        throw new FormatException($"{columnName} tidak valid ({value})");
    }

    private static decimal ReadOptionalDecimal(DataRow row, string columnName, int excelRow)
    {
        if (!row.Table.Columns.Contains(columnName)) return 0m;
        return ReadDecimal(row, columnName, excelRow, required: false);
    }

    private static int ReadInt32(DataRow row, string columnName, int excelRow)
    {
        object value = row[columnName];
        if (value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
            throw new FormatException($"{columnName} wajib diisi");

        if (value is double dbl) return Convert.ToInt32(dbl);
        if (value is decimal dec) return Convert.ToInt32(dec);

        if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)) return parsed;
        if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.CurrentCulture, out parsed)) return parsed;

        throw new FormatException($"{columnName} tidak valid ({value})");
    }

    // ── Inner DTO ────────────────────────────────────────────
    public sealed class ImportFixedAssetRow
    {
        public int ExcelRow { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public DateTime AcquisitionDate { get; set; }
        public DateTime? InServiceDate { get; set; }
        public DateTime? DepreciationStartDate { get; set; }
        public decimal AcquisitionCost { get; set; }
        public decimal ResidualValue { get; set; }
        public int UsefulLifeMonths { get; set; }
        public string DepreciationMethod { get; set; } = "SL";
        public string Status { get; set; } = "ACTIVE";
        public string DepartmentId { get; set; } = string.Empty;
        public string CostCenterId { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public string VendorId { get; set; } = string.Empty;
        public string SerialNo { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = "IDR";
        public decimal ExchangeRate { get; set; } = 1m;
        public string Notes { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
