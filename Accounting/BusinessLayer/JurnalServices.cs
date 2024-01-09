using Accounting.DataLayer;
using Accounting.Model;
using DevExpress.Office.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.BusinessLayer
{
    public static class JurnalServices
    {
        static IJurnalRepository repository;

        
        static JurnalServices()
        {
            repository = new JurnalRepository();
        }

        public static DataTable GetJurnalHeader(string piddata, string periode)
        {
            return repository.GetJurnalHeader(piddata, periode);
        }
        public static DataSet GetNotaDebet(string piddata, string pperiode, string pkodeacc)
        {
            return repository.GetNotaDebet(piddata, pperiode, pkodeacc);
        }
        public static DataSet GetNotaKredit(string piddata, string pperiode, string pkodeacc)
        {
            return repository.GetNotaKredit(piddata, pperiode, pkodeacc);
        }
        public static DataTable GetJurnalDetails(string piddata, string periode)
        {
            return repository.GetJurnalDetails(piddata, periode);
        }
        public static DataTable GetJurnalDetailsV2(string piddata, string periode)
        {
            return repository.GetJurnalDetailsV2(piddata, periode);
        }
        public static DataTable CekAkunMaster(int ptahun)
        {
            return repository.CekAkunMaster(ptahun);
        }
        public static DataTable CekDuplikasiJurnal()
        {
            return repository.CekDuplikasiJurnal();
        }
        public static DataTable CekNoJurnalExist()
        {
            return repository.CekNoJurnalExist();
        }
        public static bool ValidateColumnNames(DataTable dataTable, string[] NamaKolom)
        {
            return repository.ValidateColumnNames(dataTable,NamaKolom);
        }
       

        public static bool CekNoJurnalExist_input(string piddata, string p_nomor, string periode)
        {
            return repository.CekNoJurnalExist_input(piddata, p_nomor, periode);
        }
        public static string GetLockStatus(string piddata, string periode)
        {
            return repository.GetLockStatus(piddata, periode);
        }
        public static DataTable EditJurnalDT(string p_nomorHID)
        {
            return repository.EditJurnalDT(p_nomorHID);
        }
        public static void HapusJurnal(string p_nomorHID)
        {
            repository.HapusJurnal(p_nomorHID);
        }

        public static void JurnalRE(string p_iddata, string p_periode, string p_userid)
        {
            repository.JurnalRE(p_iddata, p_periode, p_userid);
        }
        public static int ImportJurnalGlobal(string piddata, int p_bulan, int p_tahun, string periode)
        {
            return repository.ImportJurnalGlobal(piddata, p_bulan, p_tahun, periode);
        }
        public static int ImportJurnalParsial(string piddata, int p_bulan, int p_tahun, string periode)
        {
            return repository.ImportJurnalParsial(piddata, p_bulan, p_tahun, periode);
        }
        public static DataTable PeriodeList(string iddata, string ptahun)
        {
            return repository.PeriodeList(iddata, ptahun);
        }
        public static DataTable CekJurnal_KODENULL()
        {
            return repository.CekJurnal_KODENULL();
        }
        public static int CekPeriodeExist(string piddata, string p_periode)
        {
            return repository.CekPeriodeExist(piddata, p_periode);
        }
        public static int CekRecordJurnalExist(string piddata, string periode)
        {
            return repository.CekRecordJurnalExist(piddata, periode);
        }
        public static void SaveUsingOracleBulkCopy(string destTableName, DataTable dt)
        {
            repository.SaveUsingOracleBulkCopy(destTableName, dt);
        }


        public static void ImportCOAOracleBulkCopy(string destTableName, DataTable dt, string jenisakunting)
        {
            repository.ImportCOAOracleBulkCopy(destTableName, dt, jenisakunting);
        }
        public static IQueryable<JurnalHeaderDTO> GetJurnalHeader_Dapper(string p_iddata, string p_periode)
        {
            return repository.GetJurnalHeader_Dapper(p_iddata, p_periode);
        }
        public static IQueryable<JurnalDetailDTO> GetJurnalDetails_Dapper(string p_iddata, string p_periode)
        {
            return repository.GetJurnalDetails_Dapper(p_iddata, p_periode);
        }
        public static IEnumerable<JurnalDetailDTO> SearchJurnal(string p_iddata, int p_daritahunbulan, int p_sampaitahunbulan, string p_nojurnal, string p_tanggal, string p_kode, string p_keterangan, decimal p_jumlah)
        {
            return repository.SearchJurnal(p_iddata, p_daritahunbulan, p_sampaitahunbulan, p_nojurnal, p_tanggal, p_kode, p_keterangan, p_jumlah);
        }
        public static IEnumerable<JurnalDetailDTO> SearchJurnal_Bulan(string p_iddata, String p_periode, string p_nojurnal, string p_tanggal, string p_kode, string p_keterangan, decimal p_jumlah)
        {
            return repository.SearchJurnal_Bulan(p_iddata, p_periode, p_nojurnal, p_tanggal, p_kode, p_keterangan, p_jumlah);
        }

        internal static bool InsertJurnalDetail(BindingList<JurnalDetailAdd> inputJurnalDetail)
        {
            return repository.InsertJurnalDetail(inputJurnalDetail);
        }

        public static bool CekjURNALRJE(double p_jurnalID)
        {
            return repository.CekjURNALRJE(p_jurnalID);
        }
    }
}
