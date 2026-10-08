using Accounting.Laporan;
using Accounting.Model;
using Accounting.Services;
using DevExpress.Drawing.Printing;
using DevExpress.XtraReports.UI;
using OfficeOpenXml;
using System.Data;

namespace Accounting.Tests;

public sealed class NeracaSaldoReportTests
{
    [Fact]
    public void FromDataRecord_WhenColumnsValid_MapsTypedProperties()
    {
        DataTable table = CreateSourceTable();
        table.Rows.Add(
            "10.10000.000", "Kas", "10.00000.000", 3, "D", "N", 100m,
            101m, 102m, 103m, 104m, 105m, 106m, 107m, 108m, 109m, 110m, 111m, 112m);

        using DataTableReader reader = table.CreateDataReader();
        Assert.True(reader.Read());
        NeracaSaldoRow row = NeracaSaldoRow.FromDataRecord(reader);

        Assert.Equal("10.10000.000", row.KodeAkun);
        Assert.Equal("Kas", row.NamaAkun);
        Assert.Equal("10.00000.000", row.ParentAkun);
        Assert.Equal(3, row.Level);
        Assert.False(row.IsHeader);
        Assert.False(row.IsActive);
        Assert.Equal(100m, row.SaldoAwal);
        Assert.Equal(112m, row.Desember);
        Assert.True(row.HasAnyBalance);
    }

    [Fact]
    public void FromDataRecord_WhenValuesAreDbNull_UsesSafeDefaults()
    {
        DataTable table = CreateSourceTable();
        DataRow dataRow = table.NewRow();
        table.Rows.Add(dataRow);

        using DataTableReader reader = table.CreateDataReader();
        Assert.True(reader.Read());
        NeracaSaldoRow row = NeracaSaldoRow.FromDataRecord(reader);

        Assert.Equal(string.Empty, row.KodeAkun);
        Assert.Equal(0, row.Level);
        Assert.False(row.IsHeader);
        Assert.True(row.IsActive);
        Assert.Equal(0m, row.SaldoAwal);
        Assert.False(row.HasAnyBalance);
    }

    [Fact]
    public void FromDataRecord_WhenRequiredColumnMissing_ThrowsClearError()
    {
        DataTable table = CreateSourceTable();
        table.Columns.Remove("DES");
        table.Rows.Add(table.NewRow());

        using DataTableReader reader = table.CreateDataReader();
        Assert.True(reader.Read());
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => NeracaSaldoRow.FromDataRecord(reader));

