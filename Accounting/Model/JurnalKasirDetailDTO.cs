using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Model
{
    public class JurnalKasirDetailDTO
    {
       
        public string NoJurnal { get; set; }
        public DateTime Tanggal { get; set; }
        public int Baris { get; set; }
        public string Kode { get; set; }       
        public string Rekening { get; set; }
        public decimal Debet { get; set; }
        public decimal Kredit { get; set; }
        public string Keterangan { get; set; }
        public string Posted { get; set; }
        public string Periode { get; set; }
        public string Iddata { get; set; }
        public string Userid { get; set; }
        public int GLyear { get; set; }
        public int GLmonth { get; set; }

    }
}
