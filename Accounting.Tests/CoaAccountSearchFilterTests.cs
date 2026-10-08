using Accounting.Form;
using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;
using System.ComponentModel;

namespace Accounting.Tests;

public sealed class CoaAccountSearchFilterTests
{
    [Theory]
    [InlineData(null, new string[0])]
    [InlineData("", new string[0])]
    [InlineData("11", new[] { "11" })]
    [InlineData("11,KAS", new[] { "11", "KAS" })]
    [InlineData(" 11, KAS,11, ,kas ", new[] { "11", "KAS" })]
    public void ParseAccountPrefixes_NormalizesCommaSeparatedInput(
        string? text,
        string[] expected)
    {
        Assert.Equal(expected, CoaAccountSearchFilter.ParseAccountPrefixes(text));
    }

    [Fact]
    public void CreateCriteria_EmptyInputReturnsNull()
    {
        Assert.Null(CoaAccountSearchFilter.CreateCriteria("  ,  ", "  "));
    }

    [Theory]
    [InlineData("11", new[] { "11.0100.000" })]
    [InlineData("10,12", new[] { "10.0100.000", "12.0100.000" })]
    [InlineData(" 11, 13,11 ", new[] { "11.0100.000", "13.0100.000" })]
    public void CreateAccountCriteria_FiltersPrefixes(
        string text,
        string[] expected)
    {
        CriteriaOperator criteria = Assert.IsAssignableFrom<CriteriaOperator>(
            CoaAccountSearchFilter.CreateAccountCriteria(text));

        AssertMatches(criteria, expected);
    }

    [Fact]
    public void CreateAccountCriteria_DoesNotWrapAccountPropertyInUpperFunction()
    {
        CriteriaOperator criteria = Assert.IsAssignableFrom<CriteriaOperator>(
            CoaAccountSearchFilter.CreateAccountCriteria("18"));

        string criteriaText = criteria.ToString();

        Assert.Contains("StartsWith([KODEACC]", criteriaText);
        Assert.DoesNotContain("Upper([KODEACC])", criteriaText);
    }

    [Theory]
    [InlineData("KAS", new[] { "10.0100.000", "11.0100.000" })]
    [InlineData("bank", new[] { "12.0100.000" })]
    [InlineData("usaha", new[] { "13.0100.000" })]
    public void CreateAccountNameCriteria_FiltersContainsCaseInsensitive(
        string text,
        string[] expected)
    {
        CriteriaOperator criteria = Assert.IsAssignableFrom<CriteriaOperator>(
            CoaAccountSearchFilter.CreateAccountNameCriteria(text));

        AssertMatches(criteria, expected);
    }

    [Theory]
    [InlineData("10,11", "kas", new[] { "10.0100.000", "11.0100.000" })]
    [InlineData("10,12", "bank", new[] { "12.0100.000" })]
    [InlineData("13", "kas", new string[0])]
    [InlineData("", "kas", new[] { "10.0100.000", "11.0100.000" })]
    [InlineData("12", "", new[] { "12.0100.000" })]
    public void CreateCriteria_CombinesSeparateFiltersWithAnd(
        string accountText,
        string accountNameText,
        string[] expected)
    {
        CriteriaOperator criteria = Assert.IsAssignableFrom<CriteriaOperator>(
            CoaAccountSearchFilter.CreateCriteria(accountText, accountNameText));

        AssertMatches(criteria, expected);
    }

    private static void AssertMatches(
        CriteriaOperator criteria,
        string[] expected)
    {
        IReadOnlyList<SearchRow> rows = CreateAccountRows();
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(SearchRow));
        var evaluator = new ExpressionEvaluator(properties, criteria);

        IEnumerable<string> actual = rows
            .Where(evaluator.Fit)
            .Select(row => row.KODEACC);

        Assert.Equal(expected, actual);
    }

    private static IReadOnlyList<SearchRow> CreateAccountRows()
    {
        return
        [
            new SearchRow("10.0100.000", "Kas Besar"),
            new SearchRow("11.0100.000", "kas kecil"),
            new SearchRow("12.0100.000", "Bank Mandiri"),
            new SearchRow("13.0100.000", "PIUTANG USAHA")
        ];
    }

    private sealed record SearchRow(string KODEACC, string NAMAACC);
}
