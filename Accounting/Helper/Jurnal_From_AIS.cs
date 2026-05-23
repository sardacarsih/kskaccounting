using Accounting._1.Interface;
using Accounting._2.DataAcces;
using Accounting.BusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Helper
{
    public class Jurnal_From_AIS
    {
    
        public  async Task<List<AIS_JURNAL_FINAL>> ProcessJurnalAsync(
            IEnumerable<AIS_JURNAL> aisJurnal,
            int isborongan,
            string div,
            string ket2,
            string periodestring,
            int periode,
            int remise,
            string iddata,
            DateTime TanggalJurnal,
            string noAIS_Bukti)
        {
            var jurnalimportService = new JurnalimportService();
            List<AIS_JURNAL> jurnalfiltered;

            if (isborongan == 1)
            {
                jurnalfiltered = aisJurnal
                    .Where(f => f.DIVISI == div && f.ISBORONGAN == isborongan && f.JENIS == "PANEN")
                    .OrderBy(f => f.JENIS)
                    .ThenBy(f => f.PEKERJAAN)
                    .ThenBy(f => f.BLOK)
                    .ToList();

                foreach (var item in jurnalfiltered)
                {
                    decimal debetWithPPH21 = JurnalAmountRounding.RoundJournalAmount(item.DEBETPPH / 0.975m);
                    if (item.DEBET < debetWithPPH21)
                    {
                        decimal pph21 = JurnalAmountRounding.RoundJournalAmount(debetWithPPH21 * 0.025m);
                        item.DEBET = JurnalAmountRounding.RoundJournalAmount(item.DEBET + pph21);
                        item.PPH21 = JurnalAmountRounding.RoundJournalAmount(item.PPH21 + pph21);
                    }
                }

                // Calculate the total DEBET after updates
                decimal totaldebet = JurnalAmountRounding.RoundJournalAmount(jurnalfiltered.Sum(f => f.DEBET));
                decimal totalpph21 = JurnalAmountRounding.RoundJournalAmount(jurnalfiltered.Sum(f => f.PPH21));

                // Calculate operational value
                decimal operasional = JurnalAmountRounding.RoundJournalAmount(totaldebet - totalpph21);

                // Add new rows for debetPercentage and operasional
                var newRows = new List<AIS_JURNAL>
                {
                    new() { KODE = "31.00001.001", REKENING = "Hutang PPh Pasal 21", KREDIT = totalpph21, KETERANGAN = "TOTAL PPH PASAL 21 " + ket2, POSTED = true, PERIODE = periodestring },
                    new() { KODE = "33.00001.001", REKENING = "Gaji dan Upah YMH dibayar", KREDIT = operasional, KETERANGAN = "TOTAL BORONGAN PANEN " + ket2, POSTED = true, PERIODE = periodestring }
                };

                // Add the new rows to the list
                jurnalfiltered.AddRange(newRows);
            }
            else
            {
                jurnalfiltered = aisJurnal
                    .Where(f => f.DIVISI == div && !(f.ISBORONGAN == 1 && f.JENIS == "PANEN"))
                    .OrderBy(f => f.JENIS)
                    .ThenBy(f => f.PEKERJAAN)
                    .ThenBy(f => f.BLOK)
                    .ToList();

                // Retrieve KOMPONEN_KHT data
                var komponenKHTResult = await jurnalimportService.GetKOMPONEN_KHTAsync(iddata, periode, remise, div);

                // Conditionally retrieve BPJS data
                IEnumerable<BPJS_INFO_DTO> bpjsResult = null;
                if (remise == 2)
                {
                    bpjsResult = await jurnalimportService.GetBPJSInfoListAsync(periode, div);
                }

                var filteredKomponenKHT = komponenKHTResult.Where(k => k.DIVISIID == div).ToList();
                var filteredBpjs = bpjsResult?.Where(b => b.DIVISIID == div).ToList() ?? new List<BPJS_INFO_DTO>();

                // Check if filteredKomponenKHT has data
                var hasKomponenKHTData = filteredKomponenKHT.Any();

                // Calculate totals
                var totals = new
                {
                    Libur = hasKomponenKHTData ? filteredKomponenKHT.Sum(k => k.LIBURDIBAYAR) : 0m,
                    Umakan = hasKomponenKHTData ? filteredKomponenKHT.Sum(k => k.UMAKAN) : 0m,
                    Perabot = hasKomponenKHTData ? filteredKomponenKHT.Sum(k => k.TJGPERABOT) : 0m,
                    Kompensasi = hasKomponenKHTData ? filteredKomponenKHT.Sum(k => k.KOMPENSASI) : 0m,
                    Lembur = hasKomponenKHTData ? filteredKomponenKHT.Sum(k => k.PREMIDANLEMBUR) : 0m,
                    PotKantor = 0m,
                    PotBpjsTK = filteredBpjs.Any() ? filteredBpjs.Sum(b => b.POT_BPJS_TK) : 0m,
                    PotBpjsKES = filteredBpjs.Any() ? filteredBpjs.Sum(b => b.POT_BPJS_KESEHATAN) : 0m,
                    PotBpjsPENSIUN = filteredBpjs.Any() ? filteredBpjs.Sum(b => b.POT_BPJS_TK_PENSIUN) : 0m
                };

                decimal biayaoperasional_bruto = JurnalAmountRounding.RoundJournalAmount(jurnalfiltered.Sum(f => f.DEBET) + totals.Libur + totals.Umakan + totals.Perabot + totals.Kompensasi + totals.Lembur);
                decimal biayaoperasional_netto = JurnalAmountRounding.RoundJournalAmount(biayaoperasional_bruto - (totals.PotKantor + totals.PotBpjsTK + totals.PotBpjsKES + totals.PotBpjsPENSIUN));

                var newRows = new List<AIS_JURNAL>
                {
                    new() { KODE = "40.00001.001", REKENING = " ", DEBET = totals.Libur, KETERANGAN = "Hari Libur Nasional " + ket2, POSTED = true, PERIODE = periodestring },
                    new() { KODE = "40.00001.001", REKENING = " ", DEBET = totals.Umakan, KETERANGAN = "Uang Makan " + ket2, POSTED = true, PERIODE = periodestring },
                    new() { KODE = "70.10001.006", REKENING = "Tunjangan Perumahan", DEBET = totals.Perabot, KETERANGAN = "Tunjangan Perabot " + ket2, POSTED = true, PERIODE = periodestring },
                    new() { KODE = "40.00001.001", REKENING = " ", DEBET = totals.Kompensasi, KETERANGAN = "Kompensasi " + ket2, POSTED = true, PERIODE = periodestring },
                    new() { KODE = "40.00001.001", REKENING = " ", DEBET = totals.Lembur, KETERANGAN = "Lembur " + ket2, POSTED = true, PERIODE = periodestring },
                    new() { KODE = "40.00001.001", REKENING = " ", KREDIT = totals.PotKantor, KETERANGAN = "Potongan Kantor " + ket2, POSTED = true, PERIODE = periodestring }
                };

                if (remise != 1)
                {
                    newRows.AddRange(new List<AIS_JURNAL>
            {
                new() { KODE = "70.10001.005", REKENING = "Tunjangan Astek", KREDIT = totals.PotBpjsTK, KETERANGAN = "Potongan JHT Karyawan " + ket2, POSTED = true, PERIODE = periodestring },
                new() { KODE = "70.10001.005", REKENING = "Tunjangan Astek", KREDIT = totals.PotBpjsKES, KETERANGAN = "Potongan BPJS Karyawan " + ket2, POSTED = true, PERIODE = periodestring },
                new() { KODE = "70.10001.005", REKENING = "Tunjangan Astek", KREDIT = totals.PotBpjsPENSIUN, KETERANGAN = "Potongan PENSIUN Karyawan " + ket2, POSTED = true, PERIODE = periodestring }
            });
                }

                // Add final row
                newRows.Add(new AIS_JURNAL
                {
                    KODE = "33.00001.001",
                    REKENING = "Gaji dan Upah YMH dibayar",
                    KREDIT = biayaoperasional_netto,
                    KETERANGAN = "TOTAL BIAYA OPERASIONAL " + ket2,
                    POSTED = true,
                    PERIODE = periodestring
                });

                // Add the new rows to the list
                jurnalfiltered.AddRange(newRows);
            }

            // Add sequential numbering
            int nomorUrut = 1;
            foreach (var item in jurnalfiltered)
            {
                item.NO = nomorUrut++;
            }

            // Convert to AIS_JURNAL_FINAL list
            var jurnalfinalAIS = JurnalAmountRounding.NormalizeAisFinalRows(jurnalfiltered.Select(j => new AIS_JURNAL_FINAL
            {
                NOJURNAL = noAIS_Bukti,
                TANGGAL = TanggalJurnal,
                NO = j.NO,
                KODE = j.KODE,
                REKENING = j.REKENING,
                KETERANGAN = j.KETERANGAN,
                KREDIT = j.KREDIT,
                DEBET = j.DEBET
            }));

            return jurnalfinalAIS;
        }

    }
}
