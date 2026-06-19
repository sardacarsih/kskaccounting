using Accounting.CompanyMaster.Application;
using Accounting.CompanyMaster.Domain;
using Accounting.CompanyMaster.Presentation;

namespace Accounting.Tests;

public sealed class CompanyMasterTests
{
    [Fact]
    public void Load_PopulatesCompaniesIdDataAndGroups()
    {
        FakeCompanyMasterRepository repository = new();
        FrmCompanyViewModel viewModel = CreateViewModel(repository);

        viewModel.Load();

        Assert.Single(viewModel.Companies);
        Assert.Single(viewModel.IdDataRows);
        Assert.Single(viewModel.Groups);
    }

    [Fact]
    public void SelectCompany_PopulatesCompanyFields()
    {
        FrmCompanyViewModel viewModel = CreateViewModel(new FakeCompanyMasterRepository());

        viewModel.SelectCompany(new CompanyMasterRecord { IDPT = "MSL", NAMAPT = "PT. MITRA SAUDARA LESTARI", IDGROUP = "KSKG" });

        Assert.Equal("MSL", viewModel.CompanyIDPT);
        Assert.Equal("PT. MITRA SAUDARA LESTARI", viewModel.CompanyNAMAPT);
        Assert.Equal("KSKG", viewModel.CompanyIDGROUP);
    }

    [Fact]
    public void SelectIdData_PopulatesIdDataFields()
    {
        FrmCompanyViewModel viewModel = CreateViewModel(new FakeCompanyMasterRepository());

        viewModel.SelectIdData(new IdDataRecord { IDDATA = "MSLKEBUN", IDPT = "MSL", WILAYAH = "SUI BULUH", JENIS_AKUNTANSI = "KEBUN" });

        Assert.Equal("MSLKEBUN", viewModel.IdDataIDDATA);
        Assert.Equal("MSL", viewModel.IdDataIDPT);
        Assert.Equal("SUI BULUH", viewModel.IdDataWILAYAH);
        Assert.Equal("KEBUN", viewModel.IdDataJENIS_AKUNTANSI);
    }

    [Fact]
    public void SaveCompany_WhenNew_InsertsCompany()
    {
        FakeCompanyMasterRepository repository = new();
        FrmCompanyViewModel viewModel = CreateViewModel(repository);
        viewModel.CompanyIDPT = "new";
        viewModel.CompanyNAMAPT = "new company";
        viewModel.CompanyIDGROUP = "kskg";

        CompanyMasterResult result = viewModel.SaveCompany();

        Assert.True(result.IsSuccess);
        Assert.Contains("InsertCompany:NEW", repository.Calls);
        Assert.DoesNotContain("UpdateCompany:NEW", repository.Calls);
    }

    [Fact]
    public void SaveCompany_WhenExisting_UpdatesCompany()
    {
        FakeCompanyMasterRepository repository = new();
        FrmCompanyViewModel viewModel = CreateViewModel(repository);
        viewModel.CompanyIDPT = "MSL";
        viewModel.CompanyNAMAPT = "pt changed";
        viewModel.CompanyIDGROUP = "kskg";

        CompanyMasterResult result = viewModel.SaveCompany();

        Assert.True(result.IsSuccess);
        Assert.Contains("UpdateCompany:MSL", repository.Calls);
        Assert.DoesNotContain("InsertCompany:MSL", repository.Calls);
    }

    [Fact]
    public void SaveIdData_WhenNew_InsertsIdData()
    {
        FakeCompanyMasterRepository repository = new();
        FrmCompanyViewModel viewModel = CreateViewModel(repository);
        viewModel.IdDataIDDATA = "newdata";
        viewModel.IdDataIDPT = "MSL";
        viewModel.IdDataWILAYAH = "new wilayah";
        viewModel.IdDataJENIS_AKUNTANSI = "kebun";

        CompanyMasterResult result = viewModel.SaveIdData();

        Assert.True(result.IsSuccess);
        Assert.Contains("InsertIdData:NEWDATA", repository.Calls);
        Assert.DoesNotContain("UpdateIdData:NEWDATA", repository.Calls);
    }

    [Fact]
    public void SaveIdData_WhenExisting_UpdatesIdData()
    {
        FakeCompanyMasterRepository repository = new();
        FrmCompanyViewModel viewModel = CreateViewModel(repository);
        viewModel.IdDataIDDATA = "MSLKEBUN";
        viewModel.IdDataIDPT = "MSL";
        viewModel.IdDataWILAYAH = "changed wilayah";
        viewModel.IdDataJENIS_AKUNTANSI = "PKS";

        CompanyMasterResult result = viewModel.SaveIdData();

        Assert.True(result.IsSuccess);
        Assert.Contains("UpdateIdData:MSLKEBUN", repository.Calls);
        Assert.DoesNotContain("InsertIdData:MSLKEBUN", repository.Calls);
    }

    [Fact]
    public void SaveCompany_WhenValueTooLong_ReturnsValidationAndSkipsMutation()
    {
        FakeCompanyMasterRepository repository = new();
        FrmCompanyViewModel viewModel = CreateViewModel(repository);
        viewModel.CompanyIDPT = "TOO-LONG-CODE";
        viewModel.CompanyNAMAPT = "PT TEST";
        viewModel.CompanyIDGROUP = "KSKG";

        CompanyMasterResult result = viewModel.SaveCompany();

        Assert.False(result.IsSuccess);
        Assert.Equal("MAX_LENGTH", result.Issues[0].Code);
        Assert.DoesNotContain(repository.Calls, call => call.StartsWith("InsertCompany"));
        Assert.DoesNotContain(repository.Calls, call => call.StartsWith("UpdateCompany"));
    }

