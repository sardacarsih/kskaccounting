namespace Accounting.JurnalImport.Domain;

public sealed record JurnalImportScope(
    string IdData,
    string UserId,
    int Month,
    int Year,
    int CoaYear,
    string Period,
    JurnalImportSource Source,
    JurnalImportMode Mode);
