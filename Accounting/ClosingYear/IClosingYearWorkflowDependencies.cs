namespace Accounting.ClosingYear;

public interface IClosingYearWorkflowDependencies
{
    bool IsCoaMissing(string idData, int year);

    bool HasCoaValidationErrors(string idData, int year);

    ClosingYearResult CloseYear(ClosingYearRequest request);

    int RefreshMaxYear(string idData);
}
