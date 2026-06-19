using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Accounting._1.Interface
{
    public interface IJurnalEntryRepository
    {
        bool CekNoJurnalExist_input(string pIdData, string pNomor, string periode);
        bool CekNoJurnalExistExceptJurnalId(string pIdData, string pNomor, string periode, double exceptJurnalId);
        bool CekjURNALRJE(double pJurnalId);
        string GetLockStatus(string pIdData, string pPeriode);
        JurnalPersistResult InsertJurnalMasterDetail(JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail);
        JurnalPersistResult UpdateJurnalMasterDetail(double oldJurnalId, JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail, DateTime? expectedHeaderVersionUtc);
        JurnalPersistResult HapusJurnal(double pJurnalId);
        void HapusJurnalRange(List<double> selectedValues);
        string CekSumber_Jurnal(double pJurnalId);
        void UpdateStatusJurnal_AIS_DELETED(string pNomor, string pPeriode);
        void HapusDataTableTmp();
    }

    public interface IJurnalQueryRepository
    {
        DataTable CekAkunMaster(int pTahun);
        DataTable CekDuplikasiJurnal();
        DataTable CekJurnal_KODENULL();
        DataTable CekNoJurnalExist();
        int CekPeriodeExist(string pIdData, string pPeriode);
        int CekPeriodeExist(string pIdData, int pBulan, int pTahun);
        IQueryable<JurnalHeaderDTO> GetJurnalHeader_Dapper(string pIdData, string pPeriode);
        IQueryable<JurnalDetailDTO> GetJurnalDetails_DapperAsQueryable(string pIdData, int pTahun, int pDariBulan, int pSampaiBulan);
        IEnumerable<JurnalDetailDTO> SearchJurnal(string pIdData, int pDariTahunBulan, int pSampaiTahunBulan, string pNoJurnal, string pTanggal, string pKode, string pKeterangan, decimal pJumlah);
        IEnumerable<JurnalDetailDTO> SearchJurnal_Bulan(string pIdData, string pPeriode, string pNoJurnal, string pTanggal, string pKode, string pKeterangan, decimal pJumlah);
        DataTable PeriodeList(string pIdData, string pTahun);
        DataTable PeriodeListAll(string pIdData);
        List<DTOCOAAktif> KodeUntukJurnal(string pIdData, int pTahun);
        IEnumerable<JurnalDetailDTO> GetJurnalLengkap(List<JurnalDetailReffID> reffIds);
        DataTable GetKasirKode(string pIdData);
        DataTable GetINVKode(string pIdData);
        int MaxTahunCOA(string pIdData);

        Task<DataTable> PeriodeListAllAsync(string pIdData);
        Task<int> MaxTahunCOAAsync(string pIdData);
        Task<List<DTOCOAAktif>> KodeUntukJurnalAsync(string pIdData, int pTahun);
        Task<DataTable> PeriodeListAsync(string pIdData, string pTahun);
        List<JurnalAuditSummary> SearchAuditTrail(string iddata, DateTime fromDate, DateTime toDate, string actionType, string userId, string nojurnal);
        List<JurnalAuditLog> GetAuditByJurnal(string nojurnal, string periode, string iddata, DateTime fromDate, DateTime toDate);
        List<JurnalAuditDetailDTO> GetAuditDetail(double auditId);
    }

    public interface IJurnalImportRepository
    {
        int ImportJurnalParsial(string pIdData, int pBulan, int pTahun, string pPeriode);
        void CreateNextPeriode(string pIdData, int pBulan, int pTahun);
        void RekalkulasiByJurnalID(string pIdData, int pBulan, int pTahun, double pJurnalId, string pPeriode, string pUserId);
        void RekalkulasiSaldo(string pIdData, int pBulan, int pTahun, string pUserId);
        DataTable JurnalKasirDetail_DapperKasir(DateTime pDari, DateTime pSampai, string pIdData, string pEstate, string pPosted, string pPeriode, string pUserId, int pGlYear, int pGlMonth);
        IEnumerable<JurnalKasirHeaderDTO> GetJurnalHeader_Kasir(int pPeriodeInt, string pEstate, string pIdData);
        DataTable AIS_Jurnal_Detail_ALL_HARIAN(DateTime tanggalJurnal, int pPeriode, string pPeriodeStr, string pPtLokasi, string pEstate, int pRemise, string pIdData);
        IEnumerable<JurnalInventoryHeaderDTO> GetJurnalHeader_Inventory(int pPeriodeInt, string pPtLokasi);
        IEnumerable<JurnalInventoryHeaderDTO> GetJurnalHeader_InventoryBaru(int pPeriodeInt, string pPtLokasi, string? pSourceFilter = null);
        DataTable Jurnal_Inventori(int pPeriodeInt, string pPtLokasi, string pIdData, string pPosted, string pPeriodeStr, string pUserId, int pGlYear, int pGlMonth);
        DataTable Jurnal_InventoriBaru(int pPeriodeInt, string pPtLokasi, string pIdData, string pPosted, string pPeriodeStr, string pUserId, int pGlYear, int pGlMonth, string? pSourceFilter = null);
        double CEK_TOTAL_TRANSAKSI(int pPeriodeInt, string pPtLokasi, string pModule);
        Task<List<Estate>> GetEstateAsync(string pIdData);
        Task<List<Division>> GetDivisionsAsync(string pIdData, string pEstateId, int pPeriode, int pRemise);
        Task<List<AIS_JURNAL>> GetAISforJurnalAsync(string pIdData, string pEstateId, int pPeriode, int pRemise, int pTahun, string pPeriodeKet);
        Task<List<JurnalKomponen>> GetAISforJurnalKomponenAsync(string pIdData, string pEstateId, int pPeriode, int pRemise);
        Task<List<SlipGaji_DTO>> viewDaftarGajidanTunjangan_BulananAsync(string pIdData, string pEstateId, int pPeriode);
        Task<List<FIN_POTONGAN_KANTOR>> viewPotonganKantorAsync(string pIdData, string pEstateId, int pPeriode);
        Task<List<ALOKASI_JURNAL_DTO>> AlokasiJurnalAsync(string pIdData);
        Task<List<AIS_JURNAL_FINAL>> HitungLampiranKASAsync(List<SlipGaji_DTO> slipGajiList, List<FIN_POTONGAN_KANTOR> potonganKantor, List<ALOKASI_JURNAL_DTO> alokasiJurnal, IEnumerable<DTOCOAAktif> listCoaAktif, string pPeriodeKet, string pPeriodeStr, string pEstate, DateTime tanggalJurnal);
        Task<List<AIS_JURNAL_FINAL>> GetPayrollforJurnalAsync(string pIdData, int pPeriode, int pRemise, int pTahun);
        Task<List<BPJS_INFO_DTO>> GetBPJSUmumAsync(string pIdData, int pPeriode, int pRemise);
    }

    public interface IJurnalExportRepository
    {
        void SaveUsingOracleBulkCopy(string destTableName, DataTable dataTable);
    }

    public interface IJurnalDomainRepository : IJurnalEntryRepository, IJurnalQueryRepository, IJurnalImportRepository, IJurnalExportRepository
    {
    }
}
