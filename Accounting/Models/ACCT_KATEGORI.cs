using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Model
{
    public class ACCT_KATEGORI
    {
        public string KODE { get; set; }
        public string? KETERANGAN { get; set; }
        public string KELOMPOK { get; set; }
        public string SISI { get; set; }
        public int KELOMPOKID { get; set; }
        public Decimal BULANINI { get; set; }
        public Decimal BULANLALU { get; set; }
        public Decimal AWALTAHUN { get; set; }
        public List<LaporanNeraca> DetailsAKUN { get; set; }
    }
}
