using System;

namespace Accounting.DataLayer
{
    public static class CoaGuard
    {
        public static bool IsCodeChanging(string? currentKodeAcc, string? newKodeAcc) =>
            !string.Equals(currentKodeAcc?.Trim(), newKodeAcc?.Trim(), StringComparison.Ordinal);
    }
}
