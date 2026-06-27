using Accounting.DataLayer;
using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.BusinessLayer
{
    public static class LaporanServices
    {
        static ILaporanRepository repository;
        static LaporanServices()
        {
            repository = new LaporanRepository();
        }
        public static Decimal Balanced_Check(string piddata, int p_bulan, int p_tahun)
        {
            return repository.Balanced_Check(piddata, p_bulan, p_tahun);
        }
        public static DataSet ViewAccountingReport(string piddata, int p_bulan, int p_tahun, string p_Userid, string reportCode, string jenisakunting)
        {
            return repository.ViewAccountingReport(piddata, p_bulan, p_tahun, p_Userid, reportCode, jenisakunting);
        }

        public static DataSet ViewAccountingReportDrillDown(string piddata, int p_bulan, int p_tahun, string reportCode, int sectionId, string p_kodeacc)
        {
            return repository.ViewAccountingReportDrillDown(piddata, p_bulan, p_tahun, reportCode, sectionId, p_kodeacc);
        }

        // Laba Rugi V2: single round-trip generate + fetch (package ACCT_LAPORAN_V2).
        public static DataSet ViewLap_LabaRugi_V2(string piddata, int p_bulan, int p_tahun, string p_Userid, string jenisakunting)
        {
            return repository.ViewLap_LabaRugi_V2(piddata, p_bulan, p_tahun, p_Userid, jenisakunting);
        }
        public static List<LabaRugiRow> ViewLap_LabaRugiRows_V2(string piddata, int p_bulan, int p_tahun, string p_Userid, string jenisakunting)
        {
            return repository.ViewLap_LabaRugiRows_V2(piddata, p_bulan, p_tahun, p_Userid, jenisakunting);
        }
        public static Decimal Generate_Jurnal_Closing(string piddata, int p_bulan, int p_tahun, string p_Userid, string jenisakunting)
        {
            return repository.Generate_Jurnal_Closing(piddata, p_bulan, p_tahun, p_Userid, jenisakunting);
        }
        public static int GenerateSub_LabaRugi(string p_IDDATA, int p_bulan, int p_tahun, string p_kodeacc, string userid, string lap, string posisi)
        {
            return repository.GenerateSub_LabaRugi( p_IDDATA,  p_bulan,  p_tahun,  p_kodeacc,  userid,lap,posisi);
        }
        public static DataSet ViewLap_Neraca(string p_IDDATA, int p_bulan, int p_tahun,   string userid)
        {
            return repository.ViewLap_Neraca(p_IDDATA, p_bulan, p_tahun,  userid);
        }
        public static List<NeracaRow> ViewLap_NeracaRows_V2(string p_IDDATA, int p_bulan, int p_tahun,   string userid)
        {
            return repository.ViewLap_NeracaRows_V2(p_IDDATA, p_bulan, p_tahun,  userid);
        }
        public static DataSet ViewSub_Neraca(string p_IDDATA, int p_bulan, int p_tahun, string p_kodeacc, string userid, string posisi)
        {
            return repository.ViewSub_Neraca(p_IDDATA, p_bulan, p_tahun, p_kodeacc, userid, posisi);
        }
        public static DataSet ViewLap_NeracaLajur(string p_IDDATA, int p_bulan, int p_tahun)
        {
            return repository.ViewLap_NeracaLajur(p_IDDATA, p_bulan, p_tahun);
        }
        public static DataSet ViewLap_NeracaHalfYear(string piddata, int p_tahun, string userid, int ishalf)
        {
            return repository.ViewLap_NeracaHalfYear(piddata,p_tahun, userid, ishalf);
        }
        public static DataTable ViewLap_NeracaKonsolidasi(int p_tahun, string p_pt, int p_bulan, string userid)
        {
            return repository.ViewLap_NeracaKonsolidasi(p_tahun,p_pt, p_bulan, userid);
        }
        public static DataSet ViewSub_LabaRugi(string piddata, string userid)
        {
            return repository.ViewSub_LabaRugi( piddata,  userid);
        }

        public static DataSet View_Jurnal(string piddata, string periode, string kode)
        {
            return repository.View_Jurnal( piddata,  periode,  kode);
        }
        public static DataSet ViewLap_BukuBesar(string P_IDDATA, int p_tahun, int p_bulan, int p_sampaibulan, string DARIKODE, string SAMPAIKODE
            , string p_Userid, string DARILAPORAN)
        {
            return repository.ViewLap_BukuBesar( P_IDDATA, p_tahun, p_bulan, p_sampaibulan,  DARIKODE,  SAMPAIKODE,p_Userid, DARILAPORAN);
        }
        public static DataSet ViewLap_BukuBesar_Tree(string P_IDDATA, int p_tahun, int p_bulan, int p_sampaibulan, string p_kode)
        {
            return repository.ViewLap_BukuBesar_Tree(P_IDDATA, p_tahun, p_bulan, p_sampaibulan, p_kode);
        }
        public static DataSet ViewLap_BukuBesarMultiTahun(string P_IDDATA, int p_tahundari, int p_tahunsampai, int p_bulan, int p_sampaibulan, string DARIKODE, string SAMPAIKODE
            , string p_Userid, string DARILAPORAN)
        {
            return repository.ViewLap_BukuBesarMultiTahun(P_IDDATA, p_tahundari,p_tahunsampai, p_bulan, p_sampaibulan, DARIKODE, SAMPAIKODE, p_Userid, DARILAPORAN);
        }
        public static List<AccountSummary> NeracaSaldoTahun(string piddata, int p_tahun)
        {
            return repository.NeracaSaldoTahun( piddata,  p_tahun);
        }
    }
}
