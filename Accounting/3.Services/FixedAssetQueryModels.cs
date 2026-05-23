using System;

namespace Accounting.BusinessLayer;

public sealed class FixedAssetMasterSaveRequest
{
    public long AssetId { get; init; }
    public string IdData { get; init; } = string.Empty;
    public string AssetCode { get; init; } = string.Empty;
    public string AssetName { get; init; } = string.Empty;
    public long CategoryId { get; init; }
    public long? GroupId { get; init; }
    public DateTime AcquisitionDate { get; init; }
    public DateTime? InServiceDate { get; init; }
    public DateTime? DepreciationStartDate { get; init; }
    public decimal AcquisitionCost { get; init; }
    public decimal ResidualValue { get; init; }
    public int UsefulLifeMonths { get; init; }
    public string DepreciationMethod { get; init; } = "SL";
    public string Status { get; init; } = "ACTIVE";
    public string DepartmentId { get; init; } = string.Empty;
    public string CostCenterId { get; init; } = string.Empty;
    public string LocationId { get; init; } = string.Empty;
    public string VendorId { get; init; } = string.Empty;
    public string SerialNo { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = "IDR";
    public decimal ExchangeRate { get; init; } = 1m;
    public string Notes { get; init; } = string.Empty;
}
