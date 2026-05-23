using Accounting._1.Interface;
using Accounting.BusinessLayer;

namespace Accounting.Tests;

public sealed class JurnalAmountRoundingTests
{
    [Theory]
    [InlineData("123.456", "123.46")]
    [InlineData("123.454", "123.45")]
    [InlineData("1.005", "1.01")]
    [InlineData("-1.005", "-1.01")]
    public void RoundJournalAmount_WhenValueHasMoreThanTwoDecimals_RoundsAwayFromZero(string value, string expected)
    {
        decimal amount = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
        decimal expectedAmount = decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture);

        decimal actual = JurnalAmountRounding.RoundJournalAmount(amount);

        Assert.Equal(expectedAmount, actual);
    }

    [Fact]
    public void NormalizeAisFinalRows_WhenAmountsHaveMoreThanTwoDecimals_RoundsDebitAndCreditOnly()
    {
        List<AIS_JURNAL_FINAL> rows =
        [
            new()
            {
                NOJURNAL = "AIS-001",
                TANGGAL = new DateTime(2026, 5, 15),
                NO = 1,
                KODE = "40.00001.001",
                REKENING = "Upah",
                DEBET = 123.456m,
                KREDIT = 1.005m,
                KETERANGAN = "Test",
                POSTED = true,
                PERIODE = "05/2026"
            }
        ];

        List<AIS_JURNAL_FINAL> normalized = JurnalAmountRounding.NormalizeAisFinalRows(rows);

        Assert.Equal(123.46m, normalized[0].DEBET);
        Assert.Equal(1.01m, normalized[0].KREDIT);
        Assert.Equal("AIS-001", normalized[0].NOJURNAL);
        Assert.Equal("Test", normalized[0].KETERANGAN);
    }
}
