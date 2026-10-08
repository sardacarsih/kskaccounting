using Accounting.Laporan;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace Accounting.Tests;

public sealed class GeneralLedgerLayoutTests
{
    private static readonly string[] AmountControlNames =
    [
        "tableCell13",
        "tableCell14",
        "xrLabel2",
        "xrLabel6",
        "label4",
        "label6"
    ];

    [Theory]
    [InlineData(typeof(GeneralLedgerD2))]
    [InlineData(typeof(GeneralLedgerK2))]
    public void AmountControls_UseFixedSingleLineShrinkLayout(Type reportType)
    {
        using XtraReport report = Assert.IsAssignableFrom<XtraReport>(Activator.CreateInstance(reportType));
        Dictionary<string, XRLabel> controls = report
            .AllControls<XRLabel>()
            .Where(control => AmountControlNames.Contains(control.Name, StringComparer.Ordinal))
            .ToDictionary(control => control.Name, StringComparer.Ordinal);

        Assert.Equal(AmountControlNames.Length, controls.Count);

        foreach (string controlName in AmountControlNames)
        {
            XRLabel control = controls[controlName];

            Assert.False(control.CanGrow);
            Assert.False(control.WordWrap);
            Assert.Equal(TextFitMode.ShrinkOnly, control.TextFitMode);
            Assert.Equal(TextAlignment.MiddleRight, control.TextAlignment);
            Assert.InRange(control.WidthF, 119.99F, 120.01F);
        }
    }

    [Theory]
    [InlineData(typeof(GeneralLedgerD2))]
    [InlineData(typeof(GeneralLedgerK2))]
    public void DetailAmountCells_PreserveWidthHeightAndNumberFormat(Type reportType)
    {
        using XtraReport report = Assert.IsAssignableFrom<XtraReport>(Activator.CreateInstance(reportType));
        XRTable detailTable = report.AllControls<XRTable>().Single(control => control.Name == "table3");
        Dictionary<string, XRTableCell> amountCells = report
            .AllControls<XRTableCell>()
            .Where(control => control.Name is "tableCell13" or "tableCell14")
            .ToDictionary(control => control.Name, StringComparer.Ordinal);

        Assert.Equal(840F, detailTable.WidthF);
        Assert.Equal(25F, detailTable.HeightF);
        Assert.Equal(25F, report.Bands[BandKind.Detail].HeightF);
        Assert.Equal(2, amountCells.Count);
        Assert.All(amountCells.Values, cell => Assert.Equal("{0:N2}", cell.TextFormatString));
    }
}
