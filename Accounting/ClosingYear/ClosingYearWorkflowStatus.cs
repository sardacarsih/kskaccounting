namespace Accounting.ClosingYear;

public enum ClosingYearWorkflowStatus
{
    MissingCoa,
    CoaValidationErrors,
    LockedPeriod,
    NotBalanced,
    Success
}
