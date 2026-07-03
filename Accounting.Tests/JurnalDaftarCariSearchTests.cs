using System.Data;
using Accounting._1.Interface;
using Accounting.BusinessLayer;
using Accounting.Model;

namespace Accounting.Tests;

/// <summary>
/// Guards the optimistic-concurrency token carried by the "Cari Jurnal" (search) grid.
///
/// <see cref="JurnalDaftarCariService.SearchMonthly"/> projects header rows from detail rows,
/// which do not carry a version token. If the projection leaves
/// <see cref="JurnalHeaderDTO.HeaderVersionUtc"/> at its default (<see cref="DateTime.MinValue"/>),
/// editing a searched journal fails <c>UpdateJurnalMasterDetail</c>'s concurrency check with a
/// spurious "Konflik Update". The service must enrich each header row with the real token from
/// <c>GetJurnalHeader_Dapper</c>.
/// </summary>
public sealed class JurnalDaftarCariSearchTests
{
    [Fact]
    public void SearchMonthly_PopulatesHeaderVersionFromHeaderQuery()
    {
        DateTime expectedVersion = new(2026, 6, 30, 10, 0, 0, 123, DateTimeKind.Unspecified);

        FakeJurnalQueryRepository repository = new()
        {
            DetailRows =
            [
                new JurnalDetailDTO { REFFID = 100, HIDREFF = "H1", NoJurnal = "034/MK", Tanggal = new DateTime(2026, 6, 30), BARIS = 1, Kode = "81.1", Debet = 100m },
                new JurnalDetailDTO { REFFID = 100, HIDREFF = "H1", NoJurnal = "034/MK", Tanggal = new DateTime(2026, 6, 30), BARIS = 2, Kode = "82.1", Kredit = 100m },
            ],
            HeaderRows =
            [
                new JurnalHeaderDTO { JURNALID = 100, HID = "H1", NoJurnal = "034/MK", Tanggal = new DateTime(2026, 6, 30), HeaderVersionUtc = expectedVersion },
            ],
        };

        MonthlySearchRequest request = new()
        {
            IdData = "IDX",
            Periode = "062026",
            Kode = "81",
        };

        MonthlySearchResult result = new JurnalDaftarCariService().SearchMonthly(repository, request);

        JurnalHeaderDTO header = Assert.Single(result.HeaderRows);
        Assert.Equal(expectedVersion, header.HeaderVersionUtc);
        Assert.NotEqual(default, header.HeaderVersionUtc);
    }

    [Fact]
    public void SearchMonthly_WhenHeaderVersionMissing_LeavesDefaultForFailOpenHandling()
    {
        // No matching header row -> token cannot be resolved. It stays at default(DateTime),
        // which the edit path treats as "no token" (skip the check) rather than a real value.
        FakeJurnalQueryRepository repository = new()
        {
            DetailRows =
            [
                new JurnalDetailDTO { REFFID = 200, HIDREFF = "H2", NoJurnal = "099/MK", Tanggal = new DateTime(2026, 6, 30), BARIS = 1, Kode = "81.1", Debet = 50m },
            ],
            HeaderRows = [],
        };

        MonthlySearchRequest request = new()
        {
            IdData = "IDX",
            Periode = "062026",
            Kode = "81",
        };

        MonthlySearchResult result = new JurnalDaftarCariService().SearchMonthly(repository, request);

        JurnalHeaderDTO header = Assert.Single(result.HeaderRows);
        Assert.Equal(default, header.HeaderVersionUtc);
    }

    private sealed class FakeJurnalQueryRepository : IJurnalQueryRepository
    {
        public List<JurnalDetailDTO> DetailRows { get; init; } = [];
        public List<JurnalHeaderDTO> HeaderRows { get; init; } = [];

        public IEnumerable<JurnalDetailDTO> SearchJurnal_Bulan(string pIdData, string pPeriode, string pNoJurnal, string pTanggal, string pKode, string pKeterangan, decimal pJumlah)
            => DetailRows;

        public IQueryable<JurnalHeaderDTO> GetJurnalHeader_Dapper(string pIdData, string pPeriode)
            => HeaderRows.AsQueryable();

        // Unused by SearchMonthly.
        public DataTable CekAkunMaster(int pTahun) => throw new NotImplementedException();
        public DataTable CekDuplikasiJurnal() => throw new NotImplementedException();
        public DataTable CekJurnal_KODENULL() => throw new NotImplementedException();
        public DataTable CekNoJurnalExist() => throw new NotImplementedException();
        public int CekPeriodeExist(string pIdData, string pPeriode) => throw new NotImplementedException();
        public int CekPeriodeExist(string pIdData, int pBulan, int pTahun) => throw new NotImplementedException();
        public IQueryable<JurnalDetailDTO> GetJurnalDetails_DapperAsQueryable(string pIdData, int pTahun, int pDariBulan, int pSampaiBulan) => throw new NotImplementedException();
        public IEnumerable<JurnalDetailDTO> SearchJurnal(string pIdData, int pDariTahunBulan, int pSampaiTahunBulan, string pNoJurnal, string pTanggal, string pKode, string pKeterangan, decimal pJumlah) => throw new NotImplementedException();
        public DataTable PeriodeList(string pIdData, string pTahun) => throw new NotImplementedException();
        public DataTable PeriodeListAll(string pIdData) => throw new NotImplementedException();
        public List<DTOCOAAktif> KodeUntukJurnal(string pIdData, int pTahun) => throw new NotImplementedException();
        public IEnumerable<JurnalDetailDTO> GetJurnalLengkap(List<JurnalDetailReffID> reffIds) => throw new NotImplementedException();
        public DataTable GetKasirKode(string pIdData) => throw new NotImplementedException();
        public DataTable GetINVKode(string pIdData) => throw new NotImplementedException();
        public int MaxTahunCOA(string pIdData) => throw new NotImplementedException();
        public Task<DataTable> PeriodeListAllAsync(string pIdData) => throw new NotImplementedException();
        public Task<int> MaxTahunCOAAsync(string pIdData) => throw new NotImplementedException();
        public Task<List<DTOCOAAktif>> KodeUntukJurnalAsync(string pIdData, int pTahun) => throw new NotImplementedException();
        public Task<DataTable> PeriodeListAsync(string pIdData, string pTahun) => throw new NotImplementedException();
        public List<JurnalAuditSummary> SearchAuditTrail(string iddata, DateTime fromDate, DateTime toDate, string actionType, string userId, string nojurnal) => throw new NotImplementedException();
        public List<JurnalAuditLog> GetAuditByJurnal(string nojurnal, string periode, string iddata, DateTime fromDate, DateTime toDate) => throw new NotImplementedException();
        public List<JurnalAuditDetailDTO> GetAuditDetail(double auditId) => throw new NotImplementedException();
    }
}
