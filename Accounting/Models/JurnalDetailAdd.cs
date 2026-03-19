using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Model
{
    public class JurnalDetailAdd
    {
        //static with initial from gridview
          
        public int BARIS { get; set; }        
        public string Kode { get; set; }       
        public string Rekening { get; set; }
        public decimal? Debet { get; set; }
        public decimal? Kredit { get; set; }
        public string Keterangan { get; set; }
        public string Posted { get; set; }        
        public string IDDATA { get; set; }
        public string SUMBER { get; set; }
        public string USERID { get; set; }

        //dynamic update from foreach

        [Key]
        public string DID { get; set; }
        public double REFFID { get; set; }
        public string HIDREFF { get; set; }
        public string NoJurnal { get; set; }
        public DateTime Tanggal { get; set; }
        public string Periode { get; set; }
        public int GLYEAR { get; set; }
        public int GLMONTH { get; set; }
    }
}
