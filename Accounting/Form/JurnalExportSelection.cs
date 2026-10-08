using Accounting.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accounting.Form
{
    internal static class JurnalExportSelection
    {
        internal static List<double> ResolveJurnalIds(IEnumerable<double?> selectedIds, double? focusedId)
        {
            List<double> resolvedIds = selectedIds
                .Where(id => id.HasValue && IsValidJurnalId(id.Value))
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            if (resolvedIds.Count == 0 && focusedId.HasValue && IsValidJurnalId(focusedId.Value))
            {
                resolvedIds.Add(focusedId.Value);
            }

            return resolvedIds;
        }

        internal static List<JurnalDetailDTO> FilterDetails(
            IEnumerable<JurnalDetailDTO> source,
            IEnumerable<double> jurnalIds)
        {
            HashSet<double> selectedIds = jurnalIds.ToHashSet();
            return FrmExportJurnal.NormalizeJurnalOrder(
                source.Where(detail => selectedIds.Contains(detail.REFFID)));
        }

        private static bool IsValidJurnalId(double jurnalId)
        {
            return jurnalId > 0 && !double.IsNaN(jurnalId) && !double.IsInfinity(jurnalId);
        }
    }
}
