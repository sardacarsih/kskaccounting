using System;

namespace Accounting.FixedAssets.Domain.Entities;

public sealed class FixedAssetTransaction
{
    public long TransactionId { get; init; }
    public string IdData { get; init; } = string.Empty;
    public string DocumentNo { get; init; } = string.Empty;
    public FixedAssetTransactionType TransactionType { get; init; }
    public DateTime DocumentDate { get; init; }
    public string Period { get; init; } = string.Empty;
    public long? AssetId { get; init; }
    public string CurrencyCode { get; init; } = "IDR";
    public decimal ExchangeRate { get; init; } = 1m;
    public WorkflowStatus Status { get; init; } = WorkflowStatus.Draft;
    public string SourceReferenceNo { get; init; } = string.Empty;
    public string Remarks { get; init; } = string.Empty;
}
