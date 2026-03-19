using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Model
{
    public class LaporanNeraca
    {
        public LaporanNeraca(string kode, string cat1, string kat, string cat2, string akun, string parentAkun, string tipe, string posisi, decimal bulanIni, decimal bulanLalu, decimal awalTahun)
        {
            KODE = kode;
            CAT1 = cat1;
            KAT = kat;
            CAT2 = cat2;
            AKUN = akun;
            PARENTAKUN = parentAkun;
            TIPE = tipe;
            POSISI = posisi;
            BULANINI = bulanIni;
            BULANLALU = bulanLalu;
            AWALTAHUN = awalTahun;
        }
        public string KODE { get; set; }
        public string CAT1 { get; set; }
        public string KAT { get; set; }
        public string CAT2 { get; set; }
        public string AKUN { get; set; }
        public string PARENTAKUN { get; set; }
        public string TIPE { get; set; }
        public string POSISI { get; set; }
        public decimal BULANINI { get; set; }
        public decimal BULANLALU { get; set; }
        public decimal AWALTAHUN { get; set; }
        
    }

}
