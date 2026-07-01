namespace Accounting.ClosingYear;

public sealed record ClosingYearResult(
    ClosingYearStatus Status,
    int Year,
    int NextYear,
    string Period,
    string CoaAction,
    decimal LabaRugi,
    decimal Selisih,
    string Message)
{
    public static ClosingYearResult Success(int year, int nextYear, string period, string coaAction, decimal labaRugi)
    {
        return new ClosingYearResult(
            ClosingYearStatus.Success,
            year,
            nextYear,
            period,
            coaAction,
            labaRugi,
            0m,
            string.Empty);
    }

    public static ClosingYearResult LockedPeriod(int year, string period)
    {
        return new ClosingYearResult(
            ClosingYearStatus.LockedPeriod,
            year,
            year + 1,
            period,
            string.Empty,
            0m,
            0m,
            $"Periode Akuntansi : {period} Telah Dikunci.");
    }

    public static ClosingYearResult NotBalanced(int year, string period, decimal selisih)
    {
        return new ClosingYearResult(
            ClosingYearStatus.NotBalanced,
            year,
            year + 1,
            period,
            string.Empty,
            0m,
            selisih,
            $"Neraca periode {period} belum balance.");
    }
}
