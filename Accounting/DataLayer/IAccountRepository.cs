using Accounting.Model;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    public interface IAccountRepository
    {
        DataSet PeriodeAkuntansi(string piddata,int ptahun);
        int MaxTahunCOA(string piddata);
        int MinTahunCOA(string piddata);
        int CekPeriodeExist(string piddata, int pbulan,int ptahun);
        int CekCountKode(string piddata,int ptahun,string pinduk, string pGD);
        int CekCOAExist(string iddata, int ptahun);
        DataTable CekParentNotExist();
        DataTable CekParentNotExist2();
        DataTable CekParentNotExist3(string iddata, int ptahun);
        DataTable CekSalahInduk();
        int ImportCOA(string iddata, int ptahun);
        int GetMaxPeriode(string piddata);
        int GetMinPeriode(string piddata);
        string GetNamaPeriode(string piddata);
        DataTable GetCOAWithoutSaldo(string piddata);
        DataTable GetTipeAkun(string dariform);
        DataTable GetParentAccount(string piddata, int p_tahun,string ptipe);
        int RekalkulasiByNoJurnal(string piddata, int p_bulan, int p_tahun, string p_NoJurnal,string p_Periode,string p_Userid);
        void RekalkulasiByJurnalID(string piddata, int p_bulan, int p_tahun, double p_JurnalID, string p_Periode, string p_Userid);
        int ReCalcByNoHID(string piddata, int p_bulan, int p_tahun, string p_HID, string p_Periode, string p_Userid);
        void RekalkulasiSaldo(string piddata, int p_bulan, int p_tahun , string p_Userid );
        void RekalkulasiSaldoV2(string piddata, int p_bulan, int p_tahun, string p_Userid);
        void CreateNextPeriode(string piddata, int p_bulan, int p_tahun);
        void UpdateLevelAccount(string piddata, int p_tahun);
        void CreateClosingAcct(string piddata);
        void UpdateSaldoAwalHeader(string piddata, int p_bulan, int p_tahun,string p_userid);
        void ClosingEndYear(string piddata,int p_tahun,string p_userid);
        void ClosingEndYearUpdateOnly(string piddata, int p_tahun, string p_userid);
        void ReclassLabaRugi(string piddata, int p_tahun, string p_userid);
        void InsertCOA(string piddata, int p_tahun, string pgrp, string pinduk, char pgd, string pkode,int plvl, char pposisi
                ,string pnama,decimal psawal);
        int ImportCOAbyMerge(string piddata, int p_tahun);

        IQueryable<COADaftarPerkiraanSaldoDTO> GetPerkiraanSaldo_Dapper(string p_iddata, int p_tahun, int p_bulan);
        DataTable GetPerkiraanSaldo_ADO(string p_iddata, int p_tahun, int p_bulan);
        IEnumerable<coaHIA> GetPerkiraanSaldo_TreeView(string p_iddata, int p_tahun, int p_bulan);
        bool ValidateColumnNames(DataTable dataTable, string[] NamaKolom);
        DataTable Akun_Agronomy(string p_iddata, int p_tahun,string p_kelompok);
    }
}
