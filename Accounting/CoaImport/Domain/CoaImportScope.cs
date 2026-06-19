namespace Accounting.CoaImport.Domain;

public sealed record CoaImportScope(
    string IdData,
    int Year,
    string UserId,
    string BatchId,
    CoaImportKind AccountingKind);
