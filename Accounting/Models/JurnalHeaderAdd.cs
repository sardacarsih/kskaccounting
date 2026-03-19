using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Model
{
    public class JurnalHeaderAdd
    {
        public Double JURNALID { get; set; }
        public string HID { get; set; }
        public string NOJURNAL { get; set; }
        public DateTime TANGGAL { get; set; }
        public string PERIODE { get; set; }
        public string IDDATA { get; set; }
        public string USERID { get; set; }
        public string SUMBER { get; set; }
        public string ISRE { get; set; }
        public string PC { get; set; }
        public string IP_ADD { get; set; }

        // Audit fields
        public DateTime? CREATED_DATE { get; set; }
        public string CREATED_BY { get; set; }
        public DateTime? MODIFIED_DATE { get; set; }
        public string MODIFIED_BY { get; set; }
        public string MODIFIED_PC { get; set; }
        public string MODIFIED_IP { get; set; }
    }
}
