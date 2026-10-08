using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Accounting.BusinessLayer
{
    public sealed class JurnalImportSelectionService
    {
        internal static DataTable FilterDetailsByJournalNumber(DataTable source, string journalNumber)
        {
            ArgumentNullException.ThrowIfNull(source);

            DataTable result = source.Clone();
            if (source.Rows.Count == 0 ||
                !source.Columns.Contains("NOJURNAL") ||
                string.IsNullOrWhiteSpace(journalNumber))
            {
                return result;
            }

            foreach (DataRow row in source.Rows)
            {
                string rowJournalNumber = Convert.ToString(row["NOJURNAL"]) ?? string.Empty;
                if (string.Equals(rowJournalNumber, journalNumber, StringComparison.OrdinalIgnoreCase))
                {
                    result.ImportRow(row);
                }
            }

            return result;
        }

        public List<JurnalDetailAdd> BuildInputDetails(DataTable source, IEnumerable<string> selectedNomor)
        {
            if (source == null || source.Rows.Count == 0)
            {
                return [];
            }

            HashSet<string> nomorFilter = selectedNomor
                .Where(nomor => !string.IsNullOrWhiteSpace(nomor))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (nomorFilter.Count == 0)
            {
                return [];
            }

            return source.AsEnumerable()
                .Where(row => nomorFilter.Contains(Convert.ToString(row["NOJURNAL"]) ?? string.Empty))
                .Select(MapToInputDetail)
                .ToList();
        }

        private static JurnalDetailAdd MapToInputDetail(DataRow row)
        {
            return new JurnalDetailAdd
            {
                BARIS = GetInt(row, "BARIS"),
                Kode = Convert.ToString(row["KODE"]) ?? string.Empty,
                Rekening = Convert.ToString(row["REKENING"]) ?? string.Empty,
                Debet = GetDecimal(row, "DEBET"),
                Kredit = GetDecimal(row, "KREDIT"),
                Keterangan = Convert.ToString(row["KETERANGAN"]) ?? string.Empty
            };
        }

        private static int GetInt(DataRow row, string columnName)
        {
            object value = row[columnName];
            return value == DBNull.Value ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        private static decimal GetDecimal(DataRow row, string columnName)
        {
            object value = row[columnName];
            return value == DBNull.Value ? 0m : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
    }
}
