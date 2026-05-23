using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

string outputPath = args.Length > 0 ? args[0] : "Template_Import_FixedAsset.xlsx";

using var package = new ExcelPackage();

// ═══════════════════════════════════════════════════════
// Sheet 1: Template Import
// ═══════════════════════════════════════════════════════
var ws = package.Workbook.Worksheets.Add("FixedAsset");

string[] headers =
[
    "AssetCode", "AssetName", "CategoryCode", "GroupCode",
    "AcquisitionDate", "InServiceDate", "DepreciationStartDate",
    "AcquisitionCost", "ResidualValue", "UsefulLifeMonths",
    "DepreciationMethod", "Status",
    "DepartmentId", "CostCenterId", "LocationId", "VendorId",
    "SerialNo", "CurrencyCode", "ExchangeRate", "Notes"
];

// Header row
for (int i = 0; i < headers.Length; i++)
{
    var cell = ws.Cells[1, i + 1];
    cell.Value = headers[i];
    cell.Style.Font.Bold = true;
    cell.Style.Font.Color.SetColor(Color.White);
    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 85, 151));
    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
    cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(31, 56, 100));
}

// Keterangan row (row 2)
string[] keterangan =
[
    "Kosong = auto\nFA-yyyyMM-NNNNNN",
    "* WAJIB",
    "* WAJIB\nHarus cocok ACCT_FA_CATEGORY",
    "Harus cocok ACCT_FA_GROUP\n& sesuai kategori",
    "* WAJIB\ndd-MMM-yyyy",
    "dd-MMM-yyyy",
    "dd-MMM-yyyy",
    "* WAJIB\n>= 0",
    "Default: 0\n>= 0",
    "* WAJIB\n> 0",
    "Default: SL\nSL / DB / NONE",
    "Default: ACTIVE\nDRAFT, ACTIVE,\nUNDER_CONSTRUCTION, dll",
    "",
    "",
    "",
    "",
    "",
    "Default: IDR",
    "Default: 1\n> 0",
    ""
];

for (int i = 0; i < keterangan.Length; i++)
{
    var cell = ws.Cells[2, i + 1];
    cell.Value = keterangan[i];
    cell.Style.Font.Size = 9;
    cell.Style.Font.Italic = true;
    cell.Style.Font.Color.SetColor(Color.FromArgb(89, 89, 89));
    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
    cell.Style.WrapText = true;
    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
}
ws.Row(2).Height = 52;

// Contoh baris data (row 3-5)
object?[][] sampleData =
[
    [
        "", "Laptop Dell Latitude 5540", "IT", "LAPTOP",
        new DateTime(2026, 1, 15), new DateTime(2026, 1, 20), new DateTime(2026, 2, 1),
        15000000, 1000000, 48,
        "SL", "ACTIVE",
        "IT-DEPT", "CC-100", "GDG-A", "V-001",
        "SN-DELL-001", "IDR", 1, "Laptop untuk staf IT"
    ],
    [
        "", "Kendaraan Operasional Toyota Avanza", "KDR", "",
        new DateTime(2026, 3, 1), null, null,
        250000000, 50000000, 96,
        "DB", "ACTIVE",
        "OPS", "CC-200", "POOL", "V-002",
        "B 1234 ABC", "IDR", 1, "Kendaraan dinas"
    ],
    [
        "FA-CUSTOM-001", "Gedung Kantor Pusat", "BNG", "",
        new DateTime(2025, 6, 1), new DateTime(2025, 7, 1), new DateTime(2025, 7, 1),
        5000000000L, 500000000, 240,
        "SL", "ACTIVE",
        "GA", "CC-300", "GDG-PUSAT", "",
        "", "IDR", 1, "Gedung kantor pusat 3 lantai"
    ]
];

for (int r = 0; r < sampleData.Length; r++)
{
    int row = r + 3;
    for (int c = 0; c < sampleData[r].Length; c++)
    {
        var cell = ws.Cells[row, c + 1];
        object? val = sampleData[r][c];

        if (val == null)
        {
            cell.Value = "";
        }
        else if (val is DateTime dt)
        {
            cell.Value = dt;
            cell.Style.Numberformat.Format = "dd-MMM-yyyy";
        }
        else if (val is int or long or decimal or double)
        {
            cell.Value = val;
            if (c == 7 || c == 8) // AcquisitionCost, ResidualValue
                cell.Style.Numberformat.Format = "#,##0";
            else if (c == 18) // ExchangeRate
                cell.Style.Numberformat.Format = "#,##0.####";
        }
        else
        {
            cell.Value = val;
        }
    }

    // Alternate row color
    if (r % 2 == 0)
    {
        ws.Cells[row, 1, row, headers.Length].Style.Fill.PatternType = ExcelFillStyle.Solid;
        ws.Cells[row, 1, row, headers.Length].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(235, 241, 252));
    }
}

