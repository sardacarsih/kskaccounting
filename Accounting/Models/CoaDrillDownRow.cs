using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.Model
{
    public sealed class CoaDrillDownRow
    {
        public static readonly IReadOnlyList<string> RequiredColumns =
        [
            "KODEACC", "NAMAACC", "PARENTACC", "POSISI", "ISHEADER", "DEBET", "KREDIT", "SALDOAKHIR"
        ];

        public string KodeAcc { get; init; } = string.Empty;
        public string NamaAcc { get; init; } = string.Empty;
        public string ParentAcc { get; init; } = string.Empty;
        public string Posisi { get; init; } = string.Empty;
        public string IsHeader { get; init; } = string.Empty;
        public decimal Debet { get; init; }
        public decimal Kredit { get; init; }
        public decimal SaldoAkhir { get; init; }

        public static CoaDrillDownRow FromDataRow(DataRow row)
        {
            ArgumentNullException.ThrowIfNull(row);
            EnsureRequiredColumns(row.Table);

            return new CoaDrillDownRow
            {
                KodeAcc = GetString(row, "KODEACC"),
                NamaAcc = GetString(row, "NAMAACC"),
                ParentAcc = GetString(row, "PARENTACC"),
                Posisi = GetString(row, "POSISI"),
                IsHeader = GetString(row, "ISHEADER"),
                Debet = GetDecimal(row, "DEBET"),
                Kredit = GetDecimal(row, "KREDIT"),
                SaldoAkhir = GetDecimal(row, "SALDOAKHIR")
            };
        }

        public static void EnsureRequiredColumns(DataTable table)
        {
            ArgumentNullException.ThrowIfNull(table);

            List<string> missingColumns = RequiredColumns
                .Where(column => !table.Columns.Contains(column))
                .ToList();

            if (missingColumns.Count > 0)
            {
                string tableName = string.IsNullOrWhiteSpace(table.TableName) ? "(tanpa nama)" : table.TableName;
                string receivedColumns = table.Columns.Count == 0
                    ? "(tidak ada)"
                    : string.Join(", ", table.Columns.Cast<DataColumn>().Select(column => column.ColumnName));

                throw new InvalidOperationException(
                    $"Data drilldown COA tidak lengkap untuk tabel {tableName}. " +
                    $"Kolom hilang: {string.Join(", ", missingColumns)}. " +
                    $"Kolom diterima: {receivedColumns}.");
            }
        }

        public static DataTable GetRequiredTable(DataSet dataSet)
        {
            ArgumentNullException.ThrowIfNull(dataSet);

            DataTable? table = dataSet.Tables["CoaDrillDown"];
            if (table == null)
            {
                string receivedTables = dataSet.Tables.Count == 0
                    ? "(tidak ada)"
                    : string.Join(", ", dataSet.Tables.Cast<DataTable>().Select(item => item.TableName));
                throw new InvalidOperationException(
                    "Data drilldown COA tidak tersedia. Tabel CoaDrillDown tidak ditemukan. " +
                    $"Tabel diterima: {receivedTables}.");
            }

            EnsureRequiredColumns(table);
            return table;
        }

        private static string GetString(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? string.Empty : row[column].ToString();
        }

        private static decimal GetDecimal(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? 0m : Convert.ToDecimal(row[column]);
        }
    }
}
