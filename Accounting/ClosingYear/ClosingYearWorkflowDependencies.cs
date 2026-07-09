using Accounting.BusinessLayer;

namespace Accounting.ClosingYear;

internal sealed class ClosingYearWorkflowDependencies : IClosingYearWorkflowDependencies
{
    public bool IsCoaMissing(string idData, int year)
    {
        return AccountServices.CekCOAExist(idData, year) == 1;
    }

    public bool HasCoaValidationErrors(string idData, int year)
    {
        return ToolsServices.Analisa_kesalahan_COA(idData, year).Rows.Count > 0;
    }

    public ClosingYearResult CloseYear(ClosingYearRequest request)
    {
        return ClosingYearServices.CloseYear(request);
    }

    public int RefreshMaxYear(string idData)
    {
        int maxYear = AccountServices.MaxTahunCOA(idData);
        Acct.TahunMax = maxYear;

        return maxYear;
    }
}
