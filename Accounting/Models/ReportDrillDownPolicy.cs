using System;

namespace Accounting.Model
{
    public static class ReportDrillDownPolicy
    {
        public static bool CanDrillDown(string rowKind, string isHeader, string kodeAcc)
        {
            return rowKind != LabaRugiRow.SubtotalRowKind
                && rowKind != LabaRugiRow.TotalRowKind
                && !string.Equals(isHeader, "G", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(kodeAcc);
        }

        public static string GetGeneralLedgerSide(string posisi)
        {
            return string.Equals(posisi, "K", StringComparison.OrdinalIgnoreCase) ? "K" : "D";
        }
    }
}
