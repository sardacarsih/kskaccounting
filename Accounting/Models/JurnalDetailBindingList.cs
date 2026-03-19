using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Model
{
    public class JurnalDetailBindingList
    {
            public int BARIS { get; set; }
            public string Kode { get; set; }
            public string Rekening { get; set; }
        public double Debet { get; set; }
        public double Kredit { get; set; }
        public string Keterangan { get; set; }
        public JurnalDetailBindingList(int baris, string kode, string rekening,double debet,double kredit,string keterangan)
        {
            BARIS = baris;
            Kode = kode;
            Rekening = rekening;
            Debet = debet;
            Kredit = kredit;
            Keterangan = keterangan;
        }
    }
}
