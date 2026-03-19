using System.ComponentModel.DataAnnotations;

namespace Accounting.Model
{
    public class COA_SaldoAwal
    {       
        public string Account { get; set; }
        public string NamaPerkiraan { get; set; }
        public decimal AwalTahun { get; set; }
    }
}
