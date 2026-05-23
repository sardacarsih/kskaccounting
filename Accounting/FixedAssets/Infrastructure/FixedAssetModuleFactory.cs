using Accounting.FixedAssets.Application.Services;
using Accounting.FixedAssets.Infrastructure.Oracle;

namespace Accounting.FixedAssets.Infrastructure;

public static class FixedAssetModuleFactory
{
    public static FixedAssetDepreciationAppService CreateDepreciationService(string connectionString)
    {
        return new FixedAssetDepreciationAppService(
            new OracleFixedAssetRepository(connectionString),
            new OracleFixedAssetJournalGateway(connectionString),
            new OraclePeriodLockService(connectionString));
    }

    public static FixedAssetLifecycleAppService CreateLifecycleService(string connectionString)
    {
        return new FixedAssetLifecycleAppService(
            new OracleFixedAssetLifecycleRepository(connectionString),
            new OraclePeriodLockService(connectionString));
    }
}
