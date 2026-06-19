using System;
using System.Collections.Generic;
using Accounting.CompanyMaster.Domain;

namespace Accounting.CompanyMaster.Application;

public sealed class CompanyMasterAppService
{
    private readonly ICompanyMasterRepository _repository;

    public CompanyMasterAppService(ICompanyMasterRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<CompanyMasterRecord> GetCompanies() => _repository.GetCompanies();
    public IReadOnlyList<IdDataRecord> GetIdDataRows() => _repository.GetIdDataRows();
    public IReadOnlyList<CompanyGroupOption> GetGroups() => _repository.GetGroups();

    public CompanyMasterResult SaveCompany(CompanyMasterRecord company)
    {
        CompanyMasterRecord normalized = NormalizeCompany(company);
        IReadOnlyList<CompanyMasterValidationIssue> issues = ValidateCompany(normalized);
        if (issues.Count > 0)
        {
            return CompanyMasterResult.ValidationFailed(issues);
        }

        bool exists = _repository.CompanyExists(normalized.IDPT);
        if (exists)
        {
            _repository.UpdateCompany(normalized);
            return CompanyMasterResult.Success($"{normalized.NAMAPT} diperbarui");
        }

        _repository.InsertCompany(normalized);
        return CompanyMasterResult.Success($"{normalized.NAMAPT} disimpan");
    }

    public CompanyMasterResult DeleteCompany(string idPt, string companyName)
    {
        string normalizedIdPt = NormalizeCode(idPt);
        if (string.IsNullOrWhiteSpace(normalizedIdPt))
        {
            return CompanyMasterResult.Failed("Pilih perusahaan yang akan dihapus.");
        }

        int dependencies = _repository.CountCompanyDependencies(normalizedIdPt);
        if (dependencies > 0)
        {
            return CompanyMasterResult.Failed($"Perusahaan tidak dapat dihapus karena masih memiliki {dependencies} data terkait.");
        }

        _repository.DeleteCompany(normalizedIdPt);
        string displayName = string.IsNullOrWhiteSpace(companyName) ? normalizedIdPt : companyName.Trim();
        return CompanyMasterResult.Success($"Nama Perusahaan {displayName} Deleted");
    }

    public CompanyMasterResult SaveIdData(IdDataRecord idData)
    {
        IdDataRecord normalized = NormalizeIdData(idData);
        IReadOnlyList<CompanyMasterValidationIssue> issues = ValidateIdData(normalized);
        if (issues.Count > 0)
        {
            return CompanyMasterResult.ValidationFailed(issues);
        }

        bool exists = _repository.IdDataExists(normalized.IDDATA);
        if (exists)
        {
            _repository.UpdateIdData(normalized);
            return CompanyMasterResult.Success($"{normalized.IDDATA} diperbarui");
        }

        _repository.InsertIdData(normalized);
        return CompanyMasterResult.Success($"{normalized.IDDATA} diSimpan");
    }

    public CompanyMasterResult DeleteIdData(string idData)
    {
        string normalizedIdData = NormalizeCode(idData);
        if (string.IsNullOrWhiteSpace(normalizedIdData))
        {
            return CompanyMasterResult.Failed("Pilih IDDATA yang akan dihapus.");
        }

        int dependencies = _repository.CountIdDataDependencies(normalizedIdData);
        if (dependencies > 0)
        {
            return CompanyMasterResult.Failed($"IDDATA tidak dapat dihapus karena masih memiliki {dependencies} data terkait.");
        }

        _repository.DeleteIdData(normalizedIdData);
        return CompanyMasterResult.Success($"IDDATA {normalizedIdData} Deleted");
    }

    private static CompanyMasterRecord NormalizeCompany(CompanyMasterRecord company)
    {
        return new CompanyMasterRecord
        {
            IDPT = NormalizeCode(company.IDPT),
            NAMAPT = NormalizeText(company.NAMAPT).ToUpperInvariant(),
            IDGROUP = NormalizeText(company.IDGROUP).ToUpperInvariant()
        };
    }

    private static IdDataRecord NormalizeIdData(IdDataRecord idData)
    {
        return new IdDataRecord
        {
            IDDATA = NormalizeCode(idData.IDDATA),
            IDPT = NormalizeCode(idData.IDPT),
            NAMAPT = NormalizeText(idData.NAMAPT).ToUpperInvariant(),
            WILAYAH = NormalizeText(idData.WILAYAH).ToUpperInvariant(),
            JENIS_AKUNTANSI = NormalizeText(idData.JENIS_AKUNTANSI).ToUpperInvariant()
        };
    }

    private static IReadOnlyList<CompanyMasterValidationIssue> ValidateCompany(CompanyMasterRecord company)
    {
        List<CompanyMasterValidationIssue> issues = [];
        AddRequired(issues, company.IDPT, "IDPT", "Kode perusahaan wajib diisi.");
        AddRequired(issues, company.NAMAPT, "NAMAPT", "Nama perusahaan wajib diisi.");
        AddRequired(issues, company.IDGROUP, "IDGROUP", "Group perusahaan wajib diisi.");
        AddMaxLength(issues, company.IDPT, 10, "IDPT", "Kode perusahaan maksimal 10 karakter.");
        AddMaxLength(issues, company.NAMAPT, 50, "NAMAPT", "Nama perusahaan maksimal 50 karakter.");
        AddMaxLength(issues, company.IDGROUP, 20, "IDGROUP", "Group perusahaan maksimal 20 karakter.");
        return issues;
    }

    private static IReadOnlyList<CompanyMasterValidationIssue> ValidateIdData(IdDataRecord idData)
    {
        List<CompanyMasterValidationIssue> issues = [];
        AddRequired(issues, idData.IDDATA, "IDDATA", "IDDATA wajib diisi.");
        AddRequired(issues, idData.IDPT, "IDPT", "Perusahaan wajib dipilih.");
        AddRequired(issues, idData.WILAYAH, "WILAYAH", "Wilayah wajib diisi.");
        AddRequired(issues, idData.JENIS_AKUNTANSI, "JENIS_AKUNTANSI", "Jenis pembukuan wajib dipilih.");
        AddMaxLength(issues, idData.IDDATA, 20, "IDDATA", "Kode Lokasi Perusahaan MAX 20 Karakter.");
        AddMaxLength(issues, idData.IDPT, 10, "IDPT", "Kode perusahaan maksimal 10 karakter.");
        AddMaxLength(issues, idData.WILAYAH, 50, "WILAYAH", "Wilayah maksimal 50 karakter.");
        AddMaxLength(issues, idData.JENIS_AKUNTANSI, 20, "JENIS_AKUNTANSI", "Jenis pembukuan maksimal 20 karakter.");
        return issues;
    }

    private static void AddRequired(List<CompanyMasterValidationIssue> issues, string value, string field, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(new CompanyMasterValidationIssue("REQUIRED", message, field, value));
        }
    }

    private static void AddMaxLength(List<CompanyMasterValidationIssue> issues, string value, int maxLength, string field, string message)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
        {
            issues.Add(new CompanyMasterValidationIssue("MAX_LENGTH", message, field, value));
        }
    }

    private static string NormalizeCode(string? value) => NormalizeText(value).ToUpperInvariant();
    private static string NormalizeText(string? value) => value?.Trim() ?? string.Empty;
}
