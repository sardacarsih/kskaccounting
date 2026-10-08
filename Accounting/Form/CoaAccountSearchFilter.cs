using DevExpress.Data.Filtering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Accounting.Form
{
    internal static class CoaAccountSearchFilter
    {
        internal static IReadOnlyList<string> ParseAccountPrefixes(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Array.Empty<string>();
            }

            return text
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(prefix => prefix.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        internal static CriteriaOperator? CreateCriteria(
            string? accountText,
            string? accountNameText)
        {
            CriteriaOperator? accountCriteria = CreateAccountCriteria(accountText);
            CriteriaOperator? accountNameCriteria = CreateAccountNameCriteria(accountNameText);

            if (ReferenceEquals(accountCriteria, null))
            {
                return accountNameCriteria;
            }

            if (ReferenceEquals(accountNameCriteria, null))
            {
                return accountCriteria;
            }

            return new GroupOperator(
                GroupOperatorType.And,
                accountCriteria,
                accountNameCriteria);
        }

        internal static CriteriaOperator? CreateAccountCriteria(string? text)
        {
            IReadOnlyList<string> prefixes = ParseAccountPrefixes(text);
            if (prefixes.Count == 0)
            {
                return null;
            }

            OperandProperty accountProperty = new("KODEACC");
            CriteriaOperator[] prefixCriteria = prefixes
                .Select(prefix => (CriteriaOperator)new FunctionOperator(
                    FunctionOperatorType.StartsWith,
                    accountProperty,
                    new OperandValue(prefix)))
                .ToArray();

            return prefixCriteria.Length == 1
                ? prefixCriteria[0]
                : new GroupOperator(GroupOperatorType.Or, prefixCriteria);
        }

        internal static CriteriaOperator? CreateAccountNameCriteria(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return new FunctionOperator(
                FunctionOperatorType.Contains,
                CreateUpperProperty("NAMAACC"),
                new OperandValue(text.Trim().ToUpperInvariant()));
        }

        private static FunctionOperator CreateUpperProperty(string propertyName)
        {
            return new FunctionOperator(
                FunctionOperatorType.Upper,
                new OperandProperty(propertyName));
        }
    }
}
