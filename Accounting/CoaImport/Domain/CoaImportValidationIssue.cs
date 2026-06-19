namespace Accounting.CoaImport.Domain;

public sealed record CoaImportValidationIssue(
    string Code,
    string Message,
    string? Field = null,
    string? Value = null);
