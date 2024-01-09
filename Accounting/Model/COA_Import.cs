using System.ComponentModel.DataAnnotations;

namespace Accounting.Model
{
    public class COA_Import
    {       
        public string Account { get; set; }
        public string NamaPerkiraan { get; set; }
        public string Jenis { get; set; }
        public string Level { get; set; }
        public string Induk { get; set; }
        public string Gen { get; set; }
        public string Posisi { get; set; }
        public decimal AwalTahun { get; set; }
    }
}
