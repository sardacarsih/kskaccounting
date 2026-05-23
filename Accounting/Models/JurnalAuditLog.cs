using System;

namespace Accounting.Model
{
    public class JurnalAuditLog
    {
        public double AUDIT_ID { get; set; }
        public double JURNALID { get; set; }
        public string ACTION_TYPE { get; set; }
        public DateTime ACTION_DATE { get; set; }
        public string ACTION_BY { get; set; }
        public string ACTION_PC { get; set; }
        public string ACTION_IP { get; set; }
        public string NOJURNAL { get; set; }
        public DateTime? TANGGAL { get; set; }
        public string PERIODE { get; set; }
        public string SUMBER { get; set; }
        public string IDDATA { get; set; }
        public string CHANGED_FIELDS { get; set; }
        public string DELETE_REASON { get; set; }
        public int DETAIL_ROWS_INSERTED { get; set; }
        public int DETAIL_ROWS_UPDATED { get; set; }
        public int DETAIL_ROWS_DELETED { get; set; }
    }
}
