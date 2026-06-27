using Accounting.Model;
using Accounting.Services;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Accounting.Tests;

public sealed class NeracaReportTests
{
    [Fact]
    public void FromDataRow_WhenColumnsValid_MapsTypedProperties()
    {
        DataTable table = CreateNeracaTable();
        table.Rows.Add("01", "NERACA", "AKTIVA", "AKTIVA LANCAR", "10.00000.000", "Kas & Bank", "D", 1250.75m, 1100.50m, 900.25m);

        NeracaRow row = NeracaRow.FromDataRow(table.Rows[0]);

        Assert.Equal("01", row.Kode);
        Assert.Equal("NERACA", row.Cat1);
        Assert.Equal("AKTIVA", row.Kat);
        Assert.Equal("AKTIVA LANCAR", row.Cat2);
        Assert.Equal("10.00000.000", row.Akun);
        Assert.Equal("Kas & Bank", row.Tipe);
        Assert.Equal("D", row.Posisi);
        Assert.Equal(1250.75m, row.BulanIni);
        Assert.Equal(1100.50m, row.BulanLalu);
        Assert.Equal(900.25m, row.AwalTahun);
        Assert.Equal(string.Empty, row.ParentAkun);
    }

    [Fact]
    public void FromDataRow_WhenParentAkunColumnExists_MapsParentAkun()
    {
        DataTable table = CreateNeracaTable();
        table.Columns.Add("PARENTAKUN", typeof(string));
        table.Rows.Add("01", "NERACA", "AKTIVA", "AKTIVA LANCAR", "10.10000.000", "Kas", "D", 10m, 20m, 30m, "10.00000.000");

        NeracaRow row = NeracaRow.FromDataRow(table.Rows[0]);

        Assert.Equal("10.00000.000", row.ParentAkun);
    }

    [Fact]
    public void FromDataRow_WhenDbNull_UsesSafeDefaults()
    {
        DataTable table = CreateNeracaTable();
        table.Rows.Add(DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

        NeracaRow row = NeracaRow.FromDataRow(table.Rows[0]);

        Assert.Equal(string.Empty, row.Kode);
        Assert.Equal(string.Empty, row.Akun);
        Assert.Equal(string.Empty, row.Posisi);
        Assert.Equal(0m, row.BulanIni);
        Assert.Equal(0m, row.BulanLalu);
        Assert.Equal(0m, row.AwalTahun);
    }

    [Fact]
    public void FromDataRow_WhenRequiredColumnMissing_ThrowsClearError()
    {
        DataTable table = CreateNeracaTable();
        table.Columns.Remove("BULANINI");
        DataRow row = table.NewRow();
        table.Rows.Add(row);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => NeracaRow.FromDataRow(row));

        Assert.Contains("Neraca", ex.Message);
        Assert.Contains("BULANINI", ex.Message);
    }

    [Fact]
    public void CreateReportDataSet_BuildsNeracaTableWithExpectedColumns()
    {
        DataSet dataSet = NeracaReportDataAdapter.CreateReportDataSet([CreateRow("10.00000.000")]);

        Assert.True(dataSet.Tables.Contains("Neraca"));
        DataTable table = dataSet.Tables["Neraca"]!;
        Assert.Equal(1, table.Rows.Count);
        foreach (string column in new[] { "URUT", "KODE", "CAT1", "KAT", "CAT2", "AKUN", "PARENTAKUN", "TIPE", "POSISI", "BULANINI", "BULANLALU", "AWALTAHUN" })
        {
            Assert.True(table.Columns.Contains(column), $"missing column {column}");
        }
        Assert.Equal("10.00000.000", table.Rows[0]["AKUN"]);
        Assert.Equal(1, table.Rows[0]["URUT"]);
    }

    [Fact]
    public void CreateReportDataTable_AssignsSequentialUrut()
    {
        DataTable table = NeracaReportDataAdapter.CreateReportDataTable(
            [CreateRow("10.00000.000"), CreateRow("11.00000.000"), CreateRow("30.00000.000")]);

        Assert.Equal(3, table.Rows.Count);
        Assert.Equal(1, table.Rows[0]["URUT"]);
        Assert.Equal(2, table.Rows[1]["URUT"]);
        Assert.Equal(3, table.Rows[2]["URUT"]);
    }

    [Fact]
    public void CreateReportDataTable_WhenRowsEmpty_ReturnsEmptyValidTable()
    {
        DataTable table = NeracaReportDataAdapter.CreateReportDataTable([]);

        Assert.Equal(0, table.Rows.Count);
        Assert.True(table.Columns.Contains("URUT"));
        Assert.True(table.Columns.Contains("AWALTAHUN"));
    }

    [Fact]
    public void ReadinessResult_Ready_IsReadyWithNoFailure()
    {
        NeracaReadinessResult result = NeracaReadinessResult.Ready();

        Assert.True(result.IsReady);
        Assert.Equal(NeracaReadinessFailure.None, result.Failure);
        Assert.Equal(0m, result.Selisih);
    }

    [Fact]
    public void ReadinessResult_MissingJournal_IsNotReady()
    {
        NeracaReadinessResult result = NeracaReadinessResult.MissingJournal();

        Assert.False(result.IsReady);
        Assert.Equal(NeracaReadinessFailure.MissingJournal, result.Failure);
    }

    [Fact]
    public void ReadinessResult_NotBalanced_CarriesPeriodeAndSelisih()
    {
        NeracaReadinessResult result = NeracaReadinessResult.NotBalanced("06/2026", 1500m);

        Assert.False(result.IsReady);
        Assert.Equal(NeracaReadinessFailure.NotBalanced, result.Failure);
        Assert.Equal(1500m, result.Selisih);
        Assert.Contains("06/2026", result.Message);
    }

    private static NeracaRow CreateRow(string akun)
    {
        return new NeracaRow
        {
            Kode = "01",
            Cat1 = "NERACA",
            Kat = "AKTIVA",
            Cat2 = "AKTIVA LANCAR",
            Akun = akun,
            Tipe = "Akun " + akun,
            Posisi = "D",
            BulanIni = 100m,
            BulanLalu = 90m,
            AwalTahun = 80m
        };
    }

    private static DataTable CreateNeracaTable()
    {
        DataTable table = new("Neraca");
        table.Columns.Add("KODE", typeof(string));
        table.Columns.Add("CAT1", typeof(string));
        table.Columns.Add("KAT", typeof(string));
        table.Columns.Add("CAT2", typeof(string));
        table.Columns.Add("AKUN", typeof(string));
        table.Columns.Add("TIPE", typeof(string));
        table.Columns.Add("POSISI", typeof(string));
        table.Columns.Add("BULANINI", typeof(decimal));
        table.Columns.Add("BULANLALU", typeof(decimal));
        table.Columns.Add("AWALTAHUN", typeof(decimal));
        return table;
    }
}
