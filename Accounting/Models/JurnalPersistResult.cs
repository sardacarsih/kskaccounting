using System;
using System.Collections.Generic;
using System.Linq;

namespace Accounting.Model
{
    public static class JurnalRecalcScopes
    {
        public const string None = "NONE";
        public const string InsertDelta = "INSERT_DELTA";
        public const string UpdateDelta = "UPDATE_DELTA";
        public const string DeleteDelta = "DELETE_DELTA";
    }

    public sealed class JurnalRecalcHint
    {
        public bool RequiresRecalc { get; private init; }
        public string Scope { get; private init; } = JurnalRecalcScopes.None;
        public int ImpactedRowCount { get; private init; }
        public int ImpactedAccountCount { get; private init; }
        public IReadOnlyList<string> ImpactedAccountCodes { get; private init; } = Array.Empty<string>();

        public static JurnalRecalcHint None()
        {
            return new JurnalRecalcHint
            {
                RequiresRecalc = false,
                Scope = JurnalRecalcScopes.None,
                ImpactedAccountCodes = Array.Empty<string>()
            };
        }

        public static JurnalRecalcHint Create(
            string scope,
            int impactedRowCount,
            int impactedAccountCount,
            IEnumerable<string>? impactedAccountCodes = null)
        {
            string[] normalizedCodes = (impactedAccountCodes ?? Array.Empty<string>())
                .Where(static code => !string.IsNullOrWhiteSpace(code))
                .Select(static code => code.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static code => code, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new JurnalRecalcHint
            {
                RequiresRecalc = impactedRowCount > 0 || impactedAccountCount > 0 || normalizedCodes.Length > 0,
                Scope = string.IsNullOrWhiteSpace(scope) ? JurnalRecalcScopes.None : scope,
                ImpactedRowCount = Math.Max(0, impactedRowCount),
                ImpactedAccountCount = Math.Max(0, Math.Max(impactedAccountCount, normalizedCodes.Length)),
                ImpactedAccountCodes = normalizedCodes
            };
        }
    }

    public enum JurnalPersistStatus
    {
        Success = 0,
        Conflict = 1
    }

    public sealed class JurnalPersistResult
    {
        public JurnalPersistStatus Status { get; private init; }
        public double JurnalId { get; private init; }
        public JurnalRecalcHint RecalcHint { get; private init; } = JurnalRecalcHint.None();

        public bool IsSuccess => Status == JurnalPersistStatus.Success;
        public bool IsConflict => Status == JurnalPersistStatus.Conflict;

        public static JurnalPersistResult Success(double jurnalId, JurnalRecalcHint? recalcHint = null)
        {
            return new JurnalPersistResult
            {
                Status = JurnalPersistStatus.Success,
                JurnalId = jurnalId,
                RecalcHint = recalcHint ?? JurnalRecalcHint.None()
            };
        }

        public static JurnalPersistResult Conflict(double jurnalId)
        {
            return new JurnalPersistResult
            {
                Status = JurnalPersistStatus.Conflict,
                JurnalId = jurnalId,
                RecalcHint = JurnalRecalcHint.None()
            };
        }
    }
}
