using System.ComponentModel.DataAnnotations;

namespace Accounting.Model
{
    public class COA_List
    {
        [Key]
        [StringLength(25)]
        public string ID { get; set; }
        public string KODEACC { get; set; }

        [StringLength(30)]
        public string INDUK { get; set; }

        [Required]
        [StringLength(1)]
        public string GD { get; set; }

        [Required]
        [StringLength(100)]
        public string NAMAACC { get; set; }

        [Required]
        [StringLength(2)]
        public string GRP { get; set; }

        public byte LVL { get; set; }

        [StringLength(1)]
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
