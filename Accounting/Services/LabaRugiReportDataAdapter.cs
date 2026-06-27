using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.Services
{
    public static class LabaRugiReportDataAdapter
    {
        // Section code -> result line emitted immediately after that section's subtotal.
        private static readonly IReadOnlyDictionary<string, string> ResultLineAfterSection =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["HPP"] = "LABA KOTOR",
                ["BIAYA_UMUM_ADMIN"] = "LABA USAHA",
                ["BIAYA_BUNGA"] = "LABA SEBELUM PAJAK",
                ["PPH_BADAN"] = "LABA SETELAH PAJAK"
            };

        private const string FinalResultLine = "LABA BERSIH";

        public static DataSet CreateReportDataSet(IEnumerable<LabaRugiRow> rows)
        {
            DataSet dataSet = new();
            dataSet.Tables.Add(CreateReportDataTable(rows));
            return dataSet;
        }

        public static DataTable CreateReportDataTable(IEnumerable<LabaRugiRow> rows)
        {
            DataTable table = new("LabaRugi");
            table.Columns.Add("IDDATA", typeof(string));
            table.Columns.Add("KODEACC", typeof(string));
            table.Columns.Add("URUT", typeof(int));
            table.Columns.Add("TIPEACC", typeof(string));
            table.Columns.Add("SUB1", typeof(string));
            table.Columns.Add("SUB2", typeof(string));
            table.Columns.Add("SUB3", typeof(string));
            table.Columns.Add("SUB4", typeof(string));
            table.Columns.Add("SUB5", typeof(string));
            table.Columns.Add("SUB6", typeof(string));
            table.Columns.Add("BULANINI", typeof(decimal));
            table.Columns.Add("TAHUNINI", typeof(decimal));
            table.Columns.Add("JENIS", typeof(string));
            table.Columns.Add("SETSUB", typeof(string));
            table.Columns.Add("USERGEN", typeof(string));
            table.Columns.Add("ISHEADER", typeof(string));
            table.Columns.Add("POSISI", typeof(string));
            table.Columns.Add("ROWKIND", typeof(string));

            foreach (LabaRugiRow row in BuildReportRows(rows))
            {
                table.Rows.Add(
                    row.IdData,
                    row.KodeAcc,
                    row.Urut,
                    row.TipeAcc,
                    row.Sub1,
                    row.Sub2,
                    row.Sub3,
                    row.Sub4,
                    row.Sub5,
                    row.Sub6,
                    row.BulanIni,
                    row.TahunIni,
                    row.Jenis,
                    row.SetSub,
                    row.UserGen,
                    row.IsHeader,
                    row.Posisi,
                    row.RowKind);
            }

            return table;
        }

        public static DataTable CreateExportDataTable(IEnumerable<LabaRugiRow> rows)
        {
            DataTable table = new();
            table.Columns.Add("KETERANGAN", typeof(string));
            table.Columns.Add("BULANINI", typeof(decimal));
            table.Columns.Add("TAHUNINI", typeof(decimal));

            foreach (LabaRugiRow row in BuildReportRows(rows))
            {
                table.Rows.Add(row.Sub1, row.BulanIni, row.TahunIni);
            }

            return table;
        }

        // Full report pipeline: source rows -> per-section subtotals -> cross-section result lines.
        public static List<LabaRugiRow> BuildReportRows(IEnumerable<LabaRugiRow> rows)
        {
            return AddComputedTotals(AddSectionSubtotals(rows));
        }

        public static List<LabaRugiRow> AddSectionSubtotals(IEnumerable<LabaRugiRow> rows)
        {
            if (rows == null)
            {
                throw new ArgumentNullException(nameof(rows));
            }

            List<LabaRugiRow> sourceRows = rows.ToList();
            List<LabaRugiRow> reportRows = [];
            List<LabaRugiRow> sectionRows = [];
            string currentSectionKey = string.Empty;

            foreach (LabaRugiRow row in sourceRows)
            {
                string sectionKey = GetSectionKey(row);
                if (sectionRows.Count > 0 && sectionKey != currentSectionKey)
                {
                    reportRows.Add(CreateSubtotalRow(sectionRows));
                    sectionRows.Clear();
                }

                reportRows.Add(row);
                sectionRows.Add(row);
                currentSectionKey = sectionKey;
            }

            if (sectionRows.Count > 0)
            {
                reportRows.Add(CreateSubtotalRow(sectionRows));
            }

            return reportRows;
        }

        // Inserts cross-section result lines (Laba Kotor, Laba Usaha, ... Laba Bersih) after the
        // relevant section subtotals. Running net = sum of credit-section subtotals minus
        // debit-section subtotals (section subtotals are already sign-normalized by the engine).
        public static List<LabaRugiRow> AddComputedTotals(IEnumerable<LabaRugiRow> rowsWithSubtotals)
        {
            if (rowsWithSubtotals == null)
            {
                throw new ArgumentNullException(nameof(rowsWithSubtotals));
            }

            List<LabaRugiRow> input = rowsWithSubtotals.ToList();
            List<LabaRugiRow> output = [];

            decimal runningBulan = 0m;
            decimal runningTahun = 0m;
            LabaRugiRow lastSubtotal = null;

            foreach (LabaRugiRow row in input)
            {
                output.Add(row);

                if (!row.IsSubtotal)
                {
                    continue;
                }

                int sign = string.Equals(row.Posisi, "K", StringComparison.OrdinalIgnoreCase) ? 1 : -1;
                runningBulan += sign * row.BulanIni;
                runningTahun += sign * row.TahunIni;
                lastSubtotal = row;

                if (ResultLineAfterSection.TryGetValue(row.SetSub ?? string.Empty, out string resultLabel))
                {
                    output.Add(CreateTotalRow(resultLabel, runningBulan, runningTahun, row));
                }
            }

            if (lastSubtotal != null)
            {
                output.Add(CreateTotalRow(FinalResultLine, runningBulan, runningTahun, lastSubtotal));
            }

            return output;
        }

        private static LabaRugiRow CreateSubtotalRow(IReadOnlyList<LabaRugiRow> sectionRows)
        {
            LabaRugiRow firstRow = sectionRows[0];
            LabaRugiRow lastRow = sectionRows[^1];
            string sectionName = string.IsNullOrWhiteSpace(firstRow.TipeAcc) ? firstRow.SetSub : firstRow.TipeAcc;

            return new LabaRugiRow
            {
                IdData = firstRow.IdData,
                KodeAcc = string.Empty,
                Urut = lastRow.Urut + 1,
                TipeAcc = firstRow.TipeAcc,
                Sub1 = $"Sub Total {sectionName}",
                BulanIni = sectionRows.Sum(row => row.BulanIni),
                TahunIni = sectionRows.Sum(row => row.TahunIni),
                Jenis = firstRow.Jenis,
                SetSub = firstRow.SetSub,
                UserGen = firstRow.UserGen,
                IsHeader = "G",
                Posisi = firstRow.Posisi,
                RowKind = LabaRugiRow.SubtotalRowKind
            };
        }

        // A result line shares the anchor section's group key (SETSUB/TIPEACC) so it renders inside
        // that group without triggering a new section header. KODEACC is empty so it is not drillable.
        private static LabaRugiRow CreateTotalRow(string label, decimal bulan, decimal tahun, LabaRugiRow anchor)
        {
            return new LabaRugiRow
            {
                IdData = anchor.IdData,
                KodeAcc = string.Empty,
                Urut = anchor.Urut + 1,
                TipeAcc = anchor.TipeAcc,
                Sub1 = label,
                BulanIni = bulan,
                TahunIni = tahun,
                Jenis = anchor.Jenis,
                SetSub = anchor.SetSub,
                UserGen = anchor.UserGen,
                IsHeader = "G",
                Posisi = anchor.Posisi,
                RowKind = LabaRugiRow.TotalRowKind
            };
        }

        private static string GetSectionKey(LabaRugiRow row)
        {
            if (!string.IsNullOrWhiteSpace(row.SetSub))
            {
                return row.SetSub;
            }

            return row.TipeAcc;
        }
    }
}
