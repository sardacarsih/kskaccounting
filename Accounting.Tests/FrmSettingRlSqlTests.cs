using Accounting.DataLayer;
using Accounting.Form;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Accounting.Tests;

public sealed class FrmSettingRlSqlTests
{
    [Fact]
    public void AddRootMapping_UsesValuesWithScalarDisplayOrderSubquery()
    {
        string sql = NormalizeWhitespace(GetAddRootMappingSql());

        Assert.Contains("VALUES ( :sectionId, :jenisAkunting, :iddata, :kodeAcc, (SELECT", sql);
        Assert.Contains(
            "SELECT NVL(MAX(DISPLAY_ORDER), 0) + 10 FROM ACCT_REPORT_SECTION_ACCOUNT WHERE SECTION_ID = :sectionId",
            sql);
        Assert.DoesNotContain("SELECT :sectionId", sql);
    }

    [Theory]
    [InlineData(null, 10)]
    [InlineData(30, 40)]
    public void AddRootMapping_DisplayOrderFormulaUsesTenAsInitialValueAndStep(
        int? currentMaximum,
        int expected)
    {
        Match formula = Regex.Match(
            GetAddRootMappingSql(),
            @"NVL\s*\(\s*MAX\s*\(\s*DISPLAY_ORDER\s*\)\s*,\s*(?<fallback>\d+)\s*\)\s*\+\s*(?<step>\d+)",
            RegexOptions.IgnoreCase);

        Assert.True(formula.Success, "DISPLAY_ORDER scalar formula was not found.");

        int fallback = int.Parse(formula.Groups["fallback"].Value);
        int step = int.Parse(formula.Groups["step"].Value);
        int actual = (currentMaximum ?? fallback) + step;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MappingQuery_LoadsOnlyMappingDataWithoutPerRootValidation()
    {
        string sql = NormalizeWhitespace(ReportSettingRepository.MappingQuery);

        Assert.Contains("WHERE account.SECTION_ID = :sectionId", sql);
        Assert.Contains("coa.KODEACC = account.KODEACC_ROOT", sql);
        Assert.DoesNotContain("CONNECT BY", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("LVL_STATUS", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AVAILABLE_LVLS", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidationQuery_IsSingleSetBasedCommandForEveryMatchMode()
    {
        string sql = NormalizeWhitespace(ReportSettingRepository.ValidationQuery);

        Assert.StartsWith("WITH scoped_sections AS", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tree_levels AS", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("parent_levels AS", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("group_levels AS", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CONNECT BY NOCYCLE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mapping.MATCH_MODE = 'PARENT'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mapping.MATCH_MODE = 'GRP_LVL'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LISTAGG", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(';', sql);
    }

    [Fact]
    public void ReadQueries_AreParameterizedAndBoundedByCommandTimeout()
    {
        string sectionSql = NormalizeWhitespace(ReportSettingRepository.SectionQuery);
        string mappingSql = NormalizeWhitespace(ReportSettingRepository.MappingQuery);
        string validationSql = NormalizeWhitespace(ReportSettingRepository.ValidationQuery);

        Assert.Contains(":reportCode", sectionSql);
        Assert.Contains(":sectionId", mappingSql);
        Assert.Contains(":iddata", mappingSql);
        Assert.Contains(":tahun", mappingSql);
        Assert.Contains(":jenisAkunting", validationSql);
        Assert.Equal(15, ReportSettingRepository.CommandTimeoutSeconds);
    }

    private static string GetAddRootMappingSql()
    {
        FieldInfo field = typeof(FrmSettingRL).GetField(
            "AddRootMappingSql",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        return Assert.IsType<string>(field.GetRawConstantValue());
    }

    private static string NormalizeWhitespace(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }
}
