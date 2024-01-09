using System.ComponentModel.DataAnnotations;

namespace Accounting.Model
{
    public class COADaftarPerkiraanSaldoDTO
    {
        public string ID { get; set; }
        public string KODEACC { get; set; }
        public string INDUK { get; set; }
        public string GD { get; set; }
        public string NAMAACC { get; set; }
        public string GRP { get; set; }
        public byte LVL { get; set; }
        public string ISAKTIF { get; set; }
        public string POSISI { get; set; }
        public decimal? AWALTAHUN { get; set; }
        public decimal? SALDOAWAL { get; set; }
        public decimal? DEBET { get; set; }
        public decimal? KREDIT { get; set; }
        public decimal? SALDOAKHIR { get; set; }
        public string DIVISI { get; set; }
        public string BLOK { get; set; }
        public string TAHUNTANAM { get; set; }

    }
}
