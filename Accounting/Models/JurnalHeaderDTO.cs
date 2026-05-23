using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Model
{
    public class JurnalHeaderDTO
    {
        [Key]
        public Double JURNALID { get; set; }
        public string HID { get; set; }
        public string NoJurnal { get; set; }
        public DateTime Tanggal { get; set; }
        public DateTime HeaderVersionUtc { get; set; }
    }
}
