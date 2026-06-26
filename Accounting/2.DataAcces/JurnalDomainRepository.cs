using Accounting._1.Interface;
using Accounting.BusinessLayer;
using Accounting.Model;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Accounting._2.DataAcces
{
    public sealed class JurnalDomainRepository : IJurnalDomainRepository
    {
        private readonly IJurnalimport jurnalImportRepository;

        public JurnalDomainRepository()
        {
            jurnalImportRepository = new JurnalimportService();
        }

        public DataTable CekAkunMaster(int pTahun)
        {
            return JurnalServices.CekAkunMaster(pTahun);
        }

        public DataTable CekDuplikasiJurnal()
        {
            return JurnalServices.CekDuplikasiJurnal();
        }

        public DataTable CekJurnal_KODENULL()
        {
            return JurnalServices.CekJurnal_KODENULL();
        }

        public DataTable CekNoJurnalExist()
        {
            return JurnalServices.CekNoJurnalExist();
        }

        public bool CekNoJurnalExist_input(string pIdData, string pNomor, string periode)
        {
            return JurnalServices.CekNoJurnalExist_input(pIdData, pNomor, periode);
        }

        public bool CekNoJurnalExistExceptJurnalId(string pIdData, string pNomor, string periode, double exceptJurnalId)
        {
            return JurnalServices.CekNoJurnalExistExceptJurnalId(pIdData, pNomor, periode, exceptJurnalId);
        }

        public bool CekjURNALRJE(double pJurnalId)
        {
            return JurnalServices.CekjURNALRJE(pJurnalId);
        }

        public int CekPeriodeExist(string pIdData, string pPeriode)
        {
            return JurnalServices.CekPeriodeExist(pIdData, pPeriode);
        }

        public int CekPeriodeExist(string pIdData, int pBulan, int pTahun)
        {
            return AccountServices.CekPeriodeExist(pIdData, pBulan, pTahun);
        }

        public IQueryable<JurnalHeaderDTO> GetJurnalHeader_Dapper(string pIdData, string pPeriode)
        {
            return JurnalServices.GetJurnalHeader_Dapper(pIdData, pPeriode);
        }

        public IQueryable<JurnalDetailDTO> GetJurnalDetails_DapperAsQueryable(string pIdData, int pTahun, int pDariBulan, int pSampaiBulan)
        {
            return JurnalServices.GetJurnalDetails_DapperAsQueryable(pIdData, pTahun, pDariBulan, pSampaiBulan);
        }

        public IEnumerable<JurnalDetailDTO> SearchJurnal(string pIdData, int pDariTahunBulan, int pSampaiTahunBulan, string pNoJurnal, string pTanggal, string pKode, string pKeterangan, decimal pJumlah)
        {
            return JurnalServices.SearchJurnal(pIdData, pDariTahunBulan, pSampaiTahunBulan, pNoJurnal, pTanggal, pKode, pKeterangan, pJumlah);
        }

        public IEnumerable<JurnalDetailDTO> SearchJurnal_Bulan(string pIdData, string pPeriode, string pNoJurnal, string pTanggal, string pKode, string pKeterangan, decimal pJumlah)
        {
            return JurnalServices.SearchJurnal_Bulan(pIdData, pPeriode, pNoJurnal, pTanggal, pKode, pKeterangan, pJumlah);
        }

        public string GetLockStatus(string pIdData, string pPeriode)
        {
            return JurnalServices.GetLockStatus(pIdData, pPeriode);
        }

        public DataTable PeriodeList(string pIdData, string pTahun)
        {
            return JurnalServices.PeriodeList(pIdData, pTahun);
        }

        public DataTable PeriodeListAll(string pIdData)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            using OracleCommand command = new("select tahun,periode from acct_periode where iddata=:piddata order by tahun,bulan,periode desc ", connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();
            command.Parameters.Add(":piddata", OracleDbType.Varchar2, 40).Value = pIdData;
            using OracleDataReader reader = command.ExecuteReader();
            DataTable dataTable = new();
            dataTable.Load(reader);
            return dataTable;
        }

        public async Task<DataTable> PeriodeListAllAsync(string pIdData)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            using OracleCommand command = new("select tahun,periode from acct_periode where iddata=:piddata order by tahun,bulan,periode desc ", connection)
            {
                CommandType = CommandType.Text
            };

            await connection.OpenAsync();
            command.Parameters.Add(":piddata", OracleDbType.Varchar2, 40).Value = pIdData;
            using var reader = await command.ExecuteReaderAsync();
            DataTable dataTable = new();
            dataTable.Load(reader);
            return dataTable;
        }

        public Task<int> MaxTahunCOAAsync(string pIdData)
        {
            return Task.Run(() => MaxTahunCOA(pIdData));
        }

        public Task<List<DTOCOAAktif>> KodeUntukJurnalAsync(string pIdData, int pTahun)
        {
            return Task.Run(() => KodeUntukJurnal(pIdData, pTahun));
        }

        public Task<DataTable> PeriodeListAsync(string pIdData, string pTahun)
        {
            return Task.Run(() => PeriodeList(pIdData, pTahun));
        }

        public JurnalPersistResult InsertJurnalMasterDetail(JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail)
        {
            return JurnalServices.InsertJurnalMasterDetail(jurnalHeader, jurnalDetail);
        }

        public JurnalPersistResult UpdateJurnalMasterDetail(double oldJurnalId, JurnalHeaderAdd jurnalHeader, List<JurnalDetailAdd> jurnalDetail, DateTime? expectedHeaderVersionUtc)
        {
            return JurnalServices.UpdateJurnalMasterDetail(oldJurnalId, jurnalHeader, jurnalDetail, expectedHeaderVersionUtc);
        }

        public JurnalPersistResult HapusJurnal(double pJurnalId)
        {
            return JurnalServices.HapusJurnal(pJurnalId);
        }

        public void HapusJurnalRange(List<double> selectedValues)
        {
            JurnalServices.HapusJurnalRange(selectedValues);
        }

        public void SaveUsingOracleBulkCopy(string destTableName, DataTable dataTable)
        {
            JurnalServices.SaveUsingOracleBulkCopy(destTableName, dataTable);
        }

        public List<DTOCOAAktif> KodeUntukJurnal(string pIdData, int pTahun)
        {
            return JurnalServices.KodeUntukJurnal(pIdData, pTahun);
        }

        public int ImportJurnalParsial(string pIdData, int pBulan, int pTahun, string pPeriode)
        {
            return JurnalServices.ImportJurnalParsial(pIdData, pBulan, pTahun, pPeriode);
        }

        public void CreateNextPeriode(string pIdData, int pBulan, int pTahun)
        {
            AccountServices.CreateNextPeriode(pIdData, pBulan, pTahun);
        }

        public int MaxTahunCOA(string pIdData)
        {
            return AccountServices.MaxTahunCOA(pIdData);
        }

        public void RekalkulasiByJurnalID(string pIdData, int pBulan, int pTahun, double pJurnalId, string pPeriode, string pUserId)
        {
            AccountServices.RekalkulasiByJurnalID(pIdData, pBulan, pTahun, pJurnalId, pPeriode, pUserId);
        }

        public void RekalkulasiSaldo(string pIdData, int pBulan, int pTahun, string pUserId)
        {
            AccountServices.RekalkulasiSaldo(pIdData, pBulan, pTahun, pUserId);
        }

        public string CekSumber_Jurnal(double pJurnalId)
        {
            return JurnalFromModuleServices.CekSumber_Jurnal(pJurnalId);
        }

        public DataTable JurnalKasirDetail_DapperKasir(DateTime pDari, DateTime pSampai, string pIdData, string pEstate, string pPosted, string pPeriode, string pUserId, int pGlYear, int pGlMonth)
        {
            return JurnalFromModuleServices.JurnalKasirDetail_DapperKasir(pDari, pSampai, pIdData, pEstate, pPosted, pPeriode, pUserId, pGlYear, pGlMonth);
        }

        public IEnumerable<JurnalKasirHeaderDTO> GetJurnalHeader_Kasir(int pPeriodeInt, string pEstate, string pIdData)
        {
            return JurnalFromModuleServices.GetJurnalHeader_Kasir(pPeriodeInt, pEstate, pIdData);
        }

        public DataTable AIS_Jurnal_Detail_ALL_HARIAN(DateTime tanggalJurnal, int pPeriode, string pPeriodeStr, string pPtLokasi, string pEstate, int pRemise, string pIdData)
        {
            return JurnalFromModuleServices.AIS_Jurnal_Detail_ALL_HARIAN(tanggalJurnal, pPeriode, pPeriodeStr, pPtLokasi, pEstate, pRemise, pIdData);
        }

        public IEnumerable<JurnalInventoryHeaderDTO> GetJurnalHeader_Inventory(int pPeriodeInt, string pPtLokasi)
        {
            return JurnalFromModuleServices.GetJurnalHeader_Inventory(pPeriodeInt, pPtLokasi);
        }

        public IEnumerable<JurnalInventoryHeaderDTO> GetJurnalHeader_InventoryBaru(int pPeriodeInt, string pPtLokasi, string? pSourceFilter = null)
        {
            return JurnalFromModuleServices.GetJurnalHeader_InventoryBaru(pPeriodeInt, pPtLokasi, pSourceFilter);
        }

        public DataTable Jurnal_Inventori(int pPeriodeInt, string pPtLokasi, string pIdData, string pPosted, string pPeriodeStr, string pUserId, int pGlYear, int pGlMonth)
        {
            return JurnalFromModuleServices.Jurnal_Inventori(pPeriodeInt, pPtLokasi, pIdData, pPosted, pPeriodeStr, pUserId, pGlYear, pGlMonth);
        }

        public DataTable Jurnal_InventoriBaru(int pPeriodeInt, string pPtLokasi, string pIdData, string pPosted, string pPeriodeStr, string pUserId, int pGlYear, int pGlMonth, string? pSourceFilter = null)
        {
            return JurnalFromModuleServices.Jurnal_InventoriBaru(pPeriodeInt, pPtLokasi, pIdData, pPosted, pPeriodeStr, pUserId, pGlYear, pGlMonth, pSourceFilter);
        }

        public decimal CEK_TOTAL_TRANSAKSI(int pPeriodeInt, string pPtLokasi, string pModule)
        {
            return JurnalFromModuleServices.CEK_TOTAL_TRANSAKSI(pPeriodeInt, pPtLokasi, pModule);
        }

        public DataTable GetKasirKode(string pIdData)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            using OracleCommand command = new("SELECT ESTATEID, NAMA FROM MASTER_ESTATE WHERE IDDATA =:p_iddata", connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();
            command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = pIdData;
            using OracleDataReader reader = command.ExecuteReader();
            DataTable dataTable = new();
            dataTable.Load(reader);
            return dataTable;
        }

        public DataTable GetINVKode(string pIdData)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            using OracleCommand command = new("select DISTINCT PTLOKASI, INV  FROM ACCT_CONV_PTLOKASI WHERE IDDATA =:p_iddata AND INV IS NOT NULL", connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();
            command.Parameters.Add(":p_iddata", OracleDbType.Varchar2, 20).Value = pIdData;
            using OracleDataReader reader = command.ExecuteReader();
            DataTable dataTable = new();
            dataTable.Load(reader);
            return dataTable;
        }

        public IEnumerable<JurnalDetailDTO> GetJurnalLengkap(List<JurnalDetailReffID> reffIds)
        {
            const string sql = "SELECT REFFID,NOJURNAL,TANGGAL,BARIS,KODE,REKENING,DEBET,KREDIT,KETERANGAN,Posted,Periode FROM ACCT_JURNAL_DTL WHERE REFFID IN :p_reffid order by periode,nojurnal,baris";
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            connection.Open();
            return connection.Query<JurnalDetailDTO>(sql, param: new { p_reffid = reffIds.Select(detail => detail.REFFID) });
        }

        public void HapusDataTableTmp()
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            using OracleCommand command = new("TRUNCATE TABLE ACCT_JURNAL_TMP", connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void UpdateStatusJurnal_AIS_DELETED(string pNomor, string pPeriode)
        {
            using OracleConnection connection = new(LoginInfo.OracleConnString);
            using OracleCommand command = new("UPDATE BKM_JURNAL@DATABASE_LINK SET JURNALSTATUS='T' WHERE NOJURNAL=:p_NOMOR AND PERIODE_ACCT=:P_PERIODE", connection)
            {
                CommandType = CommandType.Text
            };

            connection.Open();
            command.Parameters.Add(":p_NOMOR", OracleDbType.Varchar2, 30).Value = pNomor;
            command.Parameters.Add(":P_PERIODE", OracleDbType.Varchar2, 7).Value = pPeriode;
            command.ExecuteNonQuery();
        }

        public Task<List<Estate>> GetEstateAsync(string pIdData)
        {
            return jurnalImportRepository.GetEstateAsync(pIdData);
        }

        public Task<List<Division>> GetDivisionsAsync(string pIdData, string pEstateId, int pPeriode, int pRemise)
        {
            return jurnalImportRepository.GetDivisionsAsync(pIdData, pEstateId, pPeriode, pRemise);
        }

        public Task<List<AIS_JURNAL>> GetAISforJurnalAsync(string pIdData, string pEstateId, int pPeriode, int pRemise, int pTahun, string pPeriodeKet)
        {
            return jurnalImportRepository.GetAISforJurnalAsync(pIdData, pEstateId, pPeriode, pRemise, pTahun, pPeriodeKet);
        }

        public Task<List<JurnalKomponen>> GetAISforJurnalKomponenAsync(string pIdData, string pEstateId, int pPeriode, int pRemise)
        {
            return jurnalImportRepository.GetAISforJurnalKomponenAsync(pIdData, pEstateId, pPeriode, pRemise);
        }

        public Task<List<SlipGaji_DTO>> viewDaftarGajidanTunjangan_BulananAsync(string pIdData, string pEstateId, int pPeriode)
        {
            return jurnalImportRepository.viewDaftarGajidanTunjangan_BulananAsync(pIdData, pEstateId, pPeriode);
        }

        public Task<List<FIN_POTONGAN_KANTOR>> viewPotonganKantorAsync(string pIdData, string pEstateId, int pPeriode)
        {
            return jurnalImportRepository.viewPotonganKantorAsync(pIdData, pEstateId, pPeriode);
        }

        public Task<List<ALOKASI_JURNAL_DTO>> AlokasiJurnalAsync(string pIdData)
        {
            return jurnalImportRepository.AlokasiJurnalAsync(pIdData);
        }

        public Task<List<AIS_JURNAL_FINAL>> HitungLampiranKASAsync(List<SlipGaji_DTO> slipGajiList, List<FIN_POTONGAN_KANTOR> potonganKantor, List<ALOKASI_JURNAL_DTO> alokasiJurnal, IEnumerable<DTOCOAAktif> listCoaAktif, string pPeriodeKet, string pPeriodeStr, string pEstate, DateTime tanggalJurnal)
        {
            return jurnalImportRepository.HitungLampiranKASAsync(slipGajiList, potonganKantor, alokasiJurnal, listCoaAktif, pPeriodeKet, pPeriodeStr, pEstate, tanggalJurnal);
        }

        public Task<List<AIS_JURNAL_FINAL>> GetPayrollforJurnalAsync(string pIdData, int pPeriode, int pRemise, int pTahun)
        {
            return jurnalImportRepository.GetPayrollforJurnalAsync(pIdData, pPeriode, pRemise, pTahun);
        }

        public Task<List<BPJS_INFO_DTO>> GetBPJSUmumAsync(string pIdData, int pPeriode, int pRemise)
        {
            return jurnalImportRepository.GetBPJSUmumAsync(pIdData, pPeriode, pRemise);
        }

        public List<JurnalAuditSummary> SearchAuditTrail(string iddata, DateTime fromDate, DateTime toDate, string actionType, string userId, string nojurnal)
        {
            return JurnalServices.SearchAuditTrail(iddata, fromDate, toDate, actionType, userId, nojurnal);
        }

        public List<JurnalAuditLog> GetAuditByJurnal(string nojurnal, string periode, string iddata, DateTime fromDate, DateTime toDate)
        {
            return JurnalServices.GetAuditByJurnal(nojurnal, periode, iddata, fromDate, toDate);
        }

        public List<JurnalAuditDetailDTO> GetAuditDetail(double auditId)
        {
            return JurnalServices.GetAuditDetail(auditId);
        }
    }
}
