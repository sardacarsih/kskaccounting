using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Models
{
    public class JurnalBlokRekap
    {
        public string IDDATA { get; set; }
        public string ESTATE { get; set; }
        public string PERIODE { get; set; }
        public decimal DEBET { get; set; }
        public decimal KREDIT { get; set; }
    }
}
