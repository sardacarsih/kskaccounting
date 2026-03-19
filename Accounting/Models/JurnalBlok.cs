using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Models
{
    public class JurnalBlok
    {
        public string IDDATA { get; set; }
        public string ESTATE { get; set; } // Assuming ESTATEID is a string
        public string DIVISI { get; set; }
        public string KODEBLOK { get; set; }
        public string BLOK { get; set; }
        public string KATEGORI { get; set; }
        public string GROUPID { get; set; }
        public string PERIODE { get; set; }
        public string NOJURNAL { get; set; }
        public DateTime TANGGAL { get; set; } // Assuming TANGGAL is a date
        public string KODE { get; set; }
        public string REKENING { get; set; }
        public decimal DEBET { get; set; } // Assuming DEBET is a decimal
        public decimal KREDIT { get; set; } // Assuming DEBET is a decimal
        public string KETERANGAN { get; set; }
    }
}
