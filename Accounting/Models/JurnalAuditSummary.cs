using System;

namespace Accounting.Model
{
    public class JurnalAuditSummary
    {
        public double JURNALID { get; set; }
        public string NOJURNAL { get; set; }
        public DateTime? TANGGAL { get; set; }
        public string PERIODE { get; set; }
        public int JUMLAH_AKSI { get; set; }
        public DateTime LAST_ACTION_DATE { get; set; }
        public string ACTION_TYPES { get; set; }
    }
}
