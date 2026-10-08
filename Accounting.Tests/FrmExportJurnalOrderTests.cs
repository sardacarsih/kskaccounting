using Accounting.Form;
using Accounting.Model;

namespace Accounting.Tests;

public sealed class FrmExportJurnalOrderTests
{
    [Fact]
    public void NormalizeJurnalOrder_SortsByChronologicalPeriodJournalAndRow()
    {
        List<JurnalDetailDTO> rows =
        [
            CreateRow("10/2025", "001/JRN", 1),
            CreateRow("2/2025", "002/JRN", 1),
            CreateRow("1/2025", "002/JRN", 1),
            CreateRow("01/2025", "001/JRN", 2),
            CreateRow("1/2025", "001/JRN", 1)
        ];

        List<JurnalDetailDTO> result = FrmExportJurnal.NormalizeJurnalOrder(rows);

        Assert.Collection(
            result,
            row => AssertRow(row, "1/2025", "001/JRN", 1),
            row => AssertRow(row, "01/2025", "001/JRN", 2),
            row => AssertRow(row, "1/2025", "002/JRN", 1),
            row => AssertRow(row, "2/2025", "002/JRN", 1),
            row => AssertRow(row, "10/2025", "001/JRN", 1));
    }

    [Fact]
    public void NormalizeJurnalOrder_PlacesInvalidPeriodsAfterValidPeriodsDeterministically()
    {
        List<JurnalDetailDTO> rows =
        [
            CreateRow("invalid-b", "001/JRN", 1),
            CreateRow("12/2025", "001/JRN", 1),
            CreateRow("invalid-a", "002/JRN", 1),
            CreateRow("invalid-a", "001/JRN", 2),
            CreateRow("invalid-a", "001/JRN", 1)
        ];

        List<JurnalDetailDTO> result = FrmExportJurnal.NormalizeJurnalOrder(rows);

        Assert.Collection(
            result,
            row => AssertRow(row, "12/2025", "001/JRN", 1),
            row => AssertRow(row, "invalid-a", "001/JRN", 1),
            row => AssertRow(row, "invalid-a", "001/JRN", 2),
            row => AssertRow(row, "invalid-a", "002/JRN", 1),
            row => AssertRow(row, "invalid-b", "001/JRN", 1));
    }

    private static JurnalDetailDTO CreateRow(string periode, string noJurnal, int baris)
    {
        return new JurnalDetailDTO
        {
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

    private static void AssertRow(JurnalDetailDTO row, string periode, string noJurnal, int baris)
    {
        Assert.Equal(periode, row.Periode);
        Assert.Equal(noJurnal, row.NoJurnal);
        Assert.Equal(baris, row.BARIS);
    }
}
