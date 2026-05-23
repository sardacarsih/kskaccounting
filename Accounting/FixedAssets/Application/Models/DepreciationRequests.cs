namespace Accounting.FixedAssets.Application.Models;

public sealed class DepreciationPreviewRequest
{
    public string IdData { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty; // MM/YYYY
    public string UserId { get; init; } = string.Empty;
    public bool PersistAsDraftRun { get; init; } = true;
}

public sealed class DepreciationPostRequest
{
    public string IdData { get; init; } = string.Empty;
    public string Period { get; init; } = string.Empty; // MM/YYYY
    public long RunId { get; init; }
    public string UserId { get; init; } = string.Empty;
}
