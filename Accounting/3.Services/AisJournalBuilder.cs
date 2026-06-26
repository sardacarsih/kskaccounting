using Accounting._1.Interface;
using System;
using System.Collections.Generic;
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
        public static List<AIS_JURNAL_FINAL> BuildForDivision(
            IEnumerable<AIS_JURNAL> aisRows,
            IEnumerable<JurnalKomponen> componentRows,
            Division division,
            AisJournalBuildContext context)
        {
            if (division == null)
            {
                return [];
            }

            string divisiId = division.DIVISIID ?? string.Empty;
            string divisiName = division.DIVISI ?? string.Empty;
            string noJurnal = division.NOMOR ?? string.Empty;
            string keteranganSuffix = $"{divisiName} {context.Estate} R{context.Remise} {context.MonthName} {context.Year}";

            List<AIS_JURNAL> journalRows = division.ISBORONGAN == 1
                ? BuildBoronganRows(aisRows, componentRows, divisiId, keteranganSuffix, context.PeriodString)
                : BuildHarianRows(aisRows, componentRows, divisiId, keteranganSuffix, context.PeriodString);

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
            bool borongan)
        {
            return divisions
                .Where(division => borongan ? division.ISBORONGAN == 1 : division.ISBORONGAN != 1)
                .SelectMany(division => BuildForDivision(aisRows, componentRows, division, context))
                .ToList();
        }

        private static List<AIS_JURNAL> BuildBoronganRows(
            IEnumerable<AIS_JURNAL> aisRows,
            IEnumerable<JurnalKomponen> componentRows,
            string divisiId,
            string keteranganSuffix,
            string periodString)
        {
            List<AIS_JURNAL> journalRows = aisRows
                .Where(f => f.DIVISI == divisiId && f.ISBORONGAN == 1 && f.JENIS == "PANEN")
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

                decimal debetWithPph21 = JurnalAmountRounding.RoundJournalAmount(item.DEBETPPH / 0.975m);
                if (item.DEBET < debetWithPph21)
                {
                    decimal pph21 = JurnalAmountRounding.RoundJournalAmount(debetWithPph21 * 0.025m);
                    item.DEBET = JurnalAmountRounding.RoundJournalAmount(item.DEBET + pph21);
                    item.PPH21 = JurnalAmountRounding.RoundJournalAmount(item.PPH21 + pph21);
                }
            }

            List<AIS_JURNAL> componentJournalRows = componentRows
                .Where(x => x.Divisi == divisiId && x.IsBorongan)
                .Select(x => new AIS_JURNAL
                {
                    DEBET = x.Sisi == "D" ? JurnalAmountRounding.RoundJournalAmount(x.Jumlah) : 0,
                    KREDIT = x.Sisi == "K" ? JurnalAmountRounding.RoundJournalAmount(x.Jumlah) : 0,
                    KETERANGAN = $"{x.Keterangan ?? string.Empty} {keteranganSuffix}",
                    POSTED = true,
                    PERIODE = periodString
                })
                .ToList();

            journalRows.AddRange(componentJournalRows);

            decimal totalDebet = JurnalAmountRounding.RoundJournalAmount(journalRows.Sum(f => f.DEBET));
            decimal totalPph21 = JurnalAmountRounding.RoundJournalAmount(journalRows.Sum(f => f.PPH21));
            decimal totalKredit = JurnalAmountRounding.RoundJournalAmount(journalRows.Sum(f => f.KREDIT));
            decimal operasional = JurnalAmountRounding.RoundJournalAmount(totalDebet - (totalPph21 + totalKredit));

            journalRows.AddRange(
            [
                new() { KODE = "31.00001.001", REKENING = "Hutang PPh Pasal 21", KREDIT = totalPph21, KETERANGAN = "TOTAL PPH PASAL 21 " + keteranganSuffix, POSTED = true, PERIODE = periodString },
                new() { KODE = "33.00001.001", REKENING = "Gaji dan Upah YMH dibayar", KREDIT = operasional, KETERANGAN = "TOTAL BORONGAN PANEN " + keteranganSuffix, POSTED = true, PERIODE = periodString }
            ]);

            return journalRows;
        }

        private static List<AIS_JURNAL> BuildHarianRows(
            IEnumerable<AIS_JURNAL> aisRows,
            IEnumerable<JurnalKomponen> componentRows,
            string divisiId,
            string keteranganSuffix,
            string periodString)
        {
            List<AIS_JURNAL> journalRows = aisRows
                .Where(f => f.DIVISI == divisiId && !(f.ISBORONGAN == 1 && f.JENIS == "PANEN"))
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

            List<AIS_JURNAL> componentJournalRows = componentRows
                .Where(x => x.Divisi == divisiId && !x.IsBorongan)
                .Select(x => new AIS_JURNAL
                {
                    DEBET = x.Sisi == "D" ? JurnalAmountRounding.RoundJournalAmount(x.Jumlah) : 0,
                    KREDIT = x.Sisi == "K" ? JurnalAmountRounding.RoundJournalAmount(x.Jumlah) : 0,
                    KETERANGAN = $"{x.Keterangan ?? string.Empty} {keteranganSuffix}",
                    POSTED = true,
                    PERIODE = periodString
                })
                .ToList();

            journalRows.AddRange(componentJournalRows);

            decimal biayaOperasionalBruto = JurnalAmountRounding.RoundJournalAmount(journalRows.Sum(f => f.DEBET));
            decimal biayaOperasionalNetto = JurnalAmountRounding.RoundJournalAmount(biayaOperasionalBruto - journalRows.Sum(f => f.KREDIT));

            journalRows.Add(new AIS_JURNAL
            {
                KODE = "33.00001.001",
                REKENING = "Gaji dan Upah YMH dibayar",
                KREDIT = biayaOperasionalNetto,
                KETERANGAN = "TOTAL BIAYA OPERASIONAL " + keteranganSuffix,
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