    [Fact]
    public void DeleteCompany_WhenDependenciesExist_BlocksDelete()
    {
        FakeCompanyMasterRepository repository = new() { CompanyDependencyCount = 2 };
        FrmCompanyViewModel viewModel = CreateViewModel(repository);
        viewModel.CompanyIDPT = "MSL";
        viewModel.CompanyNAMAPT = "PT TEST";

        CompanyMasterResult result = viewModel.DeleteCompany();

        Assert.False(result.IsSuccess);
        Assert.DoesNotContain("DeleteCompany:MSL", repository.Calls);
    }

    [Fact]
    public void DeleteIdData_WhenSuccessful_DeletesAndRefreshes()
    {
        FakeCompanyMasterRepository repository = new();
        FrmCompanyViewModel viewModel = CreateViewModel(repository);
        viewModel.Load();
        viewModel.IdDataIDDATA = "MSLKEBUN";

        CompanyMasterResult result = viewModel.DeleteIdData();

        Assert.True(result.IsSuccess);
        Assert.Contains("DeleteIdData:MSLKEBUN", repository.Calls);
        Assert.Contains("GetIdDataRows", repository.Calls);
    }

    private static FrmCompanyViewModel CreateViewModel(FakeCompanyMasterRepository repository)
    {
        return new FrmCompanyViewModel(new CompanyMasterAppService(repository));
    }

    private sealed class FakeCompanyMasterRepository : ICompanyMasterRepository
    {
        public List<string> Calls { get; } = [];
        public int CompanyDependencyCount { get; set; }
        public int IdDataDependencyCount { get; set; }

        public List<CompanyMasterRecord> Companies { get; } =
        [
            new CompanyMasterRecord { IDPT = "MSL", NAMAPT = "PT. MITRA SAUDARA LESTARI", IDGROUP = "KSKG" }
        ];

        public List<IdDataRecord> IdDataRows { get; } =
        [
            new IdDataRecord { IDDATA = "MSLKEBUN", IDPT = "MSL", NAMAPT = "PT. MITRA SAUDARA LESTARI", WILAYAH = "SUI BULUH", JENIS_AKUNTANSI = "KEBUN" }
        ];

        public List<CompanyGroupOption> Groups { get; } =
        [
            new CompanyGroupOption { NAMAGROUP = "KSKG" }
        ];

        public IReadOnlyList<CompanyMasterRecord> GetCompanies()
        {
            Calls.Add("GetCompanies");
            return Companies;
        }

        public IReadOnlyList<IdDataRecord> GetIdDataRows()
        {
            Calls.Add("GetIdDataRows");
            return IdDataRows;
        }

        public IReadOnlyList<CompanyGroupOption> GetGroups()
        {
            Calls.Add("GetGroups");
            return Groups;
        }

        public bool CompanyExists(string idPt)
        {
            Calls.Add($"CompanyExists:{idPt}");
            return Companies.Any(company => company.IDPT == idPt);
        }

        public bool IdDataExists(string idData)
        {
            Calls.Add($"IdDataExists:{idData}");
            return IdDataRows.Any(row => row.IDDATA == idData);
        }

        public void InsertCompany(CompanyMasterRecord company)
        {
            Calls.Add($"InsertCompany:{company.IDPT}");
            Companies.Add(company);
        }

        public void UpdateCompany(CompanyMasterRecord company)
        {
            Calls.Add($"UpdateCompany:{company.IDPT}");
            CompanyMasterRecord existing = Companies.Single(item => item.IDPT == company.IDPT);
            existing.NAMAPT = company.NAMAPT;
            existing.IDGROUP = company.IDGROUP;
        }

        public void DeleteCompany(string idPt)
        {
            Calls.Add($"DeleteCompany:{idPt}");
            Companies.RemoveAll(company => company.IDPT == idPt);
        }

        public void InsertIdData(IdDataRecord idData)
        {
            Calls.Add($"InsertIdData:{idData.IDDATA}");
            IdDataRows.Add(idData);
        }

        public void UpdateIdData(IdDataRecord idData)
        {
            Calls.Add($"UpdateIdData:{idData.IDDATA}");
            IdDataRecord existing = IdDataRows.Single(item => item.IDDATA == idData.IDDATA);
            existing.IDPT = idData.IDPT;
            existing.WILAYAH = idData.WILAYAH;
            existing.JENIS_AKUNTANSI = idData.JENIS_AKUNTANSI;
        }

        public void DeleteIdData(string idData)
        {
            Calls.Add($"DeleteIdData:{idData}");
            IdDataRows.RemoveAll(row => row.IDDATA == idData);
        }

        public int CountCompanyDependencies(string idPt)
        {
            Calls.Add($"CountCompanyDependencies:{idPt}");
            return CompanyDependencyCount;
        }

        public int CountIdDataDependencies(string idData)
        {
            Calls.Add($"CountIdDataDependencies:{idData}");
            return IdDataDependencyCount;
        }
    }
}
