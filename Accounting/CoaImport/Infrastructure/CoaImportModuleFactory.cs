using System;
using Accounting.CoaImport.Application;
using Accounting.CoaImport.Domain;
using Accounting.CoaImport.Infrastructure.Excel;
using Accounting.CoaImport.Infrastructure.Oracle;
using Accounting.CoaImport.Presentation;

namespace Accounting.CoaImport.Infrastructure;

public static class CoaImportModuleFactory
{
    public static FrmImportCoaViewModel CreateViewModel(
        string connectionString,
        string idData,
        string userId,
        string accountingKind,
        int year)
    {
        ExcelCoaImportWorkbookReader reader = new();
        OracleCoaImportRepository repository = new(connectionString);
        ImportCoaAppService appService = new(repository);
        FrmImportCoaViewModel viewModel = new(
            new PreviewCoaImportUseCase(reader),
            new ExecuteCoaImportUseCase(appService),
            idData,
            userId,
            ToImportKind(accountingKind))
        {
            Year = year
        };

        return viewModel;
    }

    private static CoaImportKind ToImportKind(string accountingKind)
    {
        return string.Equals(accountingKind, "KEBUN", StringComparison.OrdinalIgnoreCase)
            ? CoaImportKind.Kebun
            : CoaImportKind.Pusat;
    }
}
