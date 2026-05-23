using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Domain;

namespace Accounting.FixedAssets.Application.Contracts;

public interface IFixedAssetLifecycleRepository
{
    Task<string> GenerateDocumentNoAsync(string idData, string docType, AccountingPeriod period, CancellationToken cancellationToken);
    Task<long> CreateTransactionAsync(FixedAssetTransactionCreateRequest request, string documentNo, CancellationToken cancellationToken);
    Task SubmitForApprovalAsync(string idData, long transactionId, string userId, CancellationToken cancellationToken);
    Task ApproveAsync(ApprovalActionRequest request, CancellationToken cancellationToken);
    Task RejectAsync(ApprovalActionRequest request, CancellationToken cancellationToken);
    Task<LifecyclePostingActionResult> PostApprovedAsync(LifecyclePostingActionRequest request, CancellationToken cancellationToken);
    Task ReversePostedAsync(LifecyclePostingActionRequest request, CancellationToken cancellationToken);
    Task<bool> ExistsActiveAssetAsync(string idData, long assetId, CancellationToken cancellationToken);
}
