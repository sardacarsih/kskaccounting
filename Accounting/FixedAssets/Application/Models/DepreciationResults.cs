using System.Collections.Generic;
using Accounting.FixedAssets.Domain.Entities;

namespace Accounting.FixedAssets.Application.Models;

public sealed class DepreciationPreviewResult
{
    public long? RunId { get; init; }
    public string Period { get; init; } = string.Empty;
    public IReadOnlyList<DepreciationPreviewLine> Lines { get; init; } = System.Array.Empty<DepreciationPreviewLine>();
    public decimal TotalAmount { get; init; }
}

public sealed class DepreciationPostResult
{
    public long RunId { get; init; }
    public string NoJurnal { get; init; } = string.Empty;
    public double? JurnalId { get; init; }
    public int PostedLineCount { get; init; }
    public decimal TotalAmount { get; init; }
}
