using Accounting.DataLayer;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Accounting.Tests;

public sealed class InventoryJournalSqlTests
{
    [Fact]
    public void InventoryBaruDetailSql_InventoryAccount_UsesLocationItemTypeMapping()
    {
        string sql = NormalizeWhitespace(GetInventoryBaruDetailSql());

        Assert.Contains(
            "LEFT JOIN \"InvLocationItemTypes\" ILT ON ILT.\"ItemTypeId\" = II.\"ItemTypeId\" AND ILT.\"LocationId\" = IR.\"LocationId\"",
            sql);
        Assert.Contains(
            "LEFT JOIN \"InvLocationItemTypes\" ILT ON ILT.\"ItemTypeId\" = II.\"ItemTypeId\" AND ILT.\"LocationId\" = IUS.\"LocationId\"",
            sql);
        Assert.Contains("NVL(ILT.\"CreditAccountNumber\",", sql);
        Assert.Contains("ILT.\"CreditAccountNumber\" AS CREDIT_ACCOUNT", sql);
        Assert.DoesNotContain("IIT.\"CreditAccountNumber\"", sql);
        Assert.Contains("IR.\"CreditAccountNumber\" AS CREDIT_ACCOUNT", sql);
    }

    [Fact]
    public void InventoryBaruDetailSql_LkDebitAccount_UsesHistoricalCoaIdThenItemTypeFallback()
    {
        string sql = NormalizeWhitespace(GetInventoryBaruDetailSql());

        Assert.Contains(
            "NVL((SELECT MAX(COA.KODEACC) FROM ACCT_COA COA WHERE COA.ACCTCOAID = IUSI.\"ACCTCOAID\" AND COA.IDDATA = :p_iddata), IIT.\"DebitAccountNumber\") AS DEBIT_ACCOUNT",
            sql);
        Assert.Contains(
            "WHERE COA.KODEACC = NVL((SELECT MAX(C2.KODEACC) FROM ACCT_COA C2 WHERE C2.ACCTCOAID = IUSI.\"ACCTCOAID\" AND C2.IDDATA = :p_iddata), IIT.\"DebitAccountNumber\") AND COA.IDDATA = :p_iddata AND COA.TAHUN = :p_glyear",
            sql);
        Assert.DoesNotContain(
            "COA.ACCTCOAID = IUSI.\"ACCTCOAID\" AND COA.IDDATA = :p_iddata AND COA.TAHUN = :p_glyear",
            sql);
        Assert.DoesNotContain(
            "C2.ACCTCOAID = IUSI.\"ACCTCOAID\" AND C2.IDDATA = :p_iddata AND C2.TAHUN = :p_glyear",
            sql);
        Assert.DoesNotContain("UsageDebitAccountNumber", sql);
    }

    [Fact]
    public void InventoryBaruDetailSql_LkNonAgronomy_AppendsNotesAfterItemQuantityAndUnit()
    {
        string sql = NormalizeWhitespace(GetInventoryBaruDetailSql());

        Assert.Contains(
            "TO_NCHAR(LK.ITEM_NAME) || N' = ' || TO_NCHAR(LK.QUANTITY) || N' ' || TO_NCHAR(LK.UNIT_NAME) || CASE",
            sql);
        Assert.Contains(
            "WHEN LK.NOTES IS NULL OR TRIM(LK.NOTES) IS NULL THEN N'' ELSE N', ' || TO_NCHAR(LK.NOTES)",
            sql);
    }

    [Fact]
    public void InventoryBaruDetailSql_LkAgronomy_UsesNotesOnly()
    {
        string sql = NormalizeWhitespace(GetInventoryBaruDetailSql());

        Assert.Contains(
            "WHEN LK.IS_AGRONOMY = 1 THEN CAST(LK.NOTES AS NVARCHAR2(200))",
            sql);
    }

    private static string GetInventoryBaruDetailSql()
    {
        FieldInfo field = typeof(JurnalFromModule).GetField(
            "InventoryBaruDetailSql",
            BindingFlags.Static | BindingFlags.NonPublic)!;

        return Assert.IsType<string>(field.GetRawConstantValue());
    }

    private static string NormalizeWhitespace(string value)
    {
        return Regex.Replace(value, @"\s+", " ").Trim();
    }
}
