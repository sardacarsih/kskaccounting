using Accounting.BusinessLayer;
using System.Data;

namespace Accounting.Tests;

public sealed class InventoryDetailSelectionTests
{
    [Theory]
    [InlineData("002/LT-INTI/VII/2026", "LT-002")]
    [InlineData("006/LT-INTI/VII/2026", "LT-006")]
    public void FilterDetailsByJournalNumber_ReturnsOnlyExactJournal(string journalNumber, string expectedCode)
    {
        DataTable source = CreateSource();

        DataTable result = JurnalImportSelectionService.FilterDetailsByJournalNumber(source, journalNumber);

        DataRow row = Assert.Single(result.AsEnumerable());
        Assert.Equal(journalNumber, row.Field<string>("NOJURNAL"));
        Assert.Equal(expectedCode, row.Field<string>("KODE"));
    }

    [Fact]
    public void FilterDetailsByJournalNumber_MatchesCaseInsensitively()
    {
        DataTable source = CreateSource();

        DataTable result = JurnalImportSelectionService.FilterDetailsByJournalNumber(
            source,
            "a.011/lk-inti/vii/2026");

        DataRow row = Assert.Single(result.AsEnumerable());
        Assert.Equal("LK-A011", row.Field<string>("KODE"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("999/LT-INTI/VII/2026")]
    public void FilterDetailsByJournalNumber_WhenNoMatch_ReturnsEmptyTableWithSourceSchema(string journalNumber)
    {
        DataTable source = CreateSource();

        DataTable result = JurnalImportSelectionService.FilterDetailsByJournalNumber(source, journalNumber);

        Assert.Empty(result.Rows.Cast<DataRow>());
        Assert.Equal(source.Columns.Count, result.Columns.Count);
        Assert.Equal(source.Columns["NOJURNAL"]!.DataType, result.Columns["NOJURNAL"]!.DataType);
        Assert.Equal(source.Columns["DEBET"]!.DataType, result.Columns["DEBET"]!.DataType);
    }

    [Fact]
    public void FilterDetailsByJournalNumber_WhenSourceIsEmpty_PreservesSchema()
    {
        DataTable source = CreateSchema();

        DataTable result = JurnalImportSelectionService.FilterDetailsByJournalNumber(
            source,
            "002/LT-INTI/VII/2026");

        Assert.Empty(result.Rows.Cast<DataRow>());
        Assert.Equal(source.Columns.Count, result.Columns.Count);
        Assert.Equal(typeof(decimal), result.Columns["DEBET"]!.DataType);
    }

    private static DataTable CreateSource()
    {
        DataTable source = CreateSchema();
        source.Rows.Add("002/LT-INTI/VII/2026", "LT-002", 563_299_033.91m);
        source.Rows.Add("A.008/LK-INTI/VII/2026", "LK-A008", 100_000m);
        source.Rows.Add("006/LT-INTI/VII/2026", "LT-006", 5_405_405.40m);
        source.Rows.Add("A.011/LK-INTI/VII/2026", "LK-A011", 749_792.79m);
        return source;
    }

    private static DataTable CreateSchema()
    {
        DataTable source = new();
        source.Columns.Add("NOJURNAL", typeof(string));
        source.Columns.Add("KODE", typeof(string));
        source.Columns.Add("DEBET", typeof(decimal));
        return source;
    }
}
