using Accounting.JurnalImport.Application;
using Accounting.JurnalImport.Infrastructure.Excel;
using Accounting.JurnalImport.Infrastructure.Oracle;
using Accounting.JurnalImport.Presentation;

namespace Accounting.JurnalImport.Infrastructure;

public static class JurnalImportModuleFactory
{
    public static FrmImportJurnalViewModel CreateExcelViewModel(string idData, string userId)
    {
        ExcelJurnalImportWorkbookReader reader = new();
        OracleJurnalImportDataStore dataStore = new();
        return new FrmImportJurnalViewModel(
            new PreviewJurnalImportUseCase(reader),
            new ExecuteJurnalImportUseCase(dataStore),
            idData,
            userId);
    }

    public static ExecuteJurnalImportUseCase CreateExecuteUseCase()
    {
        return new ExecuteJurnalImportUseCase(new OracleJurnalImportDataStore());
    }

    public static FrmJurnalModuleImportViewModel CreateModuleViewModel(string idData, string userId)
    {
        return new FrmJurnalModuleImportViewModel(CreateExecuteUseCase(), idData, userId);
    }
}
