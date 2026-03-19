using Accounting.DataLayer;
using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Accounting.BusinessLayer
{
    public static class AccountServices
    {
        static IAccountRepository repository;
        static AccountServices()
        {
            repository = new AccountRepository();
        }

        public static int GetMinPeriode(string piddata)
        {
            return repository.GetMinPeriode(piddata);
        }
        public static int MinTahunCOA(string piddata)
        {
            return repository.MinTahunCOA(piddata);
        }
        public static int MaxTahunCOA(string piddata)
        {
            return repository.MaxTahunCOA(piddata);
        }
        public static int GetMaxPeriode(string piddata)
        {
            return repository.GetMaxPeriode(piddata);
        }
        public static DataTable CekParentNotExist()
        {
            return repository.CekParentNotExist();
        }
        public static DataTable CekParentNotExist2()
        {
            return repository.CekParentNotExist2();
        }
        public static DataTable CekParentNotExist3(string p_iddata,int p_tahun)
        {
            return repository.CekParentNotExist3(p_iddata, p_tahun);
        }
        public static DataTable CekSalahInduk()
        {
            return repository.CekSalahInduk();
        }
        public static int CekPeriodeExist(string piddata, int p_bulan, int p_tahun)
        {
            return repository.CekPeriodeExist( piddata,  p_bulan,  p_tahun);
        }
        public static int CekCountKode(string piddata, int ptahun, string pinduk,string pGD)
        {
            return repository.CekCountKode( piddata,  ptahun,  pinduk, pGD);
        }
        
        public static int CekCOAExist(string piddata, int p_tahun)
        {
            return repository.CekCOAExist(piddata,  p_tahun);
        }

        public static int ImportCOA(string piddata, int p_tahun)
        {
            return repository.ImportCOA(piddata, p_tahun);
        }

        public static string GetNamaPeriode(string piddata)
        {
            return repository.GetNamaPeriode(piddata);
        }
        public static int RekalkulasiByNoJurnal(string piddata, int p_bulan, int p_tahun, string p_NoJurnal, string p_Periode, string p_Userid)
        {
            return repository.RekalkulasiByNoJurnal( piddata,  p_bulan,  p_tahun,  p_NoJurnal,  p_Periode,  p_Userid);
        }
        public static void RekalkulasiByJurnalID(string piddata, int p_bulan, int p_tahun, Double p_JurnalID, string p_Periode, string p_Userid)
        {
            repository.RekalkulasiByJurnalID(piddata, p_bulan, p_tahun, p_JurnalID, p_Periode, p_Userid);
        }
        public static int ReCalcByNoHID(string piddata, int p_bulan, int p_tahun, string p_HID, string p_Periode, string p_Userid)
        {
            return repository.ReCalcByNoHID(piddata, p_bulan, p_tahun, p_HID, p_Periode, p_Userid);
        }
        public static void RekalkulasiSaldo(string piddata, int p_bulan, int p_tahun,  string p_Userid)
        {
            repository.RekalkulasiSaldo( piddata,  p_bulan,  p_tahun,  p_Userid);
        }
        public static void RekalkulasiSaldoV2(string piddata, int p_bulan, int p_tahun, string p_Userid)
        {
            repository.RekalkulasiSaldoV2(piddata, p_bulan, p_tahun, p_Userid);
        }
        public static void CreateNextPeriode(string piddata, int p_bulan, int p_tahun)
        {
            repository.CreateNextPeriode(piddata, p_bulan, p_tahun);
        }

        public static void CreateClosingAcct(string piddata)
        {
            repository.CreateClosingAcct(piddata);
        }
        public static void UpdateSaldoAwalHeader(string piddata, int p_bulan, int p_tahun, string p_userid)
        {
            repository.UpdateSaldoAwalHeader(piddata, p_bulan, p_tahun, p_userid);
        }
        public static void ClosingEndYear(string piddata, int p_tahun,string p_userid)
        {
            repository.ClosingEndYear(piddata,  p_tahun,p_userid);
        }
        public static void ClosingEndYearUpdateOnly(string piddata, int p_tahun, string p_userid)
        {
            repository.ClosingEndYearUpdateOnly(piddata, p_tahun, p_userid);
        }
        public static DataSet PeriodeAkuntansi(string piddata, int ptahun)
        {
            return repository.PeriodeAkuntansi(piddata,ptahun);
        }
        public static DataTable GetParentAccount(string piddata,int p_tahun, string ptipe)
        {
            return repository.GetParentAccount(piddata, p_tahun,ptipe);
        }
        public static DataTable GetTipeAkun(string dariform)
        {
            return repository.GetTipeAkun(dariform);
        }

        public static void InsertCOA(string piddata, int p_tahun, string pgrp, string pinduk, char pgd, string pkode, int plvl,
             char pposisi, string pnama, decimal psawal)
        {
            repository.InsertCOA( piddata,  p_tahun,  pgrp,  pinduk,pgd,  pkode,  plvl,pposisi,  pnama,  psawal);
        }

        internal static int ImportCOAbyMerge(string piddata, int p_tahun)
        {
            return repository.ImportCOAbyMerge(piddata, p_tahun);
        }
        public static void UpdateLevelAccount(string piddata, int p_tahun)
        {
            repository.UpdateLevelAccount( piddata, p_tahun);
        }
        public static void ReclassLabaRugi(string piddata, int p_tahun, string p_userid)
        {
            repository.ReclassLabaRugi(piddata, p_tahun, p_userid);
        }
        public static IQueryable<COADaftarPerkiraanSaldoDTO> GetPerkiraanSaldo_Dapper(string p_iddata, int p_tahun, int p_bulan)
        {
            return   repository.GetPerkiraanSaldo_Dapper( p_iddata,  p_tahun,  p_bulan);
        }
        public static DataTable GetPerkiraanSaldo_ADO(string p_iddata, int p_tahun, int p_bulan)
        {
            return repository.GetPerkiraanSaldo_ADO(p_iddata, p_tahun, p_bulan);
        }
        public static IEnumerable<coaHIA> GetPerkiraanSaldo_TreeView(string p_iddata, int p_tahun, int p_bulan)
        {
            return repository.GetPerkiraanSaldo_TreeView(p_iddata, p_tahun, p_bulan);
        }
        public static bool ValidateColumnNames(DataTable dataTable, string[] NamaKolom)
        {
            return repository.ValidateColumnNames(dataTable, NamaKolom);
        }
        public static DataTable GetMasterAkun(string kategori, string kelompok)
        {
            return repository.GetMasterAkun(kategori, kelompok);
        }
        public static DataTable GetIndukAkun(string kategori, string kelompok)
        {
            return repository.GetIndukAkun(kategori, kelompok);
        }
        public static void UpdateCOA(string coaId, string kodeAcc, string grp, string parentAcc, string namaAcc, char isAktif, string lvl)
        {
            repository.UpdateCOA(coaId, kodeAcc, grp, parentAcc, namaAcc, isAktif, lvl);
        }
        public static void DeleteCOA(string coaId)
        {
            repository.DeleteCOA(coaId);
        }
        public static void UpdateCOATmpIdData(string iddata, int tahun, string userid)
        {
            repository.UpdateCOATmpIdData(iddata, tahun, userid);
        }
        public static void TruncateCOATmp()
        {
            repository.TruncateCOATmp();
        }
        public static DataTable Akun_Agronomy(string p_iddata, int p_tahun, string p_kelompok)
        {
            return repository.Akun_Agronomy( p_iddata,  p_tahun, p_kelompok);
        }
    }
}
