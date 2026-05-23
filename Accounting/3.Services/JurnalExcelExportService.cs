using Accounting._1.Interface;
using Accounting.Model;
using Accounting.Services;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Accounting.BusinessLayer
{
    public sealed class JurnalExcelExportService
    {
        public void ExportJurnalDetails(IEnumerable<JurnalDetailDTO> rows, string fileNamePrefix)
        {
            AuthorizationService.EnsureCanExportJurnal();

            List<JurnalDetailDTO> data = rows.ToList();
            if (data.Count == 0)
            {
                throw new InvalidOperationException("Data tidak ditemukan");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Jurnal Entries");

            worksheet.Cells["A1"].LoadFromCollection(data, true);
            worksheet.DeleteColumn(1, 2);
            ApplyStandardHeader(worksheet);
            ApplyStandardFormats(worksheet, data.Count);
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            string filename = Path.Combine(Path.GetTempPath(), $"{fileNamePrefix}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            File.WriteAllBytes(filename, package.GetAsByteArray());
            OpenFile(filename);
        }

        public void ExportAisJurnal(IEnumerable<AIS_JURNAL_FINAL> rows, string fileNamePrefix)
        {
            AuthorizationService.EnsureCanExportJurnal();

            List<AIS_JURNAL_FINAL> data = rows.ToList();
            if (data.Count == 0)
            {
                throw new InvalidOperationException("Data tidak ditemukan");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Jurnal Entries");

            worksheet.Cells["A1"].LoadFromCollection(data, true);
            ApplyStandardHeader(worksheet);
            ApplyStandardFormats(worksheet, data.Count);
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            string filename = Path.Combine(Path.GetTempPath(), $"{fileNamePrefix}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            File.WriteAllBytes(filename, package.GetAsByteArray());
            OpenFile(filename);
        }

        public void ExportKasirDataTable(DataTable source, string fileNamePrefix)
        {
            AuthorizationService.EnsureCanExportJurnal();

            if (source == null || source.Rows.Count == 0)
            {
                throw new InvalidOperationException("Data tidak ditemukan");
            }

            DataTable data = source.Copy();
            string[] columnsToRemove = ["IDDATA", "USERID", "GLYEAR", "GLMONTH"];
            foreach (string columnName in columnsToRemove)
            {
                if (data.Columns.Contains(columnName))
                {
                    data.Columns.Remove(columnName);
                }
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("JurnalKasir");

            ApplyStandardHeader(worksheet);
            for (int index = 0; index < data.Rows.Count; index++)
            {
                DataRow row = data.Rows[index];
                worksheet.Cells[index + 2, 1].Value = row["NOJURNAL"];
                worksheet.Cells[index + 2, 2].Value = row["TANGGAL"];
                worksheet.Cells[index + 2, 3].Formula = index == 0 ? "1" : $"IF(A{index + 2}<>A{index + 1},1,C{index + 1}+1)";
                worksheet.Cells[index + 2, 4].Value = row["KODE"];
                worksheet.Cells[index + 2, 5].Value = row["REKENING"];
                worksheet.Cells[index + 2, 6].Value = row["DEBET"];
                worksheet.Cells[index + 2, 7].Value = row["KREDIT"];
                worksheet.Cells[index + 2, 8].Value = row["KETERANGAN"];
                worksheet.Cells[index + 2, 9].Value = row["POSTED"];
                worksheet.Cells[index + 2, 10].Value = row["PERIODE"];
            }

            ApplyStandardFormats(worksheet, data.Rows.Count);
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            string filename = Path.Combine(Path.GetTempPath(), $"{fileNamePrefix}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            File.WriteAllBytes(filename, package.GetAsByteArray());
            OpenFile(filename);
        }

        private static void ApplyStandardHeader(ExcelWorksheet worksheet)
        {
            worksheet.Cells[1, 1].Value = "NoJurnal";
            worksheet.Cells[1, 2].Value = "Tanggal";
            worksheet.Cells[1, 3].Value = "RowNo";
            worksheet.Cells[1, 4].Value = "Kode";
            worksheet.Cells[1, 5].Value = "Rekening";
            worksheet.Cells[1, 6].Value = "Debet";
            worksheet.Cells[1, 7].Value = "Kredit";
            worksheet.Cells[1, 8].Value = "Keterangan";
            worksheet.Cells[1, 9].Value = "Posted";
            worksheet.Cells[1, 10].Value = "Periode";
        }

        private static void ApplyStandardFormats(ExcelWorksheet worksheet, int rowCount)
        {
            if (rowCount <= 0)
            {
                return;
            }

            worksheet.Cells[2, 2, rowCount + 1, 2].Style.Numberformat.Format = "dd-MMM-yyyy";
            worksheet.Cells[2, 3, rowCount + 1, 3].Formula = string.Format("IF(A2<>A1,1,C1+1)", new ExcelAddress(2, 3, rowCount + 1, 3).Address);

            const string positiveFormat = "#,##0.00_)";
            const string negativeFormat = "(#,##0.00)";
            const string zeroFormat = "-_)";
            string fullNumberFormat = positiveFormat + ";" + negativeFormat + ";" + zeroFormat;
            worksheet.Cells[2, 6, rowCount + 1, 7].Style.Numberformat.Format = fullNumberFormat;
        }

        private static void OpenFile(string path)
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}
