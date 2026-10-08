using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.Model
{
    public sealed class ReportDrillDownRow
    {
        public static readonly IReadOnlyList<string> RequiredColumns =
        [
            "KODEACC", "NAMAACC", "PARENTACC", "POSISI", "ISHEADER", "NILAI", "REPORT_CODE"
        ];

        public string KodeAcc { get; init; } = string.Empty;
        public string NamaAcc { get; init; } = string.Empty;
        public string ParentAcc { get; init; } = string.Empty;
        public string Posisi { get; init; } = string.Empty;
        public string IsHeader { get; init; } = string.Empty;
        public decimal Nilai { get; init; }
        public string ReportCode { get; init; } = string.Empty;

        public static ReportDrillDownRow FromDataRow(DataRow row)
        {
            ArgumentNullException.ThrowIfNull(row);
            EnsureRequiredColumns(row.Table);

            return new ReportDrillDownRow
            {
                KodeAcc = GetString(row, "KODEACC"),
                NamaAcc = GetString(row, "NAMAACC"),
                ParentAcc = GetString(row, "PARENTACC"),
                Posisi = GetString(row, "POSISI"),
                IsHeader = GetString(row, "ISHEADER"),
                Nilai = GetDecimal(row, "NILAI"),
                ReportCode = GetString(row, "REPORT_CODE")
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
                throw new InvalidOperationException($"Data drilldown laporan tidak lengkap. Kolom hilang: {string.Join(", ", missingColumns)}.");
            }
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
