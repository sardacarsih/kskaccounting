using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Domain.Entities;

namespace Accounting.FixedAssets.Application.Contracts;

public interface IFixedAssetJournalGateway
{
    Task<DepreciationJournalPostResponse> PostDepreciationAsync(
        string idData,
        AccountingPeriod period,
        long runId,
        string userId,
        IReadOnlyList<DepreciationPreviewLine> lines,
        CancellationToken cancellationToken);
}

public sealed class DepreciationJournalPostResponse
{
    public string NoJurnal { get; init; } = string.Empty;
    public double? JurnalId { get; init; }
    public int PostedLineCount { get; init; }
    public bool IsAlreadyPosted { get; init; }
}
