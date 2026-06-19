namespace Accounting.JurnalImport.Domain;

public sealed record JurnalImportValidationIssue(
    string Code,
    string Message,
    string? FieldName = null,
    string? Value = null);
