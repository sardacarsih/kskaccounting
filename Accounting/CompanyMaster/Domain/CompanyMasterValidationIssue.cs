namespace Accounting.CompanyMaster.Domain;

public sealed record CompanyMasterValidationIssue(
    string Code,
    string Message,
    string? Field = null,
    string? Value = null);
