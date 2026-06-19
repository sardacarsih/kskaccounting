using System.Collections.Generic;

namespace Accounting.CompanyMaster.Domain;

public sealed class CompanyMasterResult
{
    private CompanyMasterResult(bool isSuccess, string message, IReadOnlyList<CompanyMasterValidationIssue> issues)
    {
        IsSuccess = isSuccess;
        Message = message;
        Issues = issues;
    }

    public bool IsSuccess { get; }
    public string Message { get; }
    public IReadOnlyList<CompanyMasterValidationIssue> Issues { get; }

    public static CompanyMasterResult Success(string message)
    {
        return new CompanyMasterResult(true, message, []);
    }

    public static CompanyMasterResult Failed(string message)
    {
        return new CompanyMasterResult(false, message, []);
    }

    public static CompanyMasterResult ValidationFailed(IReadOnlyList<CompanyMasterValidationIssue> issues)
    {
        string message = issues.Count == 0 ? "Validasi gagal." : issues[0].Message;
        return new CompanyMasterResult(false, message, issues);
    }
}
