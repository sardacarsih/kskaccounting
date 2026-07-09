using System;
using System.Collections.Generic;

namespace Accounting.DataLayer
{
    public sealed class CoaCascadeDeleteResult
    {
        public bool Success { get; init; }
        public IReadOnlyList<string> BlockedKodeAcc { get; init; } = Array.Empty<string>();
        public int DeletedCount { get; init; }
    }
}
