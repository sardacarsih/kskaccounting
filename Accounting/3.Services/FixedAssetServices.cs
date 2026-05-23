using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Infrastructure;

namespace Accounting.BusinessLayer;

public static class FixedAssetServices
{
    public static Task<DepreciationPreviewResult> PreviewDepreciationAsync(
        string idData,
        string period,
        string userId,
        bool persistAsDraftRun = true,
        CancellationToken cancellationToken = default)
    {
        var service = FixedAssetModuleFactory.CreateDepreciationService(LoginInfo.OracleConnString);
        return service.PreviewAsync(
            new DepreciationPreviewRequest
            {
                IdData = idData,
                Period = period,
                UserId = userId,
                PersistAsDraftRun = persistAsDraftRun
            },
            cancellationToken);
    }

    public static Task<DepreciationPostResult> PostDepreciationAsync(
        string idData,
        string period,
        long runId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var service = FixedAssetModuleFactory.CreateDepreciationService(LoginInfo.OracleConnString);
        return service.PostAsync(
            new DepreciationPostRequest
            {
                IdData = idData,
                Period = period,
                RunId = runId,
                UserId = userId
            },
            cancellationToken);
    }
}
