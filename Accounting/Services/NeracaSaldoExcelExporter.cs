using Accounting.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Accounting.Services
{
    public static class NeracaSaldoExcelExporter
    {
        public const int HeaderRow = 6;
        public const int FirstDataRow = 7;
        public const int ColumnCount = 15;
        public const string AccountingNumberFormat = "#,##0.00;[Red](#,##0.00);-";

        private static readonly string[] ColumnHeaders =
        [
            "Kode Akun", "Nama Akun", "Saldo Awal", "Jan", "Feb", "Mar", "Apr", "Mei",
            "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des"
        ];

        public static byte[] CreateWorkbook(
            IReadOnlyList<NeracaSaldoRow> rows,
            NeracaSaldoReportMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(rows);
            ArgumentNullException.ThrowIfNull(metadata);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Neraca Saldo");

            ConfigureMetadata(worksheet, metadata);
            ConfigureHeaders(worksheet);
            WriteRows(worksheet, rows);
            ConfigureLayout(worksheet, rows.Count);

            return package.GetAsByteArray();
        }

        public static string CreateFileName(NeracaSaldoReportMetadata metadata)
        {
            ArgumentNullException.ThrowIfNull(metadata);

            HashSet<char> invalidCharacters = Path.GetInvalidFileNameChars().ToHashSet();
            string safeCompanyName = new(metadata.CompanyName
                .Select(character => invalidCharacters.Contains(character) ? '_' : character)
                .ToArray());
            safeCompanyName = string.Join("_", safeCompanyName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));

            if (string.IsNullOrWhiteSpace(safeCompanyName))
            {
                safeCompanyName = "Perusahaan";
            }

            return $"Neraca_Saldo_{safeCompanyName}_{metadata.Year}_{metadata.GeneratedAt:yyyyMMdd_HHmmss}.xlsx";
        }

        private static void ConfigureMetadata(
            ExcelWorksheet worksheet,
            NeracaSaldoReportMetadata metadata)
        {
            worksheet.Cells[1, 1, 1, ColumnCount].Merge = true;
            worksheet.Cells[1, 1].Value = metadata.CompanyName;
            worksheet.Cells[2, 1, 2, ColumnCount].Merge = true;
            worksheet.Cells[2, 1].Value = metadata.Region;
            worksheet.Cells[3, 1, 3, ColumnCount].Merge = true;
            worksheet.Cells[3, 1].Value = "NERACA SALDO TAHUNAN";
            worksheet.Cells[4, 1, 4, ColumnCount].Merge = true;
            worksheet.Cells[4, 1].Value = $"Tahun {metadata.Year} | User: {metadata.UserId} | Dibuat: {metadata.GeneratedAt:dd-MM-yyyy HH:mm}";

            worksheet.Cells[1, 1, 4, ColumnCount].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1, 4, ColumnCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[3, 1].Style.Font.Bold = true;
            worksheet.Cells[3, 1].Style.Font.Size = 13;
        }

        private static void ConfigureHeaders(ExcelWorksheet worksheet)
        {
            for (int index = 0; index < ColumnHeaders.Length; index++)
            {
                worksheet.Cells[HeaderRow, index + 1].Value = ColumnHeaders[index];
            }

            ExcelRange headerRange = worksheet.Cells[HeaderRow, 1, HeaderRow, ColumnCount];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.Color.SetColor(Color.White);
            headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(47, 84, 150));
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private static void WriteRows(ExcelWorksheet worksheet, IReadOnlyList<NeracaSaldoRow> rows)
        {
            for (int index = 0; index < rows.Count; index++)
            {
                NeracaSaldoRow row = rows[index];
                int worksheetRow = FirstDataRow + index;

                worksheet.Cells[worksheetRow, 1].Value = row.KodeAkun;
                worksheet.Cells[worksheetRow, 2].Value = row.NamaAkun;
                worksheet.Cells[worksheetRow, 3].Value = row.SaldoAwal;
                worksheet.Cells[worksheetRow, 4].Value = row.Januari;
                worksheet.Cells[worksheetRow, 5].Value = row.Februari;
                worksheet.Cells[worksheetRow, 6].Value = row.Maret;
                worksheet.Cells[worksheetRow, 7].Value = row.April;
                worksheet.Cells[worksheetRow, 8].Value = row.Mei;
                worksheet.Cells[worksheetRow, 9].Value = row.Juni;
                worksheet.Cells[worksheetRow, 10].Value = row.Juli;
                worksheet.Cells[worksheetRow, 11].Value = row.Agustus;
                worksheet.Cells[worksheetRow, 12].Value = row.September;
                worksheet.Cells[worksheetRow, 13].Value = row.Oktober;
                worksheet.Cells[worksheetRow, 14].Value = row.November;
                worksheet.Cells[worksheetRow, 15].Value = row.Desember;

                worksheet.Cells[worksheetRow, 2].Style.Indent = Math.Max(0, row.Level - 1);
                if (row.IsHeader)
                {
                    ExcelRange headerAccountRange = worksheet.Cells[worksheetRow, 1, worksheetRow, ColumnCount];
                    headerAccountRange.Style.Font.Bold = true;
                    headerAccountRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerAccountRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
                }
            }
        }

        private static void ConfigureLayout(ExcelWorksheet worksheet, int rowCount)
        {
            int lastRow = Math.Max(HeaderRow, FirstDataRow + rowCount - 1);
            worksheet.Cells[FirstDataRow, 3, lastRow, ColumnCount].Style.Numberformat.Format = AccountingNumberFormat;
            worksheet.Cells[HeaderRow, 1, lastRow, ColumnCount].AutoFilter = true;
            worksheet.View.FreezePanes(FirstDataRow, 3);

            worksheet.Cells[HeaderRow, 1, lastRow, ColumnCount].AutoFitColumns();
            worksheet.Column(1).Width = Math.Clamp(worksheet.Column(1).Width, 12D, 24D);
            worksheet.Column(2).Width = Math.Clamp(worksheet.Column(2).Width, 24D, 60D);
            worksheet.Cells[FirstDataRow, 2, lastRow, 2].Style.WrapText = true;
            for (int column = 3; column <= ColumnCount; column++)
            {
                worksheet.Column(column).Width = Math.Clamp(worksheet.Column(column).Width, 12D, 20D);
            }

            worksheet.Cells[HeaderRow, 1, lastRow, ColumnCount].Style.Border.Bottom.Style = ExcelBorderStyle.Hair;
            worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
            worksheet.PrinterSettings.PaperSize = ePaperSize.A3;
            worksheet.PrinterSettings.FitToPage = true;
            worksheet.PrinterSettings.FitToWidth = 1;
            worksheet.PrinterSettings.FitToHeight = 0;
            worksheet.PrinterSettings.RepeatRows = worksheet.Cells[$"1:{HeaderRow}"];
        }
    }
}
