using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Domain.Entities;

namespace Accounting.FixedAssets.Application.Contracts;

public interface IFixedAssetRepository
{
    Task<IReadOnlyList<FixedAsset>> GetDepreciableAssetsAsync(string idData, AccountingPeriod period, CancellationToken cancellationToken);
    Task<long> CreateDepreciationRunAsync(string idData, AccountingPeriod period, string userId, decimal totalAmount, CancellationToken cancellationToken);
    Task SaveDepreciationRunLinesAsync(long runId, IReadOnlyList<DepreciationPreviewLine> lines, string userId, CancellationToken cancellationToken);
    Task<DepreciationRunSnapshot?> GetDepreciationRunSnapshotAsync(long runId, string idData, CancellationToken cancellationToken);
    Task<IReadOnlyList<DepreciationPreviewLine>> GetDepreciationRunLinesAsync(long runId, string idData, CancellationToken cancellationToken);
    Task MarkDepreciationRunPostedAsync(long runId, string noJurnal, double? jurnalId, string userId, CancellationToken cancellationToken);
}
