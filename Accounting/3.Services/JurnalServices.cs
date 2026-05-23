using Accounting.DataLayer;
using Accounting.Model;
using DevExpress.Office.Utils;
using DevExpress.XtraGrid.Views.Grid;
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
        public static DataTable CekAkunMasterScoped(int ptahun, string piddata, string periode, string userid)
        {
            return repository.CekAkunMasterScoped(ptahun, piddata, periode, userid);
        }
        public static DataTable CekDuplikasiJurnal()
        {
            return repository.CekDuplikasiJurnal();
        }
        public static DataTable CekNoJurnalExist()
        {
            return repository.CekNoJurnalExist();
        }
        public static DataTable CekNoJurnalExistScoped(string piddata, string periode, string userid)
        {
            return repository.CekNoJurnalExistScoped(piddata, periode, userid);
        }
        public static bool ValidateColumnNames(DataTable dataTable, string[] NamaKolom)
        {
            return repository.ValidateColumnNames(dataTable,NamaKolom);
        }
       

        public static bool CekNoJurnalExist_input(string piddata, string p_nomor, string periode)
        {
            return repository.CekNoJurnalExist_input(piddata, p_nomor, periode);
        }

        public static bool CekNoJurnalExistExceptJurnalId(string piddata, string p_nomor, string periode, double exceptJurnalId)
        {
            return repository.CekNoJurnalExistExceptJurnalId(piddata, p_nomor, periode, exceptJurnalId);
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
        public static int ImportJurnalParsialScoped(string piddata, int p_bulan, int p_tahun, string periode, string userid)
        {
            return repository.ImportJurnalParsialScoped(piddata, p_bulan, p_tahun, periode, userid);
        }
        public static DataTable PeriodeList(string iddata, string ptahun)
        {
            return repository.PeriodeList(iddata, ptahun);
        }
        public static DataTable CekJurnal_KODENULL()
        {
            return repository.CekJurnal_KODENULL();
        }
        public static DataTable CekJurnal_KODENULL_Scoped(string piddata, string periode, string userid)
        {
            return repository.CekJurnal_KODENULL_Scoped(piddata, periode, userid);
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
        public static void DeleteJurnalTmpByScope(string piddata, string periode, string userid)
        {
            repository.DeleteJurnalTmpByScope(piddata, periode, userid);
        }


        public static void ImportCOAOracleBulkCopy(string destTableName, DataTable dt, string jenisakunting)
        {
            repository.ImportCOAOracleBulkCopy(destTableName, dt, jenisakunting);
        }
        public static IQueryable<JurnalHeaderDTO> GetJurnalHeader_Dapper(string p_iddata, string p_periode)
        {
            return repository.GetJurnalHeader_Dapper(p_iddata, p_periode);
        }
        public static IQueryable<JurnalDetailDTO> GetJurnalDetails_DapperAsQueryable(string p_iddata, int P_TAHUN, int P_DARIBULAN, int P_SAMPAIBULAN)
        {
            return repository.GetJurnalDetails_DapperAsQueryable( p_iddata,  P_TAHUN,  P_DARIBULAN,  P_SAMPAIBULAN);
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

        public static JurnalPersistResult InsertJurnalMasterDetail(JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail)
        {
              return repository.InsertJurnalMasterDetail(jurnalHeader, jurnalDetail);
        }
        public static JurnalPersistResult UpdateJurnalMasterDetail(double oldJurnalId, JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail, DateTime? expectedHeaderVersionUtc)
        {
              return repository.UpdateJurnalMasterDetail(oldJurnalId, jurnalHeader, jurnalDetail, expectedHeaderVersionUtc);
        }
        public static JurnalPersistResult HapusJurnal(double p_JurnalID)
        {
              return repository.HapusJurnal(p_JurnalID);
        }
        public static void HapusJurnalRange(List<double> selectedValues)
        {
            repository.HapusJurnalRange(selectedValues);
        }
        public static void PerformDragAndDrop(GridView targetGrid, GridView sourceGrid, DevExpress.Utils.DragDrop.DragDropEventArgs e)
        {
            repository.PerformDragAndDrop(targetGrid, sourceGrid, e);
        }
        public static List<DTOCOAAktif> KodeUntukJurnal(string piddata, int ptahun)
        {
            return repository.KodeUntukJurnal(piddata, ptahun);
        }

        public static List<JurnalAuditSummary> SearchAuditTrail(string iddata, DateTime fromDate, DateTime toDate, string actionType, string userId, string nojurnal)
        {
            return repository.SearchAuditTrail(iddata, fromDate, toDate, actionType, userId, nojurnal);
        }

        public static List<JurnalAuditLog> GetAuditByJurnal(string nojurnal, string periode, string iddata, DateTime fromDate, DateTime toDate)
        {
            return repository.GetAuditByJurnal(nojurnal, periode, iddata, fromDate, toDate);
        }

        public static List<JurnalAuditDetailDTO> GetAuditDetail(double auditId)
        {
            return repository.GetAuditDetail(auditId);
        }
    }
}
