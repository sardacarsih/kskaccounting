using System;
using System.Collections.Generic;
using System.Linq;

namespace Accounting.Form
{
    internal sealed class JurnalRekalkulasiQueuedEventArgs : EventArgs
    {
        public long JobId { get; init; }
        public string IdData { get; init; } = string.Empty;
        public string Periode { get; init; } = string.Empty;
        public IReadOnlyList<string> ImpactedAccountCodes { get; init; } = Array.Empty<string>();
    }

    internal static class JurnalRekalkulasiNotifier
    {
        public static event EventHandler<JurnalRekalkulasiQueuedEventArgs>? JobQueued;

        public static void Publish(long jobId, string idData, string periode, IReadOnlyList<string>? impactedAccountCodes)
        {
            if (jobId <= 0)
            {
                return;
            }

            string[] normalizedCodes = (impactedAccountCodes ?? Array.Empty<string>())
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            JobQueued?.Invoke(
                null,
                new JurnalRekalkulasiQueuedEventArgs
                {
                    JobId = jobId,
                    IdData = idData ?? string.Empty,
                    Periode = periode ?? string.Empty,
                    ImpactedAccountCodes = normalizedCodes
                });
        }
    }
}
