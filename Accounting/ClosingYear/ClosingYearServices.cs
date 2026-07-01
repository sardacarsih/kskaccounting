namespace Accounting.ClosingYear;

public static class ClosingYearServices
{
    private static readonly ClosingYearService Service = new(new OracleClosingYearRepository());

    public static ClosingYearResult CloseYear(ClosingYearRequest request)
    {
        return Service.CloseYear(request);
    }
}
