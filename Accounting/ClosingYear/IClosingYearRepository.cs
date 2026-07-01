namespace Accounting.ClosingYear;

public interface IClosingYearRepository
{
    ClosingYearResult CloseYear(ClosingYearRequest request);
}
