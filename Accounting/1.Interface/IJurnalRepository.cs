using Accounting.Model;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.DataLayer
{
    public interface IJurnalRepository
    {
        DataSet GetNotaDebet(string piddata, string pperiode, string pkodeacc);
        DataSet GetNotaKredit(string piddata, string pperiode, string pkodeacc);
        DataTable GetJurnalHeader(string piddata, string periode);
        DataTable GetJurnalDetails(string piddata, string periode);
        DataTable GetJurnalDetailsV2(string piddata, string periode);
        IQueryable<JurnalHeaderDTO> GetJurnalHeader_Dapper(string p_iddata, string p_periode);
        IQueryable<JurnalDetailDTO> GetJurnalDetails_DapperAsQueryable(string p_iddata, int P_TAHUN, int P_DARIBULAN, int P_SAMPAIBULAN);
        IEnumerable<JurnalDetailDTO> SearchJurnal(string p_iddata, int p_daritahunbulan, int p_sampaitahunbulan,string p_nojurnal, string p_tanggal,string p_kode, string p_keterangan, decimal p_jumlah);
        IEnumerable<JurnalDetailDTO> SearchJurnal_Bulan(string p_iddata, string p_periode, string p_nojurnal, string p_tanggal, string p_kode, string p_keterangan, decimal p_jumlah);
        DataTable CekAkunMaster(int ptahun);
        DataTable CekDuplikasiJurnal();     
        DataTable CekNoJurnalExist();
        DataTable CekJurnal_KODENULL();
        bool CekNoJurnalExist_input(string piddata,string nojurnal,string periode);
        bool CekjURNALRJE(double p_jurnalID);
        int ImportJurnalGlobal(string piddata, int p_bulan, int p_tahun, string periode);
        int ImportJurnalParsial(string piddata, int p_bulan, int p_tahun, string periode);
        DataTable EditJurnalDT(string p_nomorHID);
        int CekRecordJurnalExist(string piddata, string periode);
        int CekPeriodeExist(string piddata, string p_periode);

        string GetLockStatus(string piddata, string periode);
        DataTable PeriodeList(string piddata, string ptahun);
        void HapusJurnal(string p_nomorHID);
        void SaveUsingOracleBulkCopy(string destTableName, DataTable dt);
        void ImportCOAOracleBulkCopy(string destTableName, DataTable dt,String jenisakunting);
        void JurnalRE(string p_iddata, string p_periode, string p_userid);
        bool InsertJurnalDetail(BindingList<JurnalDetailAdd> inputJurnalDetail);
        bool ValidateColumnNames(DataTable dataTable, string[] NamaKolom);

        void InsertJurnalMasterDetail(JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail);
        void UpdateJurnalMasterDetail(double oldJurnalId, JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail);
        void HapusJurnal(double p_JurnalID);
        void HapusJurnalRange(List<double> selectedValues);
        void PerformDragAndDrop(GridView targetGrid, GridView sourceGrid, DevExpress.Utils.DragDrop.DragDropEventArgs e);
        List<DTOCOAAktif> KodeUntukJurnal(string piddata, int ptahun);
    }
}
