using System;
using System.Collections.Generic;
using System.Data;

namespace Accounting.Model
{
    public sealed class NeracaSaldoRow
    {
        public static readonly IReadOnlyList<string> RequiredColumns =
        [
            "KODEACC", "NAMAACC", "PARENTACC", "LVL", "ISHEADER", "ISAKTIF", "SALDOAWAL",
            "JAN", "FEB", "MAR", "APR", "MEI", "JUN", "JUL", "AGU", "SEP", "OKT", "NOV", "DES"
        ];

        public string KodeAkun { get; init; } = string.Empty;
        public string NamaAkun { get; init; } = string.Empty;
        public string ParentAkun { get; init; } = string.Empty;
        public int Level { get; init; }
        public bool IsHeader { get; init; }
        public bool IsActive { get; init; }
        public decimal SaldoAwal { get; init; }
        public decimal Januari { get; init; }
        public decimal Februari { get; init; }
        public decimal Maret { get; init; }
        public decimal April { get; init; }
        public decimal Mei { get; init; }
        public decimal Juni { get; init; }
        public decimal Juli { get; init; }
        public decimal Agustus { get; init; }
        public decimal September { get; init; }
        public decimal Oktober { get; init; }
        public decimal November { get; init; }
        public decimal Desember { get; init; }

        public bool HasAnyBalance =>
            SaldoAwal != 0m ||
            Januari != 0m ||
            Februari != 0m ||
            Maret != 0m ||
            April != 0m ||
            Mei != 0m ||
            Juni != 0m ||
            Juli != 0m ||
            Agustus != 0m ||
            September != 0m ||
            Oktober != 0m ||
            November != 0m ||
            Desember != 0m;

        public static NeracaSaldoRow FromDataRecord(IDataRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);
            EnsureRequiredColumns(record);

            return new NeracaSaldoRow
            {
                KodeAkun = GetString(record, "KODEACC"),
                NamaAkun = GetString(record, "NAMAACC"),
                ParentAkun = GetString(record, "PARENTACC"),
                Level = GetInt32(record, "LVL"),
                IsHeader = string.Equals(GetString(record, "ISHEADER"), "G", StringComparison.OrdinalIgnoreCase),
                IsActive = !string.Equals(GetString(record, "ISAKTIF"), "N", StringComparison.OrdinalIgnoreCase),
                SaldoAwal = GetDecimal(record, "SALDOAWAL"),
                Januari = GetDecimal(record, "JAN"),
                Februari = GetDecimal(record, "FEB"),
                Maret = GetDecimal(record, "MAR"),
                April = GetDecimal(record, "APR"),
                Mei = GetDecimal(record, "MEI"),
                Juni = GetDecimal(record, "JUN"),
                Juli = GetDecimal(record, "JUL"),
                Agustus = GetDecimal(record, "AGU"),
                September = GetDecimal(record, "SEP"),
                Oktober = GetDecimal(record, "OKT"),
                November = GetDecimal(record, "NOV"),
                Desember = GetDecimal(record, "DES")
            };
        }

        private static void EnsureRequiredColumns(IDataRecord record)
        {
            List<string> missingColumns = [];
            foreach (string column in RequiredColumns)
            {
                try
                {
                    record.GetOrdinal(column);
                }
                catch (ArgumentException)
                {
                    missingColumns.Add(column);
                }
            }

            if (missingColumns.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Data Neraca Saldo tidak lengkap. Kolom hilang: {string.Join(", ", missingColumns)}.");
            }
        }

        private static string GetString(IDataRecord record, string column)
        {
            int ordinal = record.GetOrdinal(column);
            return record.IsDBNull(ordinal) ? string.Empty : Convert.ToString(record.GetValue(ordinal)) ?? string.Empty;
        }

        private static int GetInt32(IDataRecord record, string column)
        {
            int ordinal = record.GetOrdinal(column);
            return record.IsDBNull(ordinal) ? 0 : Convert.ToInt32(record.GetValue(ordinal));
        }

        private static decimal GetDecimal(IDataRecord record, string column)
        {
            int ordinal = record.GetOrdinal(column);
            return record.IsDBNull(ordinal) ? 0m : Convert.ToDecimal(record.GetValue(ordinal));
        }
    }
}
