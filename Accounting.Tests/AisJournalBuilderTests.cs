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
    public void BuildForDivision_WhenBoronganWithFractionalAmounts_DebetEqualsKredit()
    {
        Division division = new()
        {
            ISBORONGAN = 1,
            DIVISIID = "D01",
            DIVISI = "Divisi 01",
            NOMOR = "BOR-01"
        };

        List<AIS_JURNAL> aisRows =
        [
            new() { ISBORONGAN = 1, DIVISI = "D01", JENIS = "PANEN", PEKERJAAN = "Panen", BLOK = "A1", DEBET = 975.005m, DEBETPPH = 975.005m, KREDIT = 0m, POSTED = true, PERIODE = "05/2026" },
            new() { ISBORONGAN = 1, DIVISI = "D01", JENIS = "PANEN", PEKERJAAN = "Panen", BLOK = "A2", DEBET = 333.337m, DEBETPPH = 333.337m, KREDIT = 0m, POSTED = true, PERIODE = "05/2026" }
        ];

        List<JurnalKomponen> components =
        [
            new() { IsBorongan = true, Divisi = "D01", Keterangan = "Potongan", Jumlah = 12.345m, Sisi = "K" },
            new() { IsBorongan = true, Divisi = "D01", Keterangan = "Tambahan", Jumlah = 7.005m, Sisi = "D" }
        ];

        List<AIS_JURNAL_FINAL> result = AisJournalBuilder.BuildForDivision(aisRows, components, division, Context);

        Assert.Equal(result.Sum(r => r.DEBET), result.Sum(r => r.KREDIT));
    }

    [Fact]
    public void BuildForDivision_WhenHarianWithFractionalAmounts_DebetEqualsKredit()
    {
        Division division = new()
        {
            ISBORONGAN = 0,
            DIVISIID = "D02",
            DIVISI = "Divisi 02",
            NOMOR = "HAR-02"
        };

        List<AIS_JURNAL> aisRows =
        [
            new() { ISBORONGAN = 0, DIVISI = "D02", JENIS = "RAWAT", PEKERJAAN = "Rawat", BLOK = "B1", DEBET = 100.005m, KREDIT = 0m, POSTED = true, PERIODE = "05/2026" },
            new() { ISBORONGAN = 0, DIVISI = "D02", JENIS = "RAWAT", PEKERJAAN = "Rawat", BLOK = "B2", DEBET = 55.557m, KREDIT = 0m, POSTED = true, PERIODE = "05/2026" }
        ];

        List<JurnalKomponen> components =
        [
            new() { IsBorongan = false, Divisi = "D02", Keterangan = "Tambahan", Jumlah = 20.005m, Sisi = "D" },
            new() { IsBorongan = false, Divisi = "D02", Keterangan = "Potongan", Jumlah = 5.005m, Sisi = "K" }
        ];

        List<AIS_JURNAL_FINAL> result = AisJournalBuilder.BuildForDivision(aisRows, components, division, Context);

        Assert.Equal(result.Sum(r => r.DEBET), result.Sum(r => r.KREDIT));
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

    private static readonly Dictionary<string, (string Kode, string Nama)> NoDefaults = new();

    [Fact]
    public void BuildForDivision_BoronganNonPanenRows_GoToBoronganJournalWithoutPph()
    {
        List<AIS_JURNAL> aisRows =
        [
            new() { ISBORONGAN = 1, DIVISI = "D01", JENIS = "PANEN", PEKERJAAN = "Panen", BLOK = "A1", KODE = "40.1", DEBET = 975m, DEBETPPH = 975m, POSTED = true },
            new() { ISBORONGAN = 1, DIVISI = "D01", JENIS = "RAWAT", PEKERJAAN = "Rawat", BLOK = "A2", KODE = "40.2", DEBET = 500m, DEBETPPH = 500m, POSTED = true },
            new() { ISBORONGAN = 0, DIVISI = "D01", JENIS = "RAWAT", PEKERJAAN = "Rawat", BLOK = "A3", KODE = "40.3", DEBET = 300m, POSTED = true }
        ];
        Division borongan = new() { ISBORONGAN = 1, DIVISIID = "D01", DIVISI = "Divisi 01", NOMOR = "BOR" };
        Division harian = new() { ISBORONGAN = 0, DIVISIID = "D01", DIVISI = "Divisi 01", NOMOR = "HAR" };

        List<AIS_JURNAL_FINAL> boronganRows = AisJournalBuilder.BuildForDivision(aisRows, [], borongan, Context, NoDefaults);
        List<AIS_JURNAL_FINAL> harianRows = AisJournalBuilder.BuildForDivision(aisRows, [], harian, Context, NoDefaults);

        Assert.Equal(1000m, boronganRows.Single(r => r.KODE == "40.1").DEBET);
        Assert.Equal(500m, boronganRows.Single(r => r.KODE == "40.2").DEBET);
        Assert.Equal(25m, boronganRows.Single(r => r.KODE == "31.00001.001").KREDIT);
        Assert.Equal(boronganRows.Sum(r => r.DEBET), boronganRows.Sum(r => r.KREDIT));
        Assert.DoesNotContain(harianRows, r => r.KODE == "40.2");
        Assert.Equal(300m, harianRows.Single(r => r.KODE == "33.00001.001").KREDIT);
    }

    [Fact]
    public void BuildForDivision_KomponenAccounts_ComeFromAcctDefaultWithHrisFallback()
    {
        Dictionary<string, (string Kode, string Nama)> defaults = new(StringComparer.OrdinalIgnoreCase)
        {
            ["AIS_KOMPONEN_BPJS_JHT"] = ("70.10001.005", "Tunjangan Astek"),
            ["HRIS_POT_BPJS"] = ("70.10001.006", "Potongan BPJS")
        };
        Division division = new() { ISBORONGAN = 0, DIVISIID = "D02", DIVISI = "Divisi 02", NOMOR = "HAR" };
        List<AIS_JURNAL> aisRows = [new() { ISBORONGAN = 0, DIVISI = "D02", JENIS = "RAWAT", KODE = "40.3", DEBET = 1000m, POSTED = true }];
        List<JurnalKomponen> components =
        [
            new() { Divisi = "D02", Keterangan = "BPJS JHT", Jumlah = 10m, Sisi = "K" },
            new() { Divisi = "D02", Keterangan = "BPJS KESEHATAN", Jumlah = 20m, Sisi = "K" },
            new() { Divisi = "D02", Keterangan = "UANG MAKAN", Jumlah = 30m, Sisi = "D" }
        ];

        List<AIS_JURNAL_FINAL> result = AisJournalBuilder.BuildForDivision(aisRows, components, division, Context, defaults);

        Assert.Equal("70.10001.005", result.Single(r => r.KETERANGAN?.StartsWith("BPJS JHT") == true).KODE);
        Assert.Equal("70.10001.006", result.Single(r => r.KETERANGAN?.StartsWith("BPJS KESEHATAN") == true).KODE);
        Assert.Null(result.Single(r => r.KETERANGAN?.StartsWith("UANG MAKAN") == true).KODE);
    }

    [Fact]
    public void GetJournaledMarker_MatchesBalancingCreditKeterangan()
    {
        Division harian = new() { ISBORONGAN = 0, DIVISIID = "D02", DIVISI = "Divisi 02", NOMOR = "HAR" };
        Division borongan = new() { ISBORONGAN = 1, DIVISIID = "D01", DIVISI = "Divisi 01", NOMOR = "BOR" };
        List<AIS_JURNAL> aisRows =
        [
            new() { ISBORONGAN = 0, DIVISI = "D02", JENIS = "RAWAT", KODE = "40.3", DEBET = 100m, POSTED = true },
            new() { ISBORONGAN = 1, DIVISI = "D01", JENIS = "PANEN", KODE = "40.1", DEBET = 975m, DEBETPPH = 975m, POSTED = true }
        ];

        string harianLast = AisJournalBuilder.BuildForDivision(aisRows, [], harian, Context, NoDefaults).Last().KETERANGAN;
        string boronganLast = AisJournalBuilder.BuildForDivision(aisRows, [], borongan, Context, NoDefaults).Last().KETERANGAN;

        Assert.Equal(AisJournalBuilder.GetJournaledMarker(harian, Context), harianLast);
        Assert.Equal(AisJournalBuilder.GetJournaledMarker(borongan, Context), boronganLast);
        Assert.Equal("TOTAL BIAYA OPERASIONAL Divisi 02 EST1 R1 Mei 2026", harianLast);
    }

    [Theory]
    [InlineData("BPJS JHT", "AIS_KOMPONEN_BPJS_JHT")]
    [InlineData(" tunjangan perabot ", "AIS_KOMPONEN_TUNJANGAN_PERABOT")]
    [InlineData(null, "AIS_KOMPONEN_")]
    public void KomponenAccountKey_NormalizesKeterangan(string? keterangan, string expected)
    {
        Assert.Equal(expected, AisJournalBuilder.KomponenAccountKey(keterangan));
    }
}
