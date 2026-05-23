using System;

namespace Accounting.FixedAssets.Domain.Entities;

public sealed class DepreciationPreviewLine
{
    public long AssetId { get; init; }
    public string AssetCode { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public DateTime PeriodStartDate { get; init; }
    public DateTime PeriodEndDate { get; init; }

    public decimal OpeningNetBookValue { get; init; }
    public decimal DepreciationAmount { get; init; }
    public decimal ClosingNetBookValue { get; init; }
    public decimal ResidualValue { get; init; }

    public string DepreciationExpenseAccount { get; init; } = string.Empty;
    public string AccumulatedDepreciationAccount { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
