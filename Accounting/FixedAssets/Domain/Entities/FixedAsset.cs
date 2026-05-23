using System;

namespace Accounting.FixedAssets.Domain.Entities;

public sealed class FixedAsset
{
    public long AssetId { get; init; }
    public string IdData { get; init; } = string.Empty;
    public string AssetCode { get; init; } = string.Empty;
    public string AssetName { get; init; } = string.Empty;
    public long CategoryId { get; init; }
    public long GroupId { get; init; }
    public DateTime AcquisitionDate { get; init; }
    public DateTime? InServiceDate { get; init; }
    public DateTime? DepreciationStartDate { get; init; }
    public decimal AcquisitionCost { get; init; }
    public decimal ResidualValue { get; init; }
    public int UsefulLifeMonths { get; init; }
    public DepreciationMethod DepreciationMethod { get; init; }
    public FixedAssetStatus Status { get; init; }
    public string CurrencyCode { get; init; } = "IDR";
    public decimal ExchangeRate { get; init; } = 1m;

    public decimal TotalImprovementAmount { get; init; }
    public decimal TotalRevaluationDelta { get; init; }
    public decimal TotalImpairmentAmount { get; init; }
    public decimal AccumulatedDepreciationPosted { get; init; }

    public string DepreciationExpenseAccount { get; init; } = string.Empty;
    public string AccumulatedDepreciationAccount { get; init; } = string.Empty;

    public bool IsDepreciationStopped =>
        Status == FixedAssetStatus.Disposed ||
        Status == FixedAssetStatus.Sold ||
        Status == FixedAssetStatus.WrittenOff ||
        Status == FixedAssetStatus.Retired;

    public DateTime EffectiveDepreciationStartDate =>
        DepreciationStartDate ?? InServiceDate ?? AcquisitionDate;

    public decimal CostBasis =>
        AcquisitionCost + TotalImprovementAmount + TotalRevaluationDelta - TotalImpairmentAmount;

    public decimal OpeningNetBookValue =>
        Math.Max(ResidualValue, CostBasis - AccumulatedDepreciationPosted);
}
