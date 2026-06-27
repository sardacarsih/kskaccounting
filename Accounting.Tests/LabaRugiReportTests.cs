using Accounting.Model;
using Accounting.Services;
using System.Collections.Generic;
using System.Data;

namespace Accounting.Tests;

public sealed class LabaRugiReportTests
{
    [Fact]
    public void FromDataRow_WhenColumnsValid_MapsTypedProperties()
    {
        DataTable table = CreateLabaRugiTable();
        table.Rows.Add("EST1", "40.01", 7, "D", "Pendapatan", "Sub2", "Sub3", "Sub4", "Sub5", "Sub6", 1250.75m, 9800.25m, "AKRUAL", "Y", "USER1", "D", "K");

        LabaRugiRow row = LabaRugiRow.FromDataRow(table.Rows[0]);

        Assert.Equal("EST1", row.IdData);
        Assert.Equal("40.01", row.KodeAcc);
        Assert.Equal(7, row.Urut);
        Assert.Equal("D", row.TipeAcc);
        Assert.Equal("Pendapatan", row.Sub1);
        Assert.Equal(1250.75m, row.BulanIni);
        Assert.Equal(9800.25m, row.TahunIni);
        Assert.Equal("AKRUAL", row.Jenis);
        Assert.Equal("Y", row.SetSub);
        Assert.Equal("USER1", row.UserGen);
        Assert.Equal("D", row.IsHeader);
        Assert.Equal("K", row.Posisi);
        Assert.Equal(LabaRugiRow.DetailRowKind, row.RowKind);
    }

    [Fact]
    public void FromDataRow_WhenRowKindColumnExists_MapsRowKind()
    {
        DataTable table = CreateLabaRugiTable();
        table.Columns.Add("ROWKIND", typeof(string));
        table.Rows.Add("EST1", string.Empty, 8, "PENDAPATAN", "Sub Total PENDAPATAN", string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, 1250.75m, 9800.25m, "LABARUGI", "P", "USER1", "G", "K", LabaRugiRow.SubtotalRowKind);

        LabaRugiRow row = LabaRugiRow.FromDataRow(table.Rows[0]);

        Assert.True(row.IsSubtotal);
        Assert.Equal(LabaRugiRow.SubtotalRowKind, row.RowKind);
    }

    [Fact]
    public void FromDataRow_WhenDbNull_UsesSafeDefaults()
    {
        DataTable table = CreateLabaRugiTable();
        table.Rows.Add(DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value);

        LabaRugiRow row = LabaRugiRow.FromDataRow(table.Rows[0]);

        Assert.Equal(string.Empty, row.IdData);
        Assert.Equal(string.Empty, row.KodeAcc);
        Assert.Equal(0, row.Urut);
        Assert.Equal(string.Empty, row.Sub1);
        Assert.Equal(0m, row.BulanIni);
        Assert.Equal(0m, row.TahunIni);
        Assert.Equal(string.Empty, row.IsHeader);
        Assert.Equal(LabaRugiRow.DetailRowKind, row.RowKind);
    }

    [Fact]
    public void FromDataRow_WhenRequiredColumnMissing_ThrowsClearError()
    {
        DataTable table = CreateLabaRugiTable();
        table.Columns.Remove("BULANINI");
        DataRow row = table.NewRow();
        table.Rows.Add(row);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => LabaRugiRow.FromDataRow(row));

