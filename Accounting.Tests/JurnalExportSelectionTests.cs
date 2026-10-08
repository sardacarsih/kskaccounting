using Accounting.Form;
using Accounting.Model;

namespace Accounting.Tests;

public sealed class JurnalExportSelectionTests
{
    [Fact]
    public void ResolveJurnalIds_WithCheckedRows_PrioritizesCheckedRowsOverFocusedRow()
    {
        List<double> result = JurnalExportSelection.ResolveJurnalIds([20, 10, 20], 30);

        Assert.Equal([20, 10], result);
    }

    [Fact]
    public void ResolveJurnalIds_WithoutCheckedRows_UsesFocusedRow()
    {
        List<double> result = JurnalExportSelection.ResolveJurnalIds([], 30);

        Assert.Equal([30], result);
    }

    [Fact]
    public void ResolveJurnalIds_IgnoresInvalidIds()
    {
        List<double> result = JurnalExportSelection.ResolveJurnalIds(
            [null, -1, 0, double.NaN, double.PositiveInfinity],
            null);

        Assert.Empty(result);
    }

    [Fact]
    public void FilterDetails_ReturnsOnlySelectedJournalsInNormalizedOrder()
    {
        List<JurnalDetailDTO> source =
        [
            CreateDetail(20, "02/2026", "002/JRN", 2),
            CreateDetail(30, "01/2026", "003/JRN", 1),
            CreateDetail(10, "01/2026", "001/JRN", 2),
            CreateDetail(20, "02/2026", "002/JRN", 1),
            CreateDetail(10, "01/2026", "001/JRN", 1)
        ];

        List<JurnalDetailDTO> result = JurnalExportSelection.FilterDetails(source, [20, 10]);

        Assert.Collection(
            result,
            row => AssertDetail(row, 10, 1),
            row => AssertDetail(row, 10, 2),
            row => AssertDetail(row, 20, 1),
            row => AssertDetail(row, 20, 2));
    }

    private static JurnalDetailDTO CreateDetail(double reffId, string periode, string noJurnal, int baris)
    {
        return new JurnalDetailDTO
        {
            REFFID = reffId,
            Periode = periode,
            NoJurnal = noJurnal,
            BARIS = baris,
            HIDREFF = string.Empty,
            Kode = string.Empty,
            Rekening = string.Empty,
            Keterangan = string.Empty,
            Posted = string.Empty
        };
    }

    private static void AssertDetail(JurnalDetailDTO row, double reffId, int baris)
    {
        Assert.Equal(reffId, row.REFFID);
        Assert.Equal(baris, row.BARIS);
    }
}
