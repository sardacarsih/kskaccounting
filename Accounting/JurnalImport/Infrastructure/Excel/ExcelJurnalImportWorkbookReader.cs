using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Accounting.JurnalImport.Application;
using Accounting.JurnalImport.Domain;
using ExcelDataReader;

namespace Accounting.JurnalImport.Infrastructure.Excel;

public sealed class ExcelJurnalImportWorkbookReader : IJurnalImportWorkbookReader
{
    public IReadOnlyList<string> GetSheets(string path)
    {
        using DataSet dataSet = ReadWorkbook(path);
        return dataSet.Tables.Cast<DataTable>().Select(table => table.TableName).ToList();
    }

    public IReadOnlyList<JurnalImportRow> ReadSheet(string path, string sheetName)
    {
        using DataSet dataSet = ReadWorkbook(path);
        DataTable? table = dataSet.Tables.Cast<DataTable>().FirstOrDefault(item => item.TableName == sheetName);
        if (table == null)
        {
            throw new JurnalImportValidationException(
                [new JurnalImportValidationIssue("SHEET_NOT_FOUND", $"Sheet {sheetName} tidak ditemukan.", "Sheet", sheetName)]);
        }

        JurnalImportValidationException.ThrowIfAny(
            JurnalImportTemplateValidator.Validate(table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList()));

        return JurnalImportRowMapper.FromDataTable(table);
    }

    private static DataSet ReadWorkbook(string path)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
        return reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        });
    }
}
