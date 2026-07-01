namespace Accounting.ClosingYear;

public sealed record ClosingYearRequest(
    string IdData,
    int Year,
    string UserId,
    string JenisAkunting,
    bool CreateClosingJournal);
