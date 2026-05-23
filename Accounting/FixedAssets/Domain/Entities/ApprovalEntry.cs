using System;

namespace Accounting.FixedAssets.Domain.Entities;

public sealed class ApprovalEntry
{
    public long ApprovalId { get; init; }
    public long TransactionId { get; init; }
    public int StepNo { get; init; }
    public string RoleCode { get; init; } = string.Empty;
    public WorkflowStatus Status { get; init; }
    public string? ActionBy { get; init; }
    public DateTime? ActionDate { get; init; }
    public string? Comment { get; init; }
}