// Column widths
int[] widths = [22, 38, 16, 16, 16, 16, 20, 18, 16, 16, 18, 20, 14, 14, 14, 12, 18, 14, 14, 30];
for (int i = 0; i < widths.Length; i++)
    ws.Column(i + 1).Width = widths[i];

// Freeze panes: header + keterangan
ws.View.FreezePanes(3, 1);

// Auto filter
ws.Cells[1, 1, 1, headers.Length].AutoFilter = true;

// ═══════════════════════════════════════════════════════
// Sheet 2: Panduan
// ═══════════════════════════════════════════════════════
var guide = package.Workbook.Worksheets.Add("Panduan");

guide.Cells[1, 1].Value = "Panduan Import Aset Tetap";
guide.Cells[1, 1].Style.Font.Bold = true;
guide.Cells[1, 1].Style.Font.Size = 14;

string[][] guideData =
[
    ["No", "Kolom", "Wajib", "Default", "Keterangan"],
    ["1", "AssetCode", "-", "Auto FA-yyyyMM-NNNNNN", "Kode aset. Kosongkan untuk auto-generate."],
    ["2", "AssetName", "Ya", "-", "Nama aset tetap."],
    ["3", "CategoryCode", "Ya", "-", "Kode kategori. Harus sesuai dengan data di tabel ACCT_FA_CATEGORY."],
    ["4", "GroupCode", "-", "-", "Kode kelompok. Harus sesuai ACCT_FA_GROUP dan cocok dengan kategori."],
    ["5", "AcquisitionDate", "Ya", "-", "Tanggal perolehan aset. Format: dd-MMM-yyyy."],
    ["6", "InServiceDate", "-", "null", "Tanggal mulai digunakan. Format: dd-MMM-yyyy."],
    ["7", "DepreciationStartDate", "-", "null", "Tanggal mulai penyusutan. Format: dd-MMM-yyyy."],
    ["8", "AcquisitionCost", "Ya", "-", "Nilai perolehan. Harus >= 0."],
    ["9", "ResidualValue", "-", "0", "Nilai residu/sisa. Harus >= 0."],
    ["10", "UsefulLifeMonths", "Ya", "-", "Masa manfaat dalam bulan. Harus > 0."],
    ["11", "DepreciationMethod", "-", "SL", "Metode penyusutan: SL (Garis Lurus), DB (Saldo Menurun), NONE (Tanpa Penyusutan)."],
    ["12", "Status", "-", "ACTIVE", "Status aset: DRAFT, ACTIVE, UNDER_CONSTRUCTION, DISPOSED, SOLD, TRANSFERRED, WRITTEN_OFF, RETIRED."],
    ["13", "DepartmentId", "-", "-", "ID departemen (opsional)."],
    ["14", "CostCenterId", "-", "-", "ID pusat biaya (opsional)."],
    ["15", "LocationId", "-", "-", "ID lokasi (opsional)."],
    ["16", "VendorId", "-", "-", "ID vendor/pemasok (opsional)."],
    ["17", "SerialNo", "-", "-", "Nomor seri aset (opsional)."],
    ["18", "CurrencyCode", "-", "IDR", "Kode mata uang."],
    ["19", "ExchangeRate", "-", "1", "Kurs mata uang. Harus > 0."],
    ["20", "Notes", "-", "-", "Catatan tambahan (opsional)."]
];

for (int r = 0; r < guideData.Length; r++)
{
    int row = r + 3;
    for (int c = 0; c < guideData[r].Length; c++)
    {
        guide.Cells[row, c + 1].Value = guideData[r][c];
    }

    if (r == 0)
    {
        for (int c = 0; c < guideData[r].Length; c++)
        {
            guide.Cells[row, c + 1].Style.Font.Bold = true;
            guide.Cells[row, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            guide.Cells[row, c + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 85, 151));
            guide.Cells[row, c + 1].Style.Font.Color.SetColor(Color.White);
        }
    }
}

guide.Column(1).Width = 5;
guide.Column(2).Width = 24;
guide.Column(3).Width = 8;
guide.Column(4).Width = 26;
guide.Column(5).Width = 70;

// ═══════════════════════════════════════════════════════
// Save
// ═══════════════════════════════════════════════════════
package.SaveAs(new FileInfo(outputPath));
Console.WriteLine($"Template berhasil dibuat: {Path.GetFullPath(outputPath)}");
