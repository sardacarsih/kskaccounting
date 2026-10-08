using Accounting.Model;
using System.Data;

namespace Accounting.Tests;

public sealed class ReportDrillDownRowTests
{
    [Fact]
    public void FromDataRow_MapsUnifiedDrillDownContract()
    {
        DataTable table = CreateTable();
        table.Rows.Add("10.10000.000", "Kas", "10.00000.000", "D", "D", 1250.75m, "NERACA");

        ReportDrillDownRow row = ReportDrillDownRow.FromDataRow(table.Rows[0]);

        Assert.Equal("10.10000.000", row.KodeAcc);
        Assert.Equal("Kas", row.NamaAcc);
        Assert.Equal("10.00000.000", row.ParentAcc);
        Assert.Equal("D", row.Posisi);
        Assert.Equal("D", row.IsHeader);
        Assert.Equal(1250.75m, row.Nilai);
        Assert.Equal("NERACA", row.ReportCode);
    }

    [Fact]
    public void FromDataRow_RejectsLegacyDrillDownSchema()
    {
        DataTable table = new("ReportDrillDown");
        table.Columns.Add("KODEACC", typeof(string));
        table.Columns.Add("HEADER", typeof(string));
        table.Rows.Add("10.10000.000", "D");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => ReportDrillDownRow.FromDataRow(table.Rows[0]));

        Assert.Contains("NAMAACC", exception.Message);
        Assert.Contains("NILAI", exception.Message);
        Assert.Contains("REPORT_CODE", exception.Message);
    }

    [Theory]
    [InlineData("D", "D", "4.10000.000", true)]
    [InlineData("S", "D", "4.10000.000", false)]
    [InlineData("T", "D", "4.10000.000", false)]
    [InlineData("D", "G", "4.10000.000", false)]
    [InlineData("D", "D", "", false)]
    public void CanDrillDown_RejectsAggregateAndHeaderRows(string rowKind, string isHeader, string kodeAcc, bool expected)
    {
        Assert.Equal(expected, ReportDrillDownPolicy.CanDrillDown(rowKind, isHeader, kodeAcc));
    }

    [Theory]
    [InlineData("K")]
    [InlineData("k")]
    public void GetGeneralLedgerSide_UsesReportPositionForCreditSections(string posisi)
    {
        Assert.Equal("K", ReportDrillDownPolicy.GetGeneralLedgerSide(posisi));
    }

    [Theory]
    [InlineData("D")]
    [InlineData("")]
    public void GetGeneralLedgerSide_DefaultsToDebitForNonCreditSections(string posisi)
    {
        Assert.Equal("D", ReportDrillDownPolicy.GetGeneralLedgerSide(posisi));
    }

    private static DataTable CreateTable()
    {
        DataTable table = new("ReportDrillDown");
        table.Columns.Add("KODEACC", typeof(string));
        table.Columns.Add("NAMAACC", typeof(string));
        table.Columns.Add("PARENTACC", typeof(string));
        table.Columns.Add("POSISI", typeof(string));
        table.Columns.Add("ISHEADER", typeof(string));
        table.Columns.Add("NILAI", typeof(decimal));
        table.Columns.Add("REPORT_CODE", typeof(string));
        return table;
    }
}
