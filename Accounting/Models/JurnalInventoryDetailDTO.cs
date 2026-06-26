using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Model
{
    public class JurnalInventoryDetailDTO
    {
        public string NOJURNAL { get; set; }
        public DateTime TANGGAL { get; set; }
        public Int16 BARIS { get; set; }
        public string KODE { get; set; }
        public string REKENING { get; set; }
        public decimal DEBET  { get; set; }
        public decimal KREDIT { get; set; }
        public string KETERANGAN { get; set; }
        public string POSTED { get; set; }
        public string PERIODE { get; set; }
        public string IDDATA { get; set; }
        public string USERID { get; set; }
        public Int16 GLYEAR { get; set; }
        public Int16 GLMONTH { get; set; }

    }
}
