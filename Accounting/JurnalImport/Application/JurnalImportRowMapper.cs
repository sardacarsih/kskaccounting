using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Accounting.JurnalImport.Domain;

namespace Accounting.JurnalImport.Application;

public static class JurnalImportRowMapper
{
    public static IReadOnlyList<JurnalImportRow> FromDataTable(DataTable source)
    {
        if (source.Rows.Count == 0)
        {
            return [];
        }

        return source.AsEnumerable()
            .Select((row, index) => FromDataRow(row, index + 2))
            .ToList();
    }

    public static DataTable ToStageDataTable(JurnalImportScope scope, IReadOnlyList<JurnalImportRow> rows)
    {
        DataTable table = new();
        table.Columns.Add("NOJURNAL", typeof(string));
        table.Columns.Add("TANGGAL", typeof(DateTime));
        table.Columns.Add("BARIS", typeof(int));
        table.Columns.Add("KODE", typeof(string));
        table.Columns.Add("REKENING", typeof(string));
        table.Columns.Add("DEBET", typeof(decimal));
        table.Columns.Add("KREDIT", typeof(decimal));
        table.Columns.Add("KETERANGAN", typeof(string));
        table.Columns.Add("POSTED", typeof(string));
        table.Columns.Add("PERIODE", typeof(string));
        table.Columns.Add("IDDATA", typeof(string));
        table.Columns.Add("USERID", typeof(string));
        table.Columns.Add("GLYEAR", typeof(int));
        table.Columns.Add("GLMONTH", typeof(int));

        foreach (JurnalImportRow row in rows)
        {
            table.Rows.Add(
                row.NoJurnal,
                row.Tanggal,
                row.Baris,
                string.IsNullOrWhiteSpace(row.Kode) ? DBNull.Value : row.Kode,
                row.Rekening,
                row.Debet,
                row.Kredit,
                row.Keterangan,
                row.Posted,
                scope.Period,
                scope.IdData,
                scope.UserId,
                scope.Year,
                scope.Month);
        }

        return table;
    }

    private static JurnalImportRow FromDataRow(DataRow row, int excelRow)
    {
        return new JurnalImportRow
        {
            NoJurnal = ReadString(row, "NoJurnal", "NOJURNAL"),
            Tanggal = ReadDateTime(row, excelRow, "Tanggal", "TANGGAL"),
            Baris = ReadInt32(row, excelRow, "RowNo", "BARIS", "Baris"),
            Kode = ReadString(row, "Kode", "KODE"),
            Rekening = ReadString(row, "Rekening", "REKENING"),
            Debet = ReadDecimal(row, excelRow, "Debet", "DEBET"),
            Kredit = ReadDecimal(row, excelRow, "Kredit", "KREDIT"),
            Keterangan = ReadString(row, "Keterangan", "KETERANGAN"),
            Posted = ReadString(row, "Posted", "POSTED"),
            Periode = ReadString(row, "Periode", "PERIODE")
        };
    }

    private static string ReadString(DataRow row, params string[] names)
    {
        object value = ReadValue(row, names);
        return value == DBNull.Value ? string.Empty : value.ToString()?.Trim() ?? string.Empty;
    }

    private static int ReadInt32(DataRow row, int excelRow, params string[] names)
    {
        object value = ReadValue(row, names);
        if (value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
        {
            throw new FormatException($"Baris Excel {excelRow}: kolom {names[0]} wajib diisi.");
        }

        if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
        {
            return parsed;
        }

        if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.CurrentCulture, out parsed))
        {
            return parsed;
        }

        throw new FormatException($"Baris Excel {excelRow}: kolom {names[0]} tidak valid ({value}).");
    }

    private static decimal ReadDecimal(DataRow row, int excelRow, params string[] names)
    {
        object value = ReadValue(row, names);
        if (value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return 0m;
        }

        if (value is decimal dec) return dec;
        if (value is double dbl) return Convert.ToDecimal(dbl, CultureInfo.InvariantCulture);
        if (value is float flt) return Convert.ToDecimal(flt, CultureInfo.InvariantCulture);

        if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed))
        {
            return parsed;
        }

        if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
        {
            return parsed;
        }

        throw new FormatException($"Baris Excel {excelRow}: kolom {names[0]} tidak valid ({value}).");
    }

    private static DateTime ReadDateTime(DataRow row, int excelRow, params string[] names)
    {
        object value = ReadValue(row, names);
        if (value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
        {
            throw new FormatException($"Baris Excel {excelRow}: kolom {names[0]} wajib diisi.");
        }

        if (value is DateTime dateTime)
        {
            return dateTime;
        }

        if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed))
        {
            return parsed;
        }

        if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
        {
            return parsed;
        }

        throw new FormatException($"Baris Excel {excelRow}: kolom {names[0]} tidak valid ({value}).");
    }

    private static object ReadValue(DataRow row, params string[] names)
    {
        foreach (string name in names)
        {
            if (row.Table.Columns.Contains(name))
            {
                return row[name];
            }
        }

        throw new InvalidOperationException($"Kolom {names[0]} tidak ditemukan.");
    }
}