        Assert.Contains("LabaRugi", ex.Message);
        Assert.Contains("BULANINI", ex.Message);
    }

    [Fact]
    public void CreateReportDataSet_UsesReportEngineSchemaAndAddsSubtotal()
    {
        LabaRugiRow source = CreateRow();

        DataSet dataSet = LabaRugiReportDataAdapter.CreateReportDataSet([source]);

        Assert.True(dataSet.Tables.Contains("LabaRugi"));
        DataTable table = dataSet.Tables["LabaRugi"]!;
        Assert.Equal(3, table.Rows.Count);
        Assert.True(table.Columns.Contains("KODEACC"));
        Assert.True(table.Columns.Contains("BULANINI"));
        Assert.True(table.Columns.Contains("TAHUNINI"));
        Assert.True(table.Columns.Contains("ROWKIND"));
        Assert.True(table.Columns.Contains("POSISI"));
        Assert.Equal("40.01", table.Rows[0]["KODEACC"]);
        Assert.Equal(1250.75m, table.Rows[0]["BULANINI"]);
        Assert.Equal(9800.25m, table.Rows[0]["TAHUNINI"]);
        Assert.Equal("Sub Total D", table.Rows[1]["SUB1"]);
        Assert.Equal(1250.75m, table.Rows[1]["BULANINI"]);
        Assert.Equal(9800.25m, table.Rows[1]["TAHUNINI"]);
        Assert.Equal(LabaRugiRow.SubtotalRowKind, table.Rows[1]["ROWKIND"]);
        Assert.Equal("G", table.Rows[1]["ISHEADER"]);
        Assert.Equal("LABA BERSIH", table.Rows[2]["SUB1"]);
        Assert.Equal(1250.75m, table.Rows[2]["BULANINI"]);
        Assert.Equal(9800.25m, table.Rows[2]["TAHUNINI"]);
        Assert.Equal(LabaRugiRow.TotalRowKind, table.Rows[2]["ROWKIND"]);
    }

    [Fact]
    public void CreateExportDataTable_ProjectsExpectedColumnsAndAddsSubtotal()
    {
        LabaRugiRow source = CreateRow();

        DataTable table = LabaRugiReportDataAdapter.CreateExportDataTable([source]);

        Assert.Equal(3, table.Columns.Count);
        Assert.Equal(3, table.Rows.Count);
        Assert.True(table.Columns.Contains("KETERANGAN"));
        Assert.True(table.Columns.Contains("BULANINI"));
        Assert.True(table.Columns.Contains("TAHUNINI"));
        Assert.Equal("Pendapatan", table.Rows[0]["KETERANGAN"]);
        Assert.Equal(1250.75m, table.Rows[0]["BULANINI"]);
        Assert.Equal(9800.25m, table.Rows[0]["TAHUNINI"]);
        Assert.Equal("Sub Total D", table.Rows[1]["KETERANGAN"]);
        Assert.Equal(1250.75m, table.Rows[1]["BULANINI"]);
        Assert.Equal(9800.25m, table.Rows[1]["TAHUNINI"]);
        Assert.Equal("LABA BERSIH", table.Rows[2]["KETERANGAN"]);
    }

    [Fact]
    public void CreateExportDataTable_WhenRowsEmpty_ReturnsEmptyValidTable()
    {
        DataTable table = LabaRugiReportDataAdapter.CreateExportDataTable([]);

        Assert.Equal(0, table.Rows.Count);
        Assert.Equal(["KETERANGAN", "BULANINI", "TAHUNINI"], table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray());
    }

    [Fact]
    public void AddSectionSubtotals_WhenMultipleSections_AppendsSubtotalAfterEachSection()
    {
        LabaRugiRow pendapatanA = CreateRow("PENDAPATAN", "P", "Pendapatan A", 10, 100m, 1000m);
        LabaRugiRow pendapatanB = CreateRow("PENDAPATAN", "P", "Pendapatan B", 11, 25m, 250m);
        LabaRugiRow biaya = CreateRow("BIAYA", "B", "Biaya", 20, 40m, 400m);

        List<LabaRugiRow> rows = LabaRugiReportDataAdapter.AddSectionSubtotals([pendapatanA, pendapatanB, biaya]);

        Assert.Equal(5, rows.Count);
        Assert.Equal("Pendapatan A", rows[0].Sub1);
        Assert.Equal("Pendapatan B", rows[1].Sub1);
        Assert.Equal("Sub Total PENDAPATAN", rows[2].Sub1);
        Assert.Equal(125m, rows[2].BulanIni);
        Assert.Equal(1250m, rows[2].TahunIni);
        Assert.True(rows[2].IsSubtotal);
        Assert.Equal("G", rows[2].IsHeader);
        Assert.Equal("Biaya", rows[3].Sub1);
        Assert.Equal("Sub Total BIAYA", rows[4].Sub1);
        Assert.Equal(40m, rows[4].BulanIni);
        Assert.Equal(400m, rows[4].TahunIni);
        Assert.True(rows[4].IsSubtotal);
    }

    [Fact]
    public void AddComputedTotals_EmitsLabaKotorAfterHppAndFinalLabaBersih()
    {
        LabaRugiRow pendapatan = CreateRow("PENDAPATAN", "PENDAPATAN", "Pendapatan", 10, 100m, 1000m, "K");
        LabaRugiRow hpp = CreateRow("HPP", "HPP", "Hpp", 20, 30m, 300m, "D");

        List<LabaRugiRow> rows = LabaRugiReportDataAdapter.BuildReportRows([pendapatan, hpp]);

        // pend detail, pend subtotal, hpp detail, hpp subtotal, LABA KOTOR, LABA BERSIH
        Assert.Equal(6, rows.Count);

        LabaRugiRow labaKotor = Assert.Single(rows, row => row.Sub1 == "LABA KOTOR");
        Assert.Equal(LabaRugiRow.TotalRowKind, labaKotor.RowKind);
        Assert.Equal(70m, labaKotor.BulanIni);   // 100 (K) - 30 (D)
        Assert.Equal(700m, labaKotor.TahunIni);  // 1000 - 300
        Assert.Equal("HPP", labaKotor.SetSub);   // stays inside the HPP group

        LabaRugiRow labaBersih = Assert.Single(rows, row => row.Sub1 == "LABA BERSIH");
        Assert.Equal(70m, labaBersih.BulanIni);
        Assert.Equal(700m, labaBersih.TahunIni);
    }

    [Fact]
    public void AddComputedTotals_RunningNetChainsCreditAndDebitSections()
    {
        LabaRugiRow pendapatan = CreateRow("PENDAPATAN", "PENDAPATAN", "Pendapatan", 10, 1000m, 12000m, "K");
        LabaRugiRow hpp = CreateRow("HPP", "HPP", "Hpp", 20, 400m, 5000m, "D");
        LabaRugiRow biayaUmum = CreateRow("BIAYA UMUM", "BIAYA_UMUM_ADMIN", "Biaya Umum", 30, 200m, 2500m, "D");

        List<LabaRugiRow> rows = LabaRugiReportDataAdapter.BuildReportRows([pendapatan, hpp, biayaUmum]);

        LabaRugiRow labaKotor = Assert.Single(rows, row => row.Sub1 == "LABA KOTOR");
        Assert.Equal(600m, labaKotor.BulanIni);   // 1000 - 400
        LabaRugiRow labaUsaha = Assert.Single(rows, row => row.Sub1 == "LABA USAHA");
        Assert.Equal(400m, labaUsaha.BulanIni);   // 600 - 200
        LabaRugiRow labaBersih = Assert.Single(rows, row => row.Sub1 == "LABA BERSIH");
        Assert.Equal(400m, labaBersih.BulanIni);  // overall net
        Assert.Equal(4500m, labaBersih.TahunIni); // 12000 - 5000 - 2500
    }

    [Fact]
    public void AddComputedTotals_SkipsResultLinesForAbsentSections()
    {
        LabaRugiRow pendapatan = CreateRow("PENDAPATAN", "PENDAPATAN", "Pendapatan", 10, 1000m, 12000m, "K");
        LabaRugiRow hpp = CreateRow("HPP", "HPP", "Hpp", 20, 400m, 5000m, "D");

        List<LabaRugiRow> rows = LabaRugiReportDataAdapter.BuildReportRows([pendapatan, hpp]);

        Assert.DoesNotContain(rows, row => row.Sub1 == "LABA USAHA");
        Assert.DoesNotContain(rows, row => row.Sub1 == "LABA SEBELUM PAJAK");
        Assert.DoesNotContain(rows, row => row.Sub1 == "LABA SETELAH PAJAK");
    }

    private static LabaRugiRow CreateRow(string sectionName, string setSub, string label, int urut, decimal bulanIni, decimal tahunIni, string posisi)
    {
        LabaRugiRow row = CreateRow(sectionName, setSub, label, urut, bulanIni, tahunIni);
        row.Posisi = posisi;
        return row;
    }

    private static LabaRugiRow CreateRow()
    {
        return new LabaRugiRow
        {
            IdData = "EST1",
            KodeAcc = "40.01",
            Urut = 7,
            TipeAcc = "D",
            Sub1 = "Pendapatan",
            Sub2 = "Sub2",
            Sub3 = "Sub3",
            Sub4 = "Sub4",
            Sub5 = "Sub5",
            Sub6 = "Sub6",
            BulanIni = 1250.75m,
            TahunIni = 9800.25m,
            Jenis = "AKRUAL",
            SetSub = "Y",
            UserGen = "USER1",
            IsHeader = "D",
            Posisi = "K"
        };
    }

    private static LabaRugiRow CreateRow(string sectionName, string setSub, string label, int urut, decimal bulanIni, decimal tahunIni)
    {
        LabaRugiRow row = CreateRow();
        row.Urut = urut;
        row.TipeAcc = sectionName;
        row.Sub1 = label;
        row.SetSub = setSub;
        row.BulanIni = bulanIni;
        row.TahunIni = tahunIni;
        return row;
    }

    private static DataTable CreateLabaRugiTable()
    {
        DataTable table = new("LabaRugi");
        table.Columns.Add("IDDATA", typeof(string));
        table.Columns.Add("KODEACC", typeof(string));
        table.Columns.Add("URUT", typeof(int));
        table.Columns.Add("TIPEACC", typeof(string));
        table.Columns.Add("SUB1", typeof(string));
        table.Columns.Add("SUB2", typeof(string));
        table.Columns.Add("SUB3", typeof(string));
        table.Columns.Add("SUB4", typeof(string));
        table.Columns.Add("SUB5", typeof(string));
        table.Columns.Add("SUB6", typeof(string));
        table.Columns.Add("BULANINI", typeof(decimal));
        table.Columns.Add("TAHUNINI", typeof(decimal));
        table.Columns.Add("JENIS", typeof(string));
        table.Columns.Add("SETSUB", typeof(string));
        table.Columns.Add("USERGEN", typeof(string));
        table.Columns.Add("ISHEADER", typeof(string));
        table.Columns.Add("POSISI", typeof(string));
        return table;
    }
}
