using Accounting._1.Interface;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.BusinessLayer
{
    public sealed class AisJournalBuildContext
    {
        public required string Estate { get; init; }
        public required int Remise { get; init; }
        public required string MonthName { get; init; }
        public required int Year { get; init; }
        public required string PeriodString { get; init; }
        public required DateTime JournalDate { get; init; }
    }

    public static class AisJournalBuilder
    {
        // Labels of the balancing credit line. They are also the "already journaled" marker
        // searched in ACCT_JURNAL_DTL, so keep them stable (existing journals use them).
        private const string TotalBoronganLabel = "TOTAL BORONGAN PANEN ";
        private const string TotalHarianLabel = "TOTAL BIAYA OPERASIONAL ";

        public const string KomponenAccountKeyPrefix = "AIS_KOMPONEN_";

        // BPJS deductions reuse the HRIS payroll keys when no AIS-specific key is configured.
        private static readonly Dictionary<string, string> KomponenFallbackKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            ["BPJS KESEHATAN"] = "HRIS_POT_BPJS",
            ["BPJS JHT"] = "HRIS_POT_JHT",
            ["BPJS PENSIUN"] = "HRIS_POT_PENSIUN"
        };

        /// <summary>
        /// ACCT_DEFAULT.NAMA that maps a FIN_JURNAL_KOMPONEN.KETERANGAN to an account, e.g. "BPJS JHT" → "AIS_KOMPONEN_BPJS_JHT".
        /// </summary>
        public static string KomponenAccountKey(string? keterangan)
        {
            string normalized = new((keterangan ?? string.Empty).Trim().ToUpperInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '_')
                .ToArray());
            return KomponenAccountKeyPrefix + normalized;
        }

        /// <summary>
        /// Detail KETERANGAN of the balancing credit line that identifies the journal built for this division, remise and period.
        /// </summary>
        public static string GetJournaledMarker(Division division, AisJournalBuildContext context)
        {
            string label = division.ISBORONGAN == 1 ? TotalBoronganLabel : TotalHarianLabel;
            return label + BuildKeteranganSuffix(division, context);
        }

        private static string BuildKeteranganSuffix(Division division, AisJournalBuildContext context)
        {
            return $"{division.DIVISI ?? string.Empty} {context.Estate} R{context.Remise} {context.MonthName} {context.Year}";
        }

        public static Dictionary<string, (string Kode, string Nama)> LoadDefaultAccounts(int year)
        {
            var defaults = new Dictionary<string, (string Kode, string Nama)>(StringComparer.OrdinalIgnoreCase);
            try
            {
                using var dbConnection = new OracleConnection(LoginInfo.OracleConnString);
                dbConnection.Open();
                string query = @"
                    SELECT d.NAMA, d.KODEACC, c.NAMAACC 
                    FROM ACCT_DEFAULT d
                    LEFT JOIN ACCT_COA c ON c.KODEACC = d.KODEACC AND c.IDDATA = d.IDDATA AND c.TAHUN = :tahun
                    WHERE d.IDDATA = :idData";
                var list = dbConnection.Query(query, new { idData = Accounting.CompanyInfo.IDDATA, tahun = year });
                foreach (var item in list)
                {
                    string name = item.NAMA;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        defaults[name] = ((string)item.KODEACC, (string)(item.NAMAACC ?? item.KODEACC));
                    }
                }
            }
            catch (Exception ex)
            {
                // Hardcoded fallback accounts still apply, but leave a trace so a broken ACCT_DEFAULT is visible.
                Serilog.Log.Warning(ex, "AIS: gagal membaca ACCT_DEFAULT untuk IDDATA {IdData}, tahun {Tahun}", Accounting.CompanyInfo.IDDATA, year);
            }
            return defaults;
        }

        /// <param name="defaultAccounts">ACCT_DEFAULT map from <see cref="LoadDefaultAccounts"/>; null loads it from the database.</param>
        public static List<AIS_JURNAL_FINAL> BuildForDivision(
            IEnumerable<AIS_JURNAL> aisRows,
            IEnumerable<JurnalKomponen> componentRows,
            Division division,
            AisJournalBuildContext context,
            IReadOnlyDictionary<string, (string Kode, string Nama)>? defaultAccounts = null)
        {
            if (division == null)
            {
                return [];
            }

            string divisiId = division.DIVISIID ?? string.Empty;
            string noJurnal = division.NOMOR ?? string.Empty;
            string keteranganSuffix = BuildKeteranganSuffix(division, context);

            var defaults = defaultAccounts ?? LoadDefaultAccounts(context.Year);
            (string Kode, string Nama) GetAccount(string key, string defaultKode, string defaultNama)
            {
                if (defaults.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val.Kode))
                {
                    return val;
                }
                return (defaultKode, defaultNama);
            }

            (string? Kode, string? Nama) GetKomponenAccount(string? keterangan)
            {
                if (defaults.TryGetValue(KomponenAccountKey(keterangan), out var val) && !string.IsNullOrWhiteSpace(val.Kode))
                {
                    return val;
                }

                if (KomponenFallbackKeys.TryGetValue((keterangan ?? string.Empty).Trim(), out string? fallbackKey)
                    && defaults.TryGetValue(fallbackKey, out val) && !string.IsNullOrWhiteSpace(val.Kode))
                {
                    return val;
                }

                // Left blank on purpose: the user must pick the account (save rejects a blank Kode).
                return (null, null);
            }

            List<AIS_JURNAL> journalRows = division.ISBORONGAN == 1
                ? BuildBoronganRows(aisRows, componentRows, divisiId, keteranganSuffix, context.PeriodString, GetAccount, GetKomponenAccount)
                : BuildHarianRows(aisRows, componentRows, divisiId, keteranganSuffix, context.PeriodString, GetAccount, GetKomponenAccount);

            int rowNumber = 1;
            foreach (AIS_JURNAL item in journalRows)
            {
                item.NO = rowNumber++;
            }

            return JurnalAmountRounding.NormalizeAisFinalRows(journalRows.Select(j => new AIS_JURNAL_FINAL
            {
                NOJURNAL = noJurnal,
                TANGGAL = context.JournalDate,
                NO = j.NO,
                KODE = j.KODE,
                REKENING = j.REKENING,
                DEBET = j.DEBET,
                KREDIT = j.KREDIT,
                KETERANGAN = j.KETERANGAN,
                POSTED = j.POSTED,
                PERIODE = j.PERIODE
            }));
        }

        public static List<AIS_JURNAL_FINAL> BuildForDivisions(
            IEnumerable<AIS_JURNAL> aisRows,
            IEnumerable<JurnalKomponen> componentRows,
            IEnumerable<Division> divisions,
            AisJournalBuildContext context,
            bool borongan,
            IReadOnlyDictionary<string, (string Kode, string Nama)>? defaultAccounts = null)
        {
            List<Division> selected = divisions
                .Where(division => borongan ? division.ISBORONGAN == 1 : division.ISBORONGAN != 1)
                .ToList();
            if (selected.Count == 0)
            {
                return [];
            }

            // One ACCT_DEFAULT read for the whole export instead of one per division.
            var defaults = defaultAccounts ?? LoadDefaultAccounts(context.Year);
            return selected
                .SelectMany(division => BuildForDivision(aisRows, componentRows, division, context, defaults))
                .ToList();
        }

        private static List<AIS_JURNAL> BuildKomponenRows(
            IEnumerable<JurnalKomponen> componentRows,
            string divisiId,
            bool borongan,
            string keteranganSuffix,
            string periodString,
            Func<string?, (string? Kode, string? Nama)> getKomponenAccount)
        {
            return componentRows
                .Where(x => x.Divisi == divisiId && x.IsBorongan == borongan)
                .Select(x =>
                {
                    var account = getKomponenAccount(x.Keterangan);
                    return new AIS_JURNAL
                    {
                        KODE = account.Kode,
                        REKENING = account.Nama,
                        DEBET = x.Sisi == "D" ? JurnalAmountRounding.RoundJournalAmount(x.Jumlah) : 0,
                        KREDIT = x.Sisi == "K" ? JurnalAmountRounding.RoundJournalAmount(x.Jumlah) : 0,
                        KETERANGAN = $"{x.Keterangan ?? string.Empty} {keteranganSuffix}",
                        POSTED = true,
                        PERIODE = periodString
                    };
                })
                .ToList();
        }

        private static List<AIS_JURNAL> BuildBoronganRows(
            IEnumerable<AIS_JURNAL> aisRows,
            IEnumerable<JurnalKomponen> componentRows,
            string divisiId,
            string keteranganSuffix,
            string periodString,
            Func<string, string, string, (string Kode, string Nama)> getAccount,
            Func<string?, (string? Kode, string? Nama)> getKomponenAccount)
        {
            // Every borongan BKM row (panen, rawat, umum) belongs to the borongan journal; only panen gets the PPh21 gross-up.
            List<AIS_JURNAL> journalRows = aisRows
                .Where(f => f.DIVISI == divisiId && f.ISBORONGAN == 1)
                .OrderBy(f => f.JENIS)
                .ThenBy(f => f.PEKERJAAN)
                .ThenBy(f => f.BLOK)
                .Select(CloneAisJournal)
                .ToList();

            foreach (AIS_JURNAL item in journalRows)
            {
                // Round harvest rows up front so totals (and the balancing credits derived from
                // them) agree with the per-row rounding applied later in NormalizeAisFinalRows.
                item.DEBET = JurnalAmountRounding.RoundJournalAmount(item.DEBET);
                item.KREDIT = JurnalAmountRounding.RoundJournalAmount(item.KREDIT);
                item.PPH21 = JurnalAmountRounding.RoundJournalAmount(item.PPH21);

                if (item.JENIS != "PANEN")
                {
                    continue;
                }

                decimal debetWithPph21 = JurnalAmountRounding.RoundJournalAmount(item.DEBETPPH / 0.975m);
                if (item.DEBET < debetWithPph21)
                {
                    decimal pph21 = JurnalAmountRounding.RoundJournalAmount(debetWithPph21 * 0.025m);
                    item.DEBET = JurnalAmountRounding.RoundJournalAmount(item.DEBET + pph21);
                    item.PPH21 = JurnalAmountRounding.RoundJournalAmount(item.PPH21 + pph21);
                }
            }

            journalRows.AddRange(BuildKomponenRows(componentRows, divisiId, true, keteranganSuffix, periodString, getKomponenAccount));

            decimal totalDebet = JurnalAmountRounding.RoundJournalAmount(journalRows.Sum(f => f.DEBET));
            decimal totalPph21 = JurnalAmountRounding.RoundJournalAmount(journalRows.Sum(f => f.PPH21));
            decimal totalKredit = JurnalAmountRounding.RoundJournalAmount(journalRows.Sum(f => f.KREDIT));
            decimal operasional = JurnalAmountRounding.RoundJournalAmount(totalDebet - (totalPph21 + totalKredit));

            var pph21Acc = getAccount("HUTANG_PPH21", "31.00001.001", "Hutang PPh Pasal 21");
            var gajiYmhdAcc = getAccount("GAJI_DAN_UPAH_YMH_DIBAYAR", "33.00001.001", "Gaji dan Upah YMH dibayar");

            journalRows.AddRange(
            [
                new() { KODE = pph21Acc.Kode, REKENING = pph21Acc.Nama, KREDIT = totalPph21, KETERANGAN = "TOTAL PPH PASAL 21 " + keteranganSuffix, POSTED = true, PERIODE = periodString },
                new() { KODE = gajiYmhdAcc.Kode, REKENING = gajiYmhdAcc.Nama, KREDIT = operasional, KETERANGAN = TotalBoronganLabel + keteranganSuffix, POSTED = true, PERIODE = periodString }
            ]);

            return journalRows;
        }

        private static List<AIS_JURNAL> BuildHarianRows(
            IEnumerable<AIS_JURNAL> aisRows,
            IEnumerable<JurnalKomponen> componentRows,
            string divisiId,
            string keteranganSuffix,
            string periodString,
            Func<string, string, string, (string Kode, string Nama)> getAccount,
            Func<string?, (string? Kode, string? Nama)> getKomponenAccount)
        {
            List<AIS_JURNAL> journalRows = aisRows
                .Where(f => f.DIVISI == divisiId && f.ISBORONGAN != 1)
                .OrderBy(f => f.JENIS)
                .ThenBy(f => f.PEKERJAAN)
                .ThenBy(f => f.BLOK)
                .Select(CloneAisJournal)
                .ToList();

            // Round harvest rows to 2 decimals up front so the balancing "operasional" credit
            // (computed from these sums) agrees with the per-row rounding applied later in
            // NormalizeAisFinalRows. Without this the journal can be off by 0.01 when inputs
            // carry more than 2 decimals.
            foreach (AIS_JURNAL item in journalRows)
            {
                item.DEBET = JurnalAmountRounding.RoundJournalAmount(item.DEBET);
                item.KREDIT = JurnalAmountRounding.RoundJournalAmount(item.KREDIT);
            }

            journalRows.AddRange(BuildKomponenRows(componentRows, divisiId, false, keteranganSuffix, periodString, getKomponenAccount));

            decimal biayaOperasionalBruto = JurnalAmountRounding.RoundJournalAmount(journalRows.Sum(f => f.DEBET));
            decimal biayaOperasionalNetto = JurnalAmountRounding.RoundJournalAmount(biayaOperasionalBruto - journalRows.Sum(f => f.KREDIT));

            var gajiYmhdAcc = getAccount("GAJI_DAN_UPAH_YMH_DIBAYAR", "33.00001.001", "Gaji dan Upah YMH dibayar");

            journalRows.Add(new AIS_JURNAL
            {
                KODE = gajiYmhdAcc.Kode,
                REKENING = gajiYmhdAcc.Nama,
                KREDIT = biayaOperasionalNetto,
                KETERANGAN = TotalHarianLabel + keteranganSuffix,
                POSTED = true,
                PERIODE = periodString
            });

            return journalRows;
        }

        private static AIS_JURNAL CloneAisJournal(AIS_JURNAL source)
        {
            return new AIS_JURNAL
            {
                ISBORONGAN = source.ISBORONGAN,
                DIVISI = source.DIVISI,
                JENIS = source.JENIS,
                PEKERJAAN = source.PEKERJAAN,
                BLOK = source.BLOK,
                NOJURNAL = source.NOJURNAL,
                TANGGAL = source.TANGGAL,
                NO = source.NO,
                KODE = source.KODE,
                REKENING = source.REKENING,
                DEBET = source.DEBET,
                PPH21 = source.PPH21,
                DEBETPPH = source.DEBETPPH,
                KREDIT = source.KREDIT,
                KETERANGAN = source.KETERANGAN,
                POSTED = source.POSTED,
                PERIODE = source.PERIODE
            };
        }
    }
}
