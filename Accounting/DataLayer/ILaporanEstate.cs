using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    public interface ILaporanEstate
    {
        DataTable Divisi(string p_iddata, int p_tahun);
        
        DataSet TBM_BYDIVISI(string p_iddata, int p_tahun, int p_bulan);
        DataSet TBM_BYDIVISI_Quarter(string p_iddata, int p_tahun, string p_quarter);
        DataSet TBM_BYDIVISI_Semester(string p_iddata, int p_tahun, string p_semester);
        DataSet TBM_BYDIVISI_Tahun(string p_iddata, int p_tahun);

        DataSet TM_BYDIVISI(string p_iddata, int p_tahun, int p_bulan);
        DataSet TM_BYDIVISI_Quarter(string p_iddata, int p_tahun, string p_quarter);
        DataSet TM_BYDIVISI_Semester(string p_iddata, int p_tahun, string p_semester);
        DataSet TM_BYDIVISI_Tahun(string p_iddata, int p_tahun);
    }
}
