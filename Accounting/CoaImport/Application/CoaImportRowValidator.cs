using System;
using System.Collections.Generic;
using System.Linq;
using Accounting.CoaImport.Domain;

namespace Accounting.CoaImport.Application;

public static class CoaImportRowValidator
{
    public static IReadOnlyList<CoaImportValidationIssue> Validate(
        IReadOnlyList<CoaImportRow> rows,
        IReadOnlySet<string> existingAccounts)
    {
        List<CoaImportValidationIssue> issues = [];
        HashSet<string> fileAccounts = rows
            .Select(row => Normalize(row.Account))
            .Where(account => !string.IsNullOrWhiteSpace(account))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (IGrouping<string, CoaImportRow> duplicateGroup in rows
            .Where(row => !string.IsNullOrWhiteSpace(row.Account))
            .GroupBy(row => Normalize(row.Account), StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1))
        {
            issues.Add(new CoaImportValidationIssue(
                "DUPLICATE_ACCOUNT",
                "Kode Perkiraan duplikat pada file import.",
                "ACCOUNT",
                duplicateGroup.Key));
        }

        foreach (CoaImportRow row in rows)
        {
            string account = Normalize(row.Account);
            string parent = Normalize(row.Induk);

            if (!string.IsNullOrWhiteSpace(account) &&
                string.Equals(account, parent, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new CoaImportValidationIssue(
                    "SELF_PARENT",
                    "Induk Akun tidak boleh sama dengan kode akun.",
                    "INDUK",
                    account));
            }

            if (IsDetail(row) && string.IsNullOrWhiteSpace(parent))
            {
                issues.Add(new CoaImportValidationIssue(
                    "DETAIL_WITHOUT_PARENT",
                    "Kode Perkiraan Detail tidak memiliki induk.",
                    "ACCOUNT",
                    account));
                continue;
            }

            if (!string.IsNullOrWhiteSpace(parent) &&
                !fileAccounts.Contains(parent) &&
                !existingAccounts.Contains(parent))
            {
                issues.Add(new CoaImportValidationIssue(
                    "PARENT_NOT_FOUND",
                    "Induk Perkiraan tidak terdaftar.",
                    "INDUK",
                    parent));
            }
        }

        return issues;
    }

    private static bool IsDetail(CoaImportRow row)
    {
        return string.Equals(row.Gen?.Trim(), "D", StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
