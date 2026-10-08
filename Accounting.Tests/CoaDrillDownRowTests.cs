using Accounting.Model;
using System.Data;

namespace Accounting.Tests;

public sealed class CoaDrillDownRowTests
{
    [Fact]
    public void FromDataRow_MapsCoaHierarchyContract()
    {
        DataTable table = CreateTable();
        table.Rows.Add("10.01003.001", "Kas Kelapa Genjah Estate", "10.01003.000", "D", "D", 119227853m, 0m, 0m);

        CoaDrillDownRow row = CoaDrillDownRow.FromDataRow(table.Rows[0]);

        Assert.Equal("10.01003.001", row.KodeAcc);
        Assert.Equal("10.01003.000", row.ParentAcc);
        Assert.Equal(119227853m, row.Debet);
        Assert.Equal(0m, row.Kredit);
        Assert.Equal(0m, row.SaldoAkhir);
    }

    [Fact]
    public void FromDataRow_RejectsReportDrillDownContract()
    {
        DataTable table = new("CoaDrillDown");
        table.Columns.Add("KODEACC", typeof(string));
        table.Columns.Add("NILAI", typeof(decimal));
        table.Rows.Add("10.01003.001", 10m);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => CoaDrillDownRow.FromDataRow(table.Rows[0]));

        Assert.Contains("DEBET", exception.Message);
        Assert.Contains("KREDIT", exception.Message);
        Assert.Contains("SALDOAKHIR", exception.Message);
        Assert.Contains("Kolom diterima: KODEACC, NILAI", exception.Message);
    }

    [Fact]
    public void FromDataRow_WhenOptionalValuesAreDbNull_MapsSafeDefaults()
    {
        DataTable table = CreateTable();
        table.Rows.Add("10.01003.001", DBNull.Value, DBNull.Value, DBNull.Value, "D", DBNull.Value, DBNull.Value, DBNull.Value);

        CoaDrillDownRow row = CoaDrillDownRow.FromDataRow(table.Rows[0]);

        Assert.Equal("10.01003.001", row.KodeAcc);
        Assert.Equal(string.Empty, row.NamaAcc);
        Assert.Equal(string.Empty, row.ParentAcc);
        Assert.Equal(string.Empty, row.Posisi);
        Assert.Equal(0m, row.Debet);
        Assert.Equal(0m, row.Kredit);
        Assert.Equal(0m, row.SaldoAkhir);
    }

    [Fact]
    public void GetRequiredTable_WhenNamedTableIsMissing_ThrowsActionableError()
    {
        DataSet dataSet = new();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => CoaDrillDownRow.GetRequiredTable(dataSet));

        Assert.Contains("CoaDrillDown", exception.Message);
        Assert.Contains("Tabel diterima: (tidak ada)", exception.Message);
    }

    [Fact]
    public void EnsureRequiredColumns_WhenKodeAccIsMissing_ReportsReceivedColumns()
    {
        DataTable table = CreateTable();
        table.Columns.Remove("KODEACC");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => CoaDrillDownRow.EnsureRequiredColumns(table));

        Assert.Contains("KODEACC", exception.Message);
        Assert.Contains("tabel CoaDrillDown", exception.Message);
        Assert.Contains("Kolom diterima:", exception.Message);
        Assert.DoesNotContain("KODEACC,", exception.Message.Split("Kolom diterima:")[1]);
    }

    [Theory]
    [InlineData("G", true)]
    [InlineData("D", false)]
    public void OpensHierarchy_OnlyAllowsGroupRows(string generation, bool expected)
    {
        Assert.Equal(expected, CoaDrillDownPolicy.OpensHierarchy(generation));
    }

    [Theory]
    [InlineData("G", 0, 0, false)]
    [InlineData("D", 0, 0, true)]
    [InlineData("D", 100, 0, false)]
    public void HasNoDirectTransactions_AllowsZeroMovementGroups(string generation, decimal debet, decimal kredit, bool expected)
    {
        Assert.Equal(expected, CoaDrillDownPolicy.HasNoDirectTransactions(generation, debet, kredit));
    }

    private static DataTable CreateTable()
    {
        DataTable table = new("CoaDrillDown");
        table.Columns.Add("KODEACC", typeof(string));
        table.Columns.Add("NAMAACC", typeof(string));
        table.Columns.Add("PARENTACC", typeof(string));
        table.Columns.Add("POSISI", typeof(string));
        table.Columns.Add("ISHEADER", typeof(string));
        table.Columns.Add("DEBET", typeof(decimal));
        table.Columns.Add("KREDIT", typeof(decimal));
        table.Columns.Add("SALDOAKHIR", typeof(decimal));
        return table;
    }
}
