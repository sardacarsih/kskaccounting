namespace Accounting.FixedAssets.Application.Models;

public sealed class DepreciationRunSnapshot
{
    public long RunId { get; init; }
    public string IdData { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string NoJurnal { get; init; } = string.Empty;
    public double? JurnalId { get; init; }
}
