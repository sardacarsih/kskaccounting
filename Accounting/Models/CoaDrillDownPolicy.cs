using System;

namespace Accounting.Model
{
    public static class CoaDrillDownPolicy
    {
        public static bool OpensHierarchy(string generation)
        {
            return string.Equals(generation, "G", StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasNoDirectTransactions(string generation, decimal debet, decimal kredit)
        {
            return !OpensHierarchy(generation) && debet == 0m && kredit == 0m;
        }
    }
}
