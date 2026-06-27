using System;
using System.Collections.Generic;
using System.Data;

namespace Accounting.Model
{
    public sealed class NeracaRow
    {
        public static readonly IReadOnlyList<string> RequiredColumns =
        [
            "KODE", "CAT1", "KAT", "CAT2", "AKUN", "TIPE", "POSISI", "BULANINI", "BULANLALU", "AWALTAHUN"
        ];

        public string Kode { get; set; } = string.Empty;
        public string Cat1 { get; set; } = string.Empty;
        public string Kat { get; set; } = string.Empty;
        public string Cat2 { get; set; } = string.Empty;
        public string Akun { get; set; } = string.Empty;
        public string ParentAkun { get; set; } = string.Empty;
        public string Tipe { get; set; } = string.Empty;
        public string Posisi { get; set; } = string.Empty;
        public decimal BulanIni { get; set; }
        public decimal BulanLalu { get; set; }
        public decimal AwalTahun { get; set; }

        public static NeracaRow FromDataRow(DataRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            LabaRugiRow.EnsureRequiredColumns(row.Table, RequiredColumns, "Neraca");

            return new NeracaRow
            {
                Kode = GetString(row, "KODE"),
                Cat1 = GetString(row, "CAT1"),
                Kat = GetString(row, "KAT"),
                Cat2 = GetString(row, "CAT2"),
                Akun = GetString(row, "AKUN"),
                ParentAkun = row.Table.Columns.Contains("PARENTAKUN") ? GetString(row, "PARENTAKUN") : string.Empty,
                Tipe = GetString(row, "TIPE"),
                Posisi = GetString(row, "POSISI"),
                BulanIni = GetDecimal(row, "BULANINI"),
                BulanLalu = GetDecimal(row, "BULANLALU"),
                AwalTahun = GetDecimal(row, "AWALTAHUN")
            };
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