        Assert.Contains("Neraca Saldo", exception.Message);
        Assert.Contains("DES", exception.Message);
    }

    [Fact]
    public void IncludeNonZeroRowsAndAncestors_KeepsHierarchyAndInactiveBalanceRows()
    {
        NeracaSaldoRow root = CreateRow("10", string.Empty, isHeader: true);
        NeracaSaldoRow section = CreateRow("10.10", "10", isHeader: true);
        NeracaSaldoRow zeroDetail = CreateRow("10.10.01", "10.10");
        NeracaSaldoRow inactiveBalance = CreateRow("10.10.02", "10.10", isActive: false, december: 250m);
        NeracaSaldoRow orphanBalance = CreateRow("99.01", "MISSING", december: 10m);

        IReadOnlyList<NeracaSaldoRow> result = NeracaSaldoReportService
            .IncludeNonZeroRowsAndAncestors([root, section, zeroDetail, inactiveBalance, orphanBalance]);

        Assert.Equal(["10", "10.10", "10.10.02", "99.01"], result.Select(row => row.KodeAkun));
        Assert.Contains(result, row => row.KodeAkun == "10.10.02" && !row.IsActive);
        Assert.DoesNotContain(result, row => row.KodeAkun == "10.10.01");
    }

    [Fact]
    public void IncludeNonZeroRowsAndAncestors_WhenParentsCycle_TerminatesAndPreservesOrder()
    {
        NeracaSaldoRow first = CreateRow("A", "B", december: 1m);
        NeracaSaldoRow second = CreateRow("B", "A", isHeader: true);

        IReadOnlyList<NeracaSaldoRow> result = NeracaSaldoReportService
            .IncludeNonZeroRowsAndAncestors([first, second]);

        Assert.Equal(["A", "B"], result.Select(row => row.KodeAkun));
    }

    [Fact]
    public void CreateWorkbook_WritesStableColumnsMetadataAndNumericValues()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        NeracaSaldoRow row = new()
        {
            KodeAkun = "10.10.02",
            NamaAkun = "Kas dan Bank",
            ParentAkun = "10.10",
            Level = 2,
            IsActive = true,
            SaldoAwal = 100.25m,
            Desember = 250.75m
        };
        NeracaSaldoReportMetadata metadata = CreateMetadata();

        byte[] workbookBytes = NeracaSaldoExcelExporter.CreateWorkbook([row], metadata);
        using MemoryStream stream = new(workbookBytes);
        using ExcelPackage package = new(stream);
        ExcelWorksheet worksheet = package.Workbook.Worksheets["Neraca Saldo"];

        Assert.Equal("PT TEST", worksheet.Cells[1, 1].Text);
        Assert.Equal("NERACA SALDO TAHUNAN", worksheet.Cells[3, 1].Text);
        Assert.Contains("2026", worksheet.Cells[4, 1].Text);
        Assert.Equal("Kode Akun", worksheet.Cells[6, 1].Text);
        Assert.Equal("Des", worksheet.Cells[6, 15].Text);
        Assert.Equal(100.25m, Convert.ToDecimal(worksheet.Cells[7, 3].Value));
        Assert.Equal(250.75m, Convert.ToDecimal(worksheet.Cells[7, 15].Value));
        Assert.Equal(NeracaSaldoExcelExporter.AccountingNumberFormat, worksheet.Cells[7, 3].Style.Numberformat.Format);
        Assert.InRange(worksheet.Column(1).Width, 12D, 24D);
        Assert.InRange(worksheet.Column(2).Width, 24D, 60D);
        Assert.InRange(worksheet.Column(3).Width, 12D, 20D);
        Assert.True(worksheet.Cells[7, 2].Style.WrapText);
    }

    [Fact]
    public void CreateFileName_SanitizesCompanyNameAndUsesTimestamp()
    {
        NeracaSaldoReportMetadata metadata = CreateMetadata() with
        {
            CompanyName = "PT TEST: UNIT"
        };

        string fileName = NeracaSaldoExcelExporter.CreateFileName(metadata);

        Assert.Equal("Neraca_Saldo_PT_TEST__UNIT_2026_20260716_103045.xlsx", fileName);
    }

    [Fact]
    public void PreviewReport_BindsRowsAndUsesA3LandscapeWithRepeatingHeader()
    {
        IReadOnlyList<NeracaSaldoRow> rows = [CreateRow("10", string.Empty, isHeader: true, december: 1m)];
        using NeracaSaldoTahun report = new();

        report.BindData(rows, CreateMetadata());
        report.CreateDocument();

        Assert.Same(rows, report.DataSource);
        Assert.NotEmpty(report.Pages);
        Assert.True(report.Landscape);
        Assert.Equal(DXPaperKind.A3, report.PaperKind);
        GroupHeaderBand groupHeader = Assert.Single(report.Bands.OfType<GroupHeaderBand>());
        Assert.True(groupHeader.RepeatEveryPage);
        XRTable headerTable = Assert.Single(groupHeader.Controls.OfType<XRTable>());
        Assert.Equal(15, headerTable.Rows[0].Cells.Count);
    }

    private static NeracaSaldoRow CreateRow(
        string code,
        string parent,
        bool isHeader = false,
        bool isActive = true,
        decimal december = 0m)
    {
        return new NeracaSaldoRow
        {
            KodeAkun = code,
            NamaAkun = $"Akun {code}",
            ParentAkun = parent,
            Level = parent.Length == 0 ? 1 : 2,
            IsHeader = isHeader,
            IsActive = isActive,
            Desember = december
        };
    }

    private static NeracaSaldoReportMetadata CreateMetadata()
    {
        return new NeracaSaldoReportMetadata(
            "PT TEST",
            "JAKARTA",
            2026,
            "tester",
            new DateTime(2026, 7, 16, 10, 30, 45));
    }

    private static DataTable CreateSourceTable()
    {
        DataTable table = new("NeracaSaldo");
        table.Columns.Add("KODEACC", typeof(string));
        table.Columns.Add("NAMAACC", typeof(string));
        table.Columns.Add("PARENTACC", typeof(string));
        table.Columns.Add("LVL", typeof(int));
        table.Columns.Add("ISHEADER", typeof(string));
        table.Columns.Add("ISAKTIF", typeof(string));
        table.Columns.Add("SALDOAWAL", typeof(decimal));
        table.Columns.Add("JAN", typeof(decimal));
        table.Columns.Add("FEB", typeof(decimal));
        table.Columns.Add("MAR", typeof(decimal));
        table.Columns.Add("APR", typeof(decimal));
        table.Columns.Add("MEI", typeof(decimal));
        table.Columns.Add("JUN", typeof(decimal));
        table.Columns.Add("JUL", typeof(decimal));
        table.Columns.Add("AGU", typeof(decimal));
        table.Columns.Add("SEP", typeof(decimal));
        table.Columns.Add("OKT", typeof(decimal));
        table.Columns.Add("NOV", typeof(decimal));
        table.Columns.Add("DES", typeof(decimal));
        return table;
    }
}
