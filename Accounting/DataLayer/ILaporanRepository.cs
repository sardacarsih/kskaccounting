using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    public interface ILaporanRepository
    {
        Decimal Balanced_Check(string piddata, int pbulan, int ptahun);
        Decimal Generate_LabaRugi(string piddata, int pbulan, int ptahun, string userid, string jenisakunting);
        Decimal Generate_Jurnal_Closing(string piddata, int pbulan, int ptahun, string userid, string jenisakunting);
        int GenerateSub_LabaRugi(string p_IDDATA, int p_bulan, int p_tahun, string p_kodeacc, string userid, string lap, string posisi);
        DataSet ViewLap_LabaRugi(string piddata,string userid);
        DataSet ViewLap_Neraca(string piddata, int p_bulan, int p_tahun, string userid);
        DataSet ViewLap_NeracaHalfYear(string piddata, int p_tahun, string userid, int ishalf);
        DataTable ViewLap_NeracaKonsolidasi(int p_tahun, string p_pt,int p_bulan, string userid);
        DataSet ViewSub_LabaRugi(string piddata, string userid);        
        DataSet ViewLap_BukuBesar(string P_IDDATA, int p_tahun, int p_bulan, int p_sampaibulan, string DARIKODE, string SAMPAIKODE
            ,string p_Userid, string DARILAPORAN);
        DataSet ViewLap_BukuBesarMultiTahun(
            string P_IDDATA
            , int p_tahundari
            , int p_tahunsampai
            , int p_bulan
            , int p_sampaibulan
            , string DARIKODE
            , string SAMPAIKODE
            , string p_Userid
            , string DARILAPORAN);
        DataSet View_Jurnal(string piddata, string periode, string kode);
        DataSet ViewLap_NeracaLajur(string piddata, int p_bulan, int p_tahun);
        List<AccountSummary> NeracaSaldoTahun(string piddata, int p_tahun);
    }
}
