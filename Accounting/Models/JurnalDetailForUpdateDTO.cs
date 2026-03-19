using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Model
{
    public class JurnalDetailForUpdateDTO
    {

        public Double REFFID { get; set; }
        public string HIDREFF { get; set; }
        public string NoJurnal { get; set; }
        public DateTime Tanggal { get; set; }
        public int RowNo { get; set; }
        public string Kode { get; set; }
        public string Rekening { get; set; }
        public decimal? Debet { get; set; }
        public decimal? Kredit { get; set; }
        public string Keterangan { get; set; }
        public string Posted { get; set; }
        public string Periode { get; set; }
    }
}
