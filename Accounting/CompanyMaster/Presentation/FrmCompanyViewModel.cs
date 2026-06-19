using System.Collections.Generic;
using System.ComponentModel;
using Accounting.CompanyMaster.Application;
using Accounting.CompanyMaster.Domain;

namespace Accounting.CompanyMaster.Presentation;

public sealed class FrmCompanyViewModel
{
    private readonly CompanyMasterAppService _appService;

    public FrmCompanyViewModel(CompanyMasterAppService appService)
    {
        _appService = appService;
    }

    public BindingList<CompanyMasterRecord> Companies { get; private set; } = [];
    public BindingList<IdDataRecord> IdDataRows { get; private set; } = [];
    public BindingList<CompanyGroupOption> Groups { get; private set; } = [];

    public string CompanyIDPT { get; set; } = string.Empty;
    public string CompanyNAMAPT { get; set; } = string.Empty;
    public string CompanyIDGROUP { get; set; } = string.Empty;

    public string IdDataIDDATA { get; set; } = string.Empty;
    public string IdDataIDPT { get; set; } = string.Empty;
    public string IdDataWILAYAH { get; set; } = string.Empty;
    public string IdDataJENIS_AKUNTANSI { get; set; } = "KEBUN";

    public void Load()
    {
        RefreshCompanies();
        RefreshIdDataRows();
        RefreshGroups();
    }

    public void SelectCompany(CompanyMasterRecord? company)
    {
        if (company == null)
        {
            ClearCompanyFields();
            return;
        }

        CompanyIDPT = company.IDPT;
        CompanyNAMAPT = company.NAMAPT;
        CompanyIDGROUP = company.IDGROUP;
    }

    public void SelectIdData(IdDataRecord? idData)
    {
        if (idData == null)
        {
            ClearIdDataFields();
            return;
        }

        IdDataIDDATA = idData.IDDATA;
        IdDataIDPT = idData.IDPT;
        IdDataWILAYAH = idData.WILAYAH;
        IdDataJENIS_AKUNTANSI = idData.JENIS_AKUNTANSI;
    }

    public CompanyMasterResult SaveCompany()
    {
        CompanyMasterResult result = _appService.SaveCompany(new CompanyMasterRecord
        {
            IDPT = CompanyIDPT,
            NAMAPT = CompanyNAMAPT,
            IDGROUP = CompanyIDGROUP
        });

        if (result.IsSuccess)
        {
            RefreshCompanies();
        }

        return result;
    }

    public CompanyMasterResult DeleteCompany()
    {
        CompanyMasterResult result = _appService.DeleteCompany(CompanyIDPT, CompanyNAMAPT);
        if (result.IsSuccess)
        {
            RefreshCompanies();
            RefreshIdDataRows();
            ClearCompanyFields();
        }

        return result;
    }

    public CompanyMasterResult SaveIdData()
    {
        CompanyMasterResult result = _appService.SaveIdData(new IdDataRecord
        {
            IDDATA = IdDataIDDATA,
            IDPT = IdDataIDPT,
            WILAYAH = IdDataWILAYAH,
            JENIS_AKUNTANSI = IdDataJENIS_AKUNTANSI
        });

        if (result.IsSuccess)
        {
            RefreshIdDataRows();
        }

        return result;
    }

    public CompanyMasterResult DeleteIdData()
    {
        CompanyMasterResult result = _appService.DeleteIdData(IdDataIDDATA);
        if (result.IsSuccess)
        {
            RefreshIdDataRows();
            ClearIdDataFields();
        }

        return result;
    }

    private void RefreshCompanies()
    {
        Companies = ToBindingList(_appService.GetCompanies());
    }

    private void RefreshIdDataRows()
    {
        IdDataRows = ToBindingList(_appService.GetIdDataRows());
    }

    private void RefreshGroups()
    {
        Groups = ToBindingList(_appService.GetGroups());
    }

    private void ClearCompanyFields()
    {
        CompanyIDPT = string.Empty;
        CompanyNAMAPT = string.Empty;
        CompanyIDGROUP = string.Empty;
    }

    private void ClearIdDataFields()
    {
        IdDataIDDATA = string.Empty;
        IdDataIDPT = string.Empty;
        IdDataWILAYAH = string.Empty;
        IdDataJENIS_AKUNTANSI = "KEBUN";
    }

    private static BindingList<T> ToBindingList<T>(IReadOnlyList<T> items)
    {
        return new BindingList<T>(new List<T>(items));
    }
}
