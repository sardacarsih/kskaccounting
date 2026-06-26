using System;
using System.Collections.Generic;
using System.Linq;

namespace Accounting.JurnalImport.Domain;

public sealed class JurnalImportRecalcQueueResult
{
    private JurnalImportRecalcQueueResult(
        IReadOnlyList<long> jobIds,
        IReadOnlyList<string> impactedAccountCodes,
        int impactedRowCount)
    {
        JobIds = jobIds;
        ImpactedAccountCodes = impactedAccountCodes;
        ImpactedRowCount = impactedRowCount;
    }

    public IReadOnlyList<long> JobIds { get; }
    public IReadOnlyList<string> ImpactedAccountCodes { get; }
    public int ImpactedAccountCount => ImpactedAccountCodes.Count;
    public int ImpactedRowCount { get; }

    public static JurnalImportRecalcQueueResult Empty()
    {
        return new JurnalImportRecalcQueueResult([], [], 0);
    }

    public static JurnalImportRecalcQueueResult Create(
        IEnumerable<long> jobIds,
        IEnumerable<string> impactedAccountCodes,
        int impactedRowCount)
    {
        long[] normalizedJobIds = (jobIds ?? [])
            .Where(jobId => jobId > 0)
            .Distinct()
            .ToArray();
        string[] normalizedCodes = (impactedAccountCodes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new JurnalImportRecalcQueueResult(
            normalizedJobIds,
            normalizedCodes,
            Math.Max(0, impactedRowCount));
    }
}