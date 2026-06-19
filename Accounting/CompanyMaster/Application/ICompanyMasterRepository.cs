using System.Collections.Generic;
using Accounting.CompanyMaster.Domain;

namespace Accounting.CompanyMaster.Application;

public interface ICompanyMasterRepository
{
    IReadOnlyList<CompanyMasterRecord> GetCompanies();
    IReadOnlyList<IdDataRecord> GetIdDataRows();
    IReadOnlyList<CompanyGroupOption> GetGroups();
    bool CompanyExists(string idPt);
    bool IdDataExists(string idData);
    void InsertCompany(CompanyMasterRecord company);
    void UpdateCompany(CompanyMasterRecord company);
    void DeleteCompany(string idPt);
    void InsertIdData(IdDataRecord idData);
    void UpdateIdData(IdDataRecord idData);
    void DeleteIdData(string idData);
    int CountCompanyDependencies(string idPt);
    int CountIdDataDependencies(string idData);
}
