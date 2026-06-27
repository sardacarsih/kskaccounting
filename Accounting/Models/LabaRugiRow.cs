using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.Model
{
    public sealed class LabaRugiRow
    {
        public const string DetailRowKind = "D";
        public const string SubtotalRowKind = "S";
        public const string TotalRowKind = "T";

        public static readonly IReadOnlyList<string> RequiredColumns =
        [
            "IDDATA", "KODEACC", "URUT", "TIPEACC", "SUB1", "SUB2", "SUB3", "SUB4",
            "SUB5", "SUB6", "BULANINI", "TAHUNINI", "JENIS", "SETSUB", "USERGEN", "ISHEADER", "POSISI"
        ];

        public string IdData { get; set; } = string.Empty;
        public string KodeAcc { get; set; } = string.Empty;
        public int Urut { get; set; }
        public string TipeAcc { get; set; } = string.Empty;
        public string Sub1 { get; set; } = string.Empty;
        public string Sub2 { get; set; } = string.Empty;
        public string Sub3 { get; set; } = string.Empty;
        public string Sub4 { get; set; } = string.Empty;
        public string Sub5 { get; set; } = string.Empty;
        public string Sub6 { get; set; } = string.Empty;
        public decimal BulanIni { get; set; }
        public decimal TahunIni { get; set; }
        public string Jenis { get; set; } = string.Empty;
        public string SetSub { get; set; } = string.Empty;
        public string UserGen { get; set; } = string.Empty;
        public string IsHeader { get; set; } = string.Empty;
        public string Posisi { get; set; } = string.Empty;
        public string RowKind { get; set; } = DetailRowKind;

        public bool IsSubtotal => RowKind == SubtotalRowKind;
        public bool IsTotal => RowKind == TotalRowKind;

        public static LabaRugiRow FromDataRow(DataRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            EnsureRequiredColumns(row.Table, RequiredColumns, "LabaRugi");

            return new LabaRugiRow
            {
                IdData = GetString(row, "IDDATA"),
                KodeAcc = GetString(row, "KODEACC"),
                Urut = GetInt32(row, "URUT"),
                TipeAcc = GetString(row, "TIPEACC"),
                Sub1 = GetString(row, "SUB1"),
                Sub2 = GetString(row, "SUB2"),
                Sub3 = GetString(row, "SUB3"),
                Sub4 = GetString(row, "SUB4"),
                Sub5 = GetString(row, "SUB5"),
                Sub6 = GetString(row, "SUB6"),
                BulanIni = GetDecimal(row, "BULANINI"),
                TahunIni = GetDecimal(row, "TAHUNINI"),
                Jenis = GetString(row, "JENIS"),
                SetSub = GetString(row, "SETSUB"),
                UserGen = GetString(row, "USERGEN"),
                IsHeader = GetString(row, "ISHEADER"),
                Posisi = GetString(row, "POSISI"),
                RowKind = row.Table.Columns.Contains("ROWKIND") ? GetString(row, "ROWKIND") : DetailRowKind
            };
        }

        public static void EnsureRequiredColumns(DataTable table, IReadOnlyCollection<string> requiredColumns, string tableName)
        {
            if (table == null)
            {
                throw new InvalidOperationException($"Data laporan tidak memiliki tabel {tableName}.");
            }

            List<string> missingColumns = requiredColumns
                .Where(column => !table.Columns.Contains(column))
                .ToList();

            if (missingColumns.Count > 0)
            {
                throw new InvalidOperationException($"Data laporan {tableName} tidak lengkap. Kolom hilang: {string.Join(", ", missingColumns)}.");
            }
        }

        private static string GetString(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? string.Empty : row[column].ToString();
        }

        private static int GetInt32(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? 0 : Convert.ToInt32(row[column]);
        }

        private static decimal GetDecimal(DataRow row, string column)
        {
            return row[column] == DBNull.Value ? 0m : Convert.ToDecimal(row[column]);
        }
    }
}


