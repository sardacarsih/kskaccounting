using System;

namespace Accounting.FixedAssets.Domain.Entities;

public sealed class DepreciationRunHeader
{
    public long RunId { get; init; }
    public string IdData { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public string RunNo { get; init; } = string.Empty;
    public WorkflowStatus Status { get; init; }
    public DateTime CreatedDate { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
}
