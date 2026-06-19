using Accounting._1.Interface;
using Accounting.BusinessLayer;

namespace Accounting.Tests;

public sealed class AisJournalBuilderTests
{
    private static readonly AisJournalBuildContext Context = new()
    {
        Estate = "EST1",
        Remise = 1,
        MonthName = "Mei",
        Year = 2026,
        PeriodString = "05/2026",
        JournalDate = new DateTime(2026, 5, 15)
    };

    [Fact]
    public void BuildForDivision_WhenBorongan_AddsPphAndTotalRows()
    {
        Division division = new()
        {
            ISBORONGAN = 1,
            DIVISIID = "D01",
            DIVISI = "Divisi 01",
            NOMOR = "202605/R1/BORONGAN/EST1/Divisi 01"
        };

        List<AIS_JURNAL> aisRows =
        [
            new()
            {
                ISBORONGAN = 1,
                DIVISI = "D01",
                JENIS = "PANEN",
                PEKERJAAN = "Panen",
                BLOK = "A1",
                KODE = "40.00001.001",
                REKENING = "Upah Panen",
                DEBET = 975m,
                DEBETPPH = 975m,
                KREDIT = 0m,
                KETERANGAN = "Panen A1",
                POSTED = true,
                PERIODE = "05/2026"
            }
        ];

        List<JurnalKomponen> components =
        [
            new()
            {
                IsBorongan = true,
                Divisi = "D01",
                Keterangan = "Potongan borongan",
                Jumlah = 10m,
                Sisi = "K"
            }
        ];

        List<AIS_JURNAL_FINAL> result = AisJournalBuilder.BuildForDivision(aisRows, components, division, Context);

        Assert.Equal(4, result.Count);
        Assert.Equal(1000m, result[0].DEBET);
        Assert.Equal("31.00001.001", result[2].KODE);
        Assert.Equal(25m, result[2].KREDIT);
        Assert.Equal("33.00001.001", result[3].KODE);
        Assert.Equal(965m, result[3].KREDIT);
    }

    [Fact]
    public void BuildForDivision_WhenHarian_AddsComponentAndOperationalTotalRows()
    {
        Division division = new()
        {
            ISBORONGAN = 0,
            DIVISIID = "D02",
            DIVISI = "Divisi 02",
            NOMOR = "202605/R1/HARIAN/EST1/Divisi 02"
        };

        List<AIS_JURNAL> aisRows =
        [
            new()
            {
                ISBORONGAN = 0,
                DIVISI = "D02",
                JENIS = "RAWAT",
                PEKERJAAN = "Rawat",
                BLOK = "B1",
                KODE = "40.00001.001",
                REKENING = "Upah Harian",
                DEBET = 100m,
                KREDIT = 0m,
                KETERANGAN = "Rawat B1",
                POSTED = true,
                PERIODE = "05/2026"
            }
        ];

        List<JurnalKomponen> components =
        [
            new()
            {
                IsBorongan = false,
                Divisi = "D02",
                Keterangan = "Tambahan harian",
                Jumlah = 20m,
                Sisi = "D"
            },
            new()
            {
                IsBorongan = false,
                Divisi = "D02",
                Keterangan = "Potongan harian",
                Jumlah = 5m,
                Sisi = "K"
            }
        ];

        List<AIS_JURNAL_FINAL> result = AisJournalBuilder.BuildForDivision(aisRows, components, division, Context);

        Assert.Equal(4, result.Count);
        Assert.Equal(20m, result[1].DEBET);
        Assert.Equal(5m, result[2].KREDIT);
        Assert.Equal("33.00001.001", result[3].KODE);
        Assert.Equal(115m, result[3].KREDIT);
    }

    [Fact]
    public void BuildForDivisions_WhenExportingAllBorongan_KeepsRowNumbersPerJournal()
    {
        List<Division> divisions =
        [
            new() { ISBORONGAN = 1, DIVISIID = "D01", DIVISI = "Divisi 01", NOMOR = "BOR-01" },
            new() { ISBORONGAN = 1, DIVISIID = "D02", DIVISI = "Divisi 02", NOMOR = "BOR-02" },
            new() { ISBORONGAN = 0, DIVISIID = "D03", DIVISI = "Divisi 03", NOMOR = "HAR-03" }
        ];

        List<AIS_JURNAL> aisRows =
        [
            new() { ISBORONGAN = 1, DIVISI = "D01", JENIS = "PANEN", PEKERJAAN = "Panen", BLOK = "A1", DEBET = 100m, DEBETPPH = 100m, POSTED = true, PERIODE = "05/2026" },
            new() { ISBORONGAN = 1, DIVISI = "D02", JENIS = "PANEN", PEKERJAAN = "Panen", BLOK = "B1", DEBET = 200m, DEBETPPH = 200m, POSTED = true, PERIODE = "05/2026" },
            new() { ISBORONGAN = 0, DIVISI = "D03", JENIS = "RAWAT", PEKERJAAN = "Rawat", BLOK = "C1", DEBET = 300m, POSTED = true, PERIODE = "05/2026" }
        ];

        List<AIS_JURNAL_FINAL> result = AisJournalBuilder.BuildForDivisions(aisRows, [], divisions, Context, borongan: true);

        Assert.DoesNotContain(result, row => row.NOJURNAL == "HAR-03");
        Assert.Equal(1, result.First(row => row.NOJURNAL == "BOR-01").NO);
        Assert.Equal(1, result.First(row => row.NOJURNAL == "BOR-02").NO);
    }

    [Fact]
    public void BuildForDivisions_WhenExportingAllHarian_ExcludesBoronganHeaders()
    {
        List<Division> divisions =
        [
            new() { ISBORONGAN = 1, DIVISIID = "D01", DIVISI = "Divisi 01", NOMOR = "BOR-01" },
            new() { ISBORONGAN = 0, DIVISIID = "D02", DIVISI = "Divisi 02", NOMOR = "HAR-02" }
        ];

        List<AIS_JURNAL> aisRows =
        [
            new() { ISBORONGAN = 1, DIVISI = "D01", JENIS = "PANEN", PEKERJAAN = "Panen", BLOK = "A1", DEBET = 100m, DEBETPPH = 100m, POSTED = true, PERIODE = "05/2026" },
            new() { ISBORONGAN = 0, DIVISI = "D02", JENIS = "RAWAT", PEKERJAAN = "Rawat", BLOK = "B1", DEBET = 200m, POSTED = true, PERIODE = "05/2026" }
        ];

        List<AIS_JURNAL_FINAL> result = AisJournalBuilder.BuildForDivisions(aisRows, [], divisions, Context, borongan: false);

        Assert.DoesNotContain(result, row => row.NOJURNAL == "BOR-01");
        Assert.Contains(result, row => row.NOJURNAL == "HAR-02");
    }
}
