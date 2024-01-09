using Accounting.DataLayer;
using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.BusinessLayer
{
    public static class JurnalFromModuleServices
    {
        static IJurnalFromModule repository;

        
        static JurnalFromModuleServices()
        {
            repository = new JurnalFromModule();
        }

        public static IQueryable<JurnalInventoryHeaderDTO> GetJurnalHeader_Inventory(int p_periode_int, string p_ptlokasi)
        {
            return repository.GetJurnalHeader_Inventory(p_periode_int, p_ptlokasi);
        }
        public static IQueryable<JurnalKasirHeaderDTO> GetJurnalHeader_Kasir(int p_periode_int, string p_ptlokasi)
        {
            return repository.GetJurnalHeader_Kasir( p_periode_int,  p_ptlokasi);
        }
        public static IQueryable<JurnalInventoryDetailDTO> GetJurnalDetails_Inventory(int p_periode_int, string p_ptlokasi, string p_iddata, string p_posted, string p_periode_str, string p_userid, int p_glyear, int p_glmonth)
        {
            return repository.GetJurnalDetails_Inventory(p_periode_int, p_ptlokasi, p_iddata, p_posted, p_periode_str, p_userid, p_glyear, p_glmonth);
        }
        public static DataTable JurnalKasirDetail_DapperKasir(DateTime p_dari, DateTime p_sampai, string p_ptlokasi, string p_iddata, string p_estate, string p_posted, string p_periode, string p_userid, int p_glyear, int p_glmonth)
        {
            return repository.JurnalKasirDetail_DapperKasir( p_dari,  p_sampai, p_ptlokasi,p_iddata,  p_estate,  p_posted,  p_periode,  p_userid,  p_glyear,  p_glmonth);
        }
        public static DataTable AIS_Jurnal_Header(int p_periode, string p_ptlokasi, string p_estate,int p_remise)
        {
            return repository.AIS_Jurnal_Header(  p_periode,   p_ptlokasi,   p_estate, p_remise);
        }
        public static DataTable AIS_Jurnal_Detail_BOR(string p_NOBUKTI, DateTime TanggalJurnal, int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata, string p_divisi)
        {
            return repository.AIS_Jurnal_Detail_BOR(p_NOBUKTI, TanggalJurnal, p_periode, p_periode_str, p_ptlokasi, p_estate,p_remise, p_iddata, p_divisi);
        }
        public static DataTable AIS_Jurnal_Detail_HARIAN(string p_NOBUKTI, DateTime TanggalJurnal, int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata, string p_divisi)
        {
            return repository.AIS_Jurnal_Detail_HARIAN(p_NOBUKTI, TanggalJurnal, p_periode, p_periode_str, p_ptlokasi, p_estate, p_remise, p_iddata, p_divisi);
        }
        public static DataTable AIS_Jurnal_Detail_ALL_BORONGAN(DateTime TanggalJurnal, int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata)
        {
            return repository.AIS_Jurnal_Detail_ALL_BORONGAN(TanggalJurnal, p_periode, p_periode_str, p_ptlokasi, p_estate, p_remise, p_iddata);
        }
        public static DataTable AIS_Jurnal_Detail_ALL_HARIAN(DateTime TanggalJurnal, int p_periode, string p_periode_str, string p_ptlokasi, string p_estate, int p_remise, string p_iddata)
        {
            return repository.AIS_Jurnal_Detail_ALL_HARIAN(TanggalJurnal, p_periode, p_periode_str, p_ptlokasi, p_estate, p_remise, p_iddata);
        }
        public static DataTable Jurnal_Inventori(int p_periode_int, string p_ptlokasi, string p_iddata, string p_posted, string p_periode_str, string p_userid, int p_glyear, int p_glmonth)
        {
            return repository.Jurnal_Inventori(  p_periode_int,   p_ptlokasi,   p_iddata,   p_posted,   p_periode_str,   p_userid,   p_glyear,   p_glmonth);
        }
        public static void InserJurnal_FromKasir(List<JurnalKasirDetailDTO> JurnalFromKasir)
        {
            repository.InserJurnal_FromKasir( JurnalFromKasir);
        }
        public static double CEK_TOTAL_TRANSAKSI(int p_periode, string p_ptlokasi, string p_module)
        {
            return repository.CEK_TOTAL_TRANSAKSI(  p_periode,   p_ptlokasi,   p_module);
        }
        public static string CekSumber_Jurnal(double p_jurnalid)
        {
            return repository.CekSumber_Jurnal(p_jurnalid);
        }

        
        
    }
}
