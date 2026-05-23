using Accounting._1.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accounting.BusinessLayer
{
    public static class JurnalAmountRounding
    {
        public static decimal RoundJournalAmount(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        public static List<AIS_JURNAL_FINAL> NormalizeAisFinalRows(IEnumerable<AIS_JURNAL_FINAL> rows)
        {
            return rows.Select(row => new AIS_JURNAL_FINAL
            {
                NOJURNAL = row.NOJURNAL,
                TANGGAL = row.TANGGAL,
                NO = row.NO,
                KODE = row.KODE,
                REKENING = row.REKENING,
                DEBET = RoundJournalAmount(row.DEBET),
                KREDIT = RoundJournalAmount(row.KREDIT),
                KETERANGAN = row.KETERANGAN,
                POSTED = row.POSTED,
                PERIODE = row.PERIODE
            }).ToList();
        }
    }
}
