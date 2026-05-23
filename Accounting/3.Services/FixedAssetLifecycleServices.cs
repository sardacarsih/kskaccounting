using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Infrastructure;

namespace Accounting.BusinessLayer;

public static class FixedAssetLifecycleServices
{
    public static Task<long> CreateTransactionAsync(FixedAssetTransactionCreateRequest request, CancellationToken cancellationToken = default)
    {
        var service = FixedAssetModuleFactory.CreateLifecycleService(LoginInfo.OracleConnString);
        return service.CreateTransactionAsync(request, cancellationToken);
    }

    public static Task ApproveAsync(ApprovalActionRequest request, CancellationToken cancellationToken = default)
    {
        var service = FixedAssetModuleFactory.CreateLifecycleService(LoginInfo.OracleConnString);
        return service.ApproveAsync(request, cancellationToken);
    }

    public static Task RejectAsync(ApprovalActionRequest request, CancellationToken cancellationToken = default)
    {
        var service = FixedAssetModuleFactory.CreateLifecycleService(LoginInfo.OracleConnString);
        return service.RejectAsync(request, cancellationToken);
    }

    public static Task<LifecyclePostingActionResult> PostApprovedAsync(LifecyclePostingActionRequest request, CancellationToken cancellationToken = default)
    {
        var service = FixedAssetModuleFactory.CreateLifecycleService(LoginInfo.OracleConnString);
        return service.PostApprovedAsync(request, cancellationToken);
    }

    public static Task ReversePostedAsync(LifecyclePostingActionRequest request, CancellationToken cancellationToken = default)
    {
        var service = FixedAssetModuleFactory.CreateLifecycleService(LoginInfo.OracleConnString);
        return service.ReversePostedAsync(request, cancellationToken);
    }
}
