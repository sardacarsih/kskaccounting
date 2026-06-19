using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Accounting.CoaImport.Application;
using Accounting.CoaImport.Domain;
using ExcelDataReader;

namespace Accounting.CoaImport.Infrastructure.Excel;

public sealed class ExcelCoaImportWorkbookReader : ICoaImportWorkbookReader
{
    public IReadOnlyList<string> GetSheets(string path)
    {
        using DataSet result = ReadWorkbook(path);
        return result.Tables.Cast<DataTable>().Select(table => table.TableName).ToList();
    }

    public IReadOnlyList<CoaImportRow> ReadSheet(string path, string sheetName, CoaImportKind kind)
    {
        using DataSet result = ReadWorkbook(path);
        DataTable? table = result.Tables.Cast<DataTable>().FirstOrDefault(item => item.TableName == sheetName);
        if (table is null)
        {
            throw new InvalidOperationException($"Sheet '{sheetName}' tidak ditemukan.");
        }

        IReadOnlyList<string> columns = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();
        IReadOnlyList<CoaImportValidationIssue> issues = CoaImportTemplateValidator.Validate(columns, kind);
        if (issues.Count > 0)
        {
            throw new CoaImportValidationException(issues);
        }

        return table.Rows.Cast<DataRow>()
            .Select((row, index) => MapRow(row, kind, index + 2))
            .ToList();
    }

    private static DataSet ReadWorkbook(string path)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
        return reader.AsDataSet(CreateDataSetConfiguration());
    }

    private static ExcelDataSetConfiguration CreateDataSetConfiguration()
    {
        return new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        };
    }

    private static CoaImportRow MapRow(DataRow row, CoaImportKind kind, int excelRowNumber)
    {
        return new CoaImportRow
        {
            Account = GetString(row, "Account"),
            NamaPerkiraan = GetString(row, "Nama Perkiraan"),
            Jenis = GetString(row, "Jenis"),
            Level = GetString(row, "Level"),
            Induk = GetString(row, "Induk"),
            Gen = GetString(row, "Gen"),
            Posisi = GetString(row, "Saldo Normal"),
            AwalTahun = GetDecimal(row, "Awal Tahun", excelRowNumber),
            Divisi = kind == CoaImportKind.Kebun ? GetString(row, "Divisi") : string.Empty,
            Blok = kind == CoaImportKind.Kebun ? GetString(row, "Blok") : string.Empty,
            TahunTanam = kind == CoaImportKind.Kebun ? GetString(row, "TahunTanam") : string.Empty
        };
    }

    private static string GetString(DataRow row, string columnName)
    {
        return row[columnName]?.ToString() ?? string.Empty;
    }

    private static decimal GetDecimal(DataRow row, string columnName, int excelRowNumber)
    {
        object value = row[columnName];
        if (value is null || value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return 0m;
        }

        if (value is decimal decimalValue)
        {
            return decimalValue;
        }

        if (decimal.TryParse(value.ToString(), out decimal result))
        {
            return result;
        }

        throw new FormatException($"Kolom {columnName} pada baris {excelRowNumber} harus bernilai angka.");
    }
}
