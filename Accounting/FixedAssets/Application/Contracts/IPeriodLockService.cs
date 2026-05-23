using System.Threading;
using System.Threading.Tasks;

namespace Accounting.FixedAssets.Application.Contracts;

public interface IPeriodLockService
{
    Task<bool> IsPeriodLockedAsync(string idData, string period, CancellationToken cancellationToken);
}
