using Accounting.Model;
using System.Data;

namespace Accounting.Tests;

public sealed class GeneralLedgerRowTests
{
    [Fact]
    public void FromDataRow_MapsAndRecognizesOpeningBalance()
    {
        DataTable table = CreateTable();
        DateTime openingDate = new(2026, 3, 1);
        table.Rows.Add(0, 202603, "03/2026", "01.001", "Bank", "000", openingDate, "SALDO AWAL", 1250m, 0m);

        GeneralLedgerRow row = GeneralLedgerRow.FromDataRow(table.Rows[0]);

        Assert.Equal(0, row.Baris);
        Assert.Equal(202603, row.Param);
        Assert.Equal(openingDate, row.Tanggal);
        Assert.Equal(1250m, row.Debet);
        Assert.True(row.IsOpeningBalance);
    }

    [Fact]
    public void FromDataRow_RecognizesOpeningBalancesForMultiplePeriods()
    {
        DataTable table = CreateTable();
        table.Rows.Add(0, 202601, "01/2026", "01.001", "Bank", "000", new DateTime(2026, 1, 1), "SALDO AWAL", 1000m, 0m);
        table.Rows.Add(0, 202602, "02/2026", "01.001", "Bank", "000", new DateTime(2026, 2, 1), "SALDO AWAL", 1250m, 0m);

        List<GeneralLedgerRow> rows = table.Rows
            .Cast<DataRow>()
            .Select(GeneralLedgerRow.FromDataRow)
            .ToList();

        int[] expectedPeriods = [202601, 202602];
        Assert.Equal(expectedPeriods, rows.Select(row => row.Param));
        Assert.All(rows, row => Assert.True(row.IsOpeningBalance));
        Assert.All(rows, row => Assert.Equal(0, row.Baris));
    }

    [Fact]
    public void GetRequiredTable_WithCompleteContract_ReturnsBukuBesarTable()
    {
        DataSet dataSet = new();
        DataTable table = CreateTable();
        dataSet.Tables.Add(table);

        DataTable result = GeneralLedgerRow.GetRequiredTable(dataSet);

        Assert.Same(table, result);
        Assert.Equal(GeneralLedgerRow.RequiredColumns, table.Columns.Cast<DataColumn>().Select(column => column.ColumnName));
    }

    [Fact]
    public void EnsureRequiredColumns_WhenReportMetadataIsMissing_ThrowsActionableError()
    {
        DataTable table = CreateTable();
        table.Columns.Remove("BARIS");
        table.Columns.Remove("PARAM");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => GeneralLedgerRow.EnsureRequiredColumns(table));

        Assert.Contains("BARIS", exception.Message);
        Assert.Contains("PARAM", exception.Message);
        Assert.Contains("Kolom diterima:", exception.Message);
    }

    [Fact]
    public void EnsureRequiredColumns_WhenColumnOrderChanges_RejectsExportBreakingContract()
    {
        DataTable table = CreateTable();
        table.Columns["PARAM"]!.SetOrdinal(0);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => GeneralLedgerRow.EnsureRequiredColumns(table));

        Assert.Contains("urutan atau jumlah kolom tidak sesuai", exception.Message);
        Assert.Contains("Urutan wajib: BARIS, PARAM", exception.Message);
    }

    [Fact]
    public void GetRequiredTable_WhenBukuBesarTableIsMissing_ThrowsActionableError()
    {
        DataSet dataSet = new();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => GeneralLedgerRow.GetRequiredTable(dataSet));

        Assert.Contains("BukuBesar", exception.Message);
        Assert.Contains("Tabel diterima: (tidak ada)", exception.Message);
    }

    private static DataTable CreateTable()
    {
        DataTable table = new("BukuBesar");
        table.Columns.Add("BARIS", typeof(int));
        table.Columns.Add("PARAM", typeof(int));
        table.Columns.Add("PERIODE", typeof(string));
        table.Columns.Add("KODE", typeof(string));
        table.Columns.Add("REKENING", typeof(string));
        table.Columns.Add("NOJURNAL", typeof(string));
        table.Columns.Add("TANGGAL", typeof(DateTime));
        table.Columns.Add("KETERANGAN", typeof(string));
        table.Columns.Add("DEBET", typeof(decimal));
        table.Columns.Add("KREDIT", typeof(decimal));
        return table;
    }
}
