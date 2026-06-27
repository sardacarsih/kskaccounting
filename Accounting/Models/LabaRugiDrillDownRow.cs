using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.Model
{
    public sealed class LabaRugiDrillDownRow
    {
        private static readonly IReadOnlyList<string> RequiredColumns =
        [
            "KODEACC", "HEADER", "POSISI", "KELOMPOK"
        ];

        public string KodeAcc { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public string Posisi { get; set; } = string.Empty;
        public string Kelompok { get; set; } = string.Empty;

        public static LabaRugiDrillDownRow FromDataRow(DataRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            EnsureRequiredColumns(row.Table);

            return new LabaRugiDrillDownRow
            {
                KodeAcc = GetString(row, "KODEACC"),
                Header = GetString(row, "HEADER"),
                Posisi = GetString(row, "POSISI"),
                Kelompok = GetString(row, "KELOMPOK")
            };
        }

        private static void EnsureRequiredColumns(DataTable table)
        {
            if (table == null)
            {
                throw new InvalidOperationException("Data laporan tidak memiliki tabel SubLabaRugi.");
            }

            List<string> missingColumns = RequiredColumns
                .Where(column => !table.Columns.Contains(column))
                .ToList();

            if (missingColumns.Count > 0)
            {
                throw new InvalidOperationException($"Data laporan SubLabaRugi tidak lengkap. Kolom hilang: {string.Join(", ", missingColumns)}.");
            }
        }

        private static string GetString(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? string.Empty : row[column].ToString();
        }
    }
}
