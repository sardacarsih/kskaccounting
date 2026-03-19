using System.ComponentModel.DataAnnotations;

namespace Accounting.Model
{
    public class COA_ImportKEBUN
    {       
        public string Account { get; set; }
        public string NamaPerkiraan { get; set; }
        public string Jenis { get; set; }
        public string Level { get; set; }
        public string Induk { get; set; }
        public string Gen { get; set; }
        public string Posisi { get; set; }
        public decimal AwalTahun { get; set; }
        public string Blok { get; set; }
        public string Divisi { get; set; }
        public string TahunTanam { get; set; }

    }
}
