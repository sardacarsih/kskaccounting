using Accounting._1.Interface;
using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.DataLayer
{
    public interface IJurnalFromModule
    {
        IEnumerable<JurnalKasirHeaderDTO> GetJurnalHeader_Kasir(int p_periode_int, string p_ptlokasi, string p_estate);
        IEnumerable<JurnalInventoryHeaderDTO> GetJurnalHeader_Inventory(int p_periode_int, string p_ptlokasi);
        DataTable JurnalKasirDetail_DapperKasir(
            DateTime p_dari,
            DateTime p_sampai,
            string p_iddata,
            string p_estate,
            string p_posted,
            string p_periode,
            string p_userid,
            int p_glyear,
            int p_glmonth);
        IEnumerable<JurnalInventoryDetailDTO> GetJurnalDetails_Inventory(
        int p_periode_int, string p_ptlokasi, string p_iddata, string p_posted,
        string p_periode_str, string p_userid, int p_glyear, int p_glmonth);
        DataTable Jurnal_Inventori(int p_periode_int, string p_ptlokasi, string p_iddata, string p_posted, string p_periode_str, string p_userid, int p_glyear, int p_glmonth);
        DataTable AIS_Jurnal_Header(int p_periode, string p_ptlokasi, string p_estate, int p_remise);
        DataTable AIS_Jurnal_Detail_BOR(string p_NOBUKTI, DateTime TanggalJurnal, int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata, string p_divisi);       
        DataTable AIS_Jurnal_Detail_HARIAN(string p_NOBUKTI,DateTime TanggalJurnal,int p_periode, string p_periode_str,string p_ptlokasi, string p_estate, int p_remise, string p_iddata, string p_divisi);
        DataTable AIS_Jurnal_Detail_ALL_BORONGAN(DateTime TanggalJurnal, int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata);
        DataTable AIS_Jurnal_Detail_ALL_HARIAN(DateTime TanggalJurnal, int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata);
        void InserJurnal_FromKasir(List<JurnalKasirDetailDTO> JurnalFromKasir);
        double CEK_TOTAL_TRANSAKSI(int p_periode, string p_ptlokasi, string p_module);

        string CekSumber_Jurnal(double p_jurnalid);
    }
}
