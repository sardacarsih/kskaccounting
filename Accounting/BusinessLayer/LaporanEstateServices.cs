using Accounting.DataLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.BusinessLayer
{
    public static class LaporanEstateServices
    {
        static ILaporanEstate repository;
        static LaporanEstateServices()
        {
            repository = new LaporanEstate();
        }
        public static DataSet TBM_BYDIVISI_Quarter(string p_iddata,int p_tahun,string p_quarter)
        {
            return repository.TBM_BYDIVISI_Quarter(p_iddata, p_tahun, p_quarter);
        }


        public static DataSet TM_BYDIVISI_Quarter(string p_iddata, int p_tahun, string p_quarter)
        {
            return repository.TM_BYDIVISI_Quarter(p_iddata, p_tahun, p_quarter);
        }
    }
}
