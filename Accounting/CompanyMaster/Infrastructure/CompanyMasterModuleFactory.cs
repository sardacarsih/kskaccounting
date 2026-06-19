using Accounting.CompanyMaster.Application;
using Accounting.CompanyMaster.Infrastructure.Oracle;
using Accounting.CompanyMaster.Presentation;

namespace Accounting.CompanyMaster.Infrastructure;

public static class CompanyMasterModuleFactory
{
    public static FrmCompanyViewModel CreateViewModel(string connectionString)
    {
        OracleCompanyMasterRepository repository = new(connectionString);
        CompanyMasterAppService appService = new(repository);
        return new FrmCompanyViewModel(appService);
    }
}
