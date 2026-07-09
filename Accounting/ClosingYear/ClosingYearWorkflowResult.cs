namespace Accounting.ClosingYear;

public sealed record ClosingYearWorkflowResult(
    ClosingYearWorkflowStatus Status,
    int Year,
    int Month,
    string Period,
    string MonthDisplay,
    ClosingYearResult? ClosingResult,
    int RefreshedMaxYear,
    string Message)
{
    public static ClosingYearWorkflowResult MissingCoa(int year, string period, string monthDisplay)
    {
        return new ClosingYearWorkflowResult(
            ClosingYearWorkflowStatus.MissingCoa,
            year,
            12,
            period,
            monthDisplay,
            null,
            0,
            "Daftar Perkiraan Belum tersedia");
    }

    public static ClosingYearWorkflowResult CoaValidationErrors(int year, string period, string monthDisplay)
    {
        return new ClosingYearWorkflowResult(
            ClosingYearWorkflowStatus.CoaValidationErrors,
            year,
            12,
            period,
            monthDisplay,
            null,
            0,
            string.Empty);
    }

    public static ClosingYearWorkflowResult Locked(ClosingYearResult result, string monthDisplay)
    {
        return FromClosingResult(ClosingYearWorkflowStatus.LockedPeriod, result, monthDisplay, 0);
    }

    public static ClosingYearWorkflowResult NotBalanced(ClosingYearResult result, string monthDisplay)
    {
        return FromClosingResult(ClosingYearWorkflowStatus.NotBalanced, result, monthDisplay, 0);
    }

    public static ClosingYearWorkflowResult Success(ClosingYearResult result, string monthDisplay, int refreshedMaxYear)
    {
        return FromClosingResult(ClosingYearWorkflowStatus.Success, result, monthDisplay, refreshedMaxYear);
    }

    private static ClosingYearWorkflowResult FromClosingResult(
        ClosingYearWorkflowStatus status,
        ClosingYearResult result,
        string monthDisplay,
        int refreshedMaxYear)
    {
        return new ClosingYearWorkflowResult(
            status,
            result.Year,
            12,
            result.Period,
            monthDisplay,
            result,
            refreshedMaxYear,
            result.Message);
    }
}
