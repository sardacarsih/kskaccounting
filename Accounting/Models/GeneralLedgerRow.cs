using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.Model;

/// <summary>
/// Represents the validated ten-column contract consumed by the General Ledger reports and exports.
/// </summary>
public sealed class GeneralLedgerRow
{
    /// <summary>
    /// Gets the ordered columns required by the General Ledger report templates.
    /// </summary>
    public static readonly IReadOnlyList<string> RequiredColumns =
    [
        "BARIS", "PARAM", "PERIODE", "KODE", "REKENING",
        "NOJURNAL", "TANGGAL", "KETERANGAN", "DEBET", "KREDIT"
    ];

    public int Baris { get; init; }

    public int Param { get; init; }

    public string Periode { get; init; } = string.Empty;

    public string Kode { get; init; } = string.Empty;

    public string Rekening { get; init; } = string.Empty;

    public string NoJurnal { get; init; } = string.Empty;

    public DateTime? Tanggal { get; init; }

    public string Keterangan { get; init; } = string.Empty;

    public decimal Debet { get; init; }

    public decimal Kredit { get; init; }

    public bool IsOpeningBalance => string.Equals(Keterangan, "SALDO AWAL", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Maps a validated data row to the General Ledger contract.
    /// </summary>
    public static GeneralLedgerRow FromDataRow(DataRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        EnsureRequiredColumns(row.Table);

        return new GeneralLedgerRow
        {
            Baris = GetInt32(row, "BARIS"),
            Param = GetInt32(row, "PARAM"),
            Periode = GetString(row, "PERIODE"),
            Kode = GetString(row, "KODE"),
            Rekening = GetString(row, "REKENING"),
            NoJurnal = GetString(row, "NOJURNAL"),
            Tanggal = GetNullableDateTime(row, "TANGGAL"),
            Keterangan = GetString(row, "KETERANGAN"),
            Debet = GetDecimal(row, "DEBET"),
            Kredit = GetDecimal(row, "KREDIT")
        };
    }

    /// <summary>
    /// Returns and validates the BukuBesar table from a report dataset.
    /// </summary>
    public static DataTable GetRequiredTable(DataSet dataSet)
    {
        ArgumentNullException.ThrowIfNull(dataSet);

        DataTable? table = dataSet.Tables["BukuBesar"];
        if (table == null)
        {
            string receivedTables = dataSet.Tables.Count == 0
                ? "(tidak ada)"
                : string.Join(", ", dataSet.Tables.Cast<DataTable>().Select(item => item.TableName));

            throw new InvalidOperationException(
                "Data Buku Besar tidak tersedia. Tabel BukuBesar tidak ditemukan. " +
                $"Tabel diterima: {receivedTables}.");
        }

        EnsureRequiredColumns(table);
        return table;
    }

    /// <summary>
    /// Validates that a table exposes every field required by the report templates.
    /// </summary>
    public static void EnsureRequiredColumns(DataTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        List<string> missingColumns = RequiredColumns
            .Where(column => !table.Columns.Contains(column))
            .ToList();

        List<string> receivedColumns = table.Columns
            .Cast<DataColumn>()
            .Select(column => column.ColumnName)
            .ToList();

        bool hasExpectedOrder = receivedColumns.Count == RequiredColumns.Count
            && receivedColumns.SequenceEqual(RequiredColumns, StringComparer.OrdinalIgnoreCase);

        if (missingColumns.Count == 0 && hasExpectedOrder)
        {
            return;
        }

        string tableName = string.IsNullOrWhiteSpace(table.TableName) ? "(tanpa nama)" : table.TableName;
        string receivedColumnList = receivedColumns.Count == 0
            ? "(tidak ada)"
            : string.Join(", ", receivedColumns);

        string missingColumnList = missingColumns.Count == 0
            ? "(tidak ada; urutan atau jumlah kolom tidak sesuai)"
            : string.Join(", ", missingColumns);

        throw new InvalidOperationException(
            $"Data Buku Besar tidak lengkap untuk tabel {tableName}. " +
            $"Kolom hilang: {missingColumnList}. " +
            $"Urutan wajib: {string.Join(", ", RequiredColumns)}. " +
            $"Kolom diterima: {receivedColumnList}.");
    }

    private static string GetString(DataRow row, string column)
    {
        return row[column] == DBNull.Value ? string.Empty : row[column].ToString() ?? string.Empty;
    }

    private static int GetInt32(DataRow row, string column)
    {
        return row[column] == DBNull.Value ? 0 : Convert.ToInt32(row[column]);
    }

    private static decimal GetDecimal(DataRow row, string column)
    {
        return row[column] == DBNull.Value ? 0m : Convert.ToDecimal(row[column]);
    }

    private static DateTime? GetNullableDateTime(DataRow row, string column)
    {
        return row[column] == DBNull.Value ? null : Convert.ToDateTime(row[column]);
    }
}
