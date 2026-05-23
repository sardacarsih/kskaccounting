using System;
using Accounting.FixedAssets.Domain;

namespace Accounting.FixedAssets.Application.Models;

public sealed class FixedAssetTransactionCreateRequest
{
    public string IdData { get; init; } = string.Empty;
    public long AssetId { get; init; }
    public FixedAssetTransactionType TransactionType { get; init; }
    public DateTime DocumentDate { get; init; }
    public string Period { get; init; } = string.Empty;
    public decimal AmountBase { get; init; }
    public decimal? OldAmountBase { get; init; }
    public decimal? NewAmountBase { get; init; }
    public string CurrencyCode { get; init; } = "IDR";
    public decimal ExchangeRate { get; init; } = 1m;
    public string SourceReferenceNo { get; init; } = string.Empty;
    public string Remarks { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
}

public sealed class ApprovalActionRequest
{
    public string IdData { get; init; } = string.Empty;
    public long TransactionId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string RoleCode { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
}

public sealed class LifecyclePostingActionRequest
{
    public string IdData { get; init; } = string.Empty;
    public long TransactionId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string Comment { get; init; } = string.Empty;
}

public sealed class LifecyclePostingActionResult
{
    public long TransactionId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string NoJurnal { get; init; } = string.Empty;
    public double? JurnalId { get; init; }
}
