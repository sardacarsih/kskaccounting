namespace Accounting.Model
{
    public class JurnalAuditDetailDTO
    {
        public double AUDIT_DTL_ID { get; set; }
        public double AUDIT_ID { get; set; }
        public string CHANGE_TYPE { get; set; }
        public int BARIS { get; set; }
        public string KODE { get; set; }
        public string REKENING { get; set; }
        public decimal DEBET { get; set; }
        public decimal KREDIT { get; set; }
        public string KETERANGAN { get; set; }
        public string OLD_KODE { get; set; }
        public decimal? OLD_DEBET { get; set; }
        public decimal? OLD_KREDIT { get; set; }
        public string OLD_KETERANGAN { get; set; }
    }
}
