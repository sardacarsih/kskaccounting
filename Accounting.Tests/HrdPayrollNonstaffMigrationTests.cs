using System.Text.Json;
using System.Text.RegularExpressions;

namespace Accounting.Tests;

public sealed class HrdPayrollNonstaffMigrationTests
{
    private const string MigrationId = "20260721_002_hrd_payroll_nonstaff_compat";

    private static readonly string[] CanonicalColumns =
    [
        "PERIODE", "REMISE", "NOABSEN", "NIK", "NAMA", "DIVISIID", "DIVISI", "IDJAB", "JAB", "GOL",
        "IDBAG", "BAG", "KERJAID", "KERJA", "ACKODE", "KATEGORI_TK", "ESTATE", "ACBANK",
        "HARIKERJANORMAL", "HKKOREKSI", "TOTAL_HK", "LIBURDIBAYAR", "UMAKAN", "TJGPERABOT", "JAMLEMBUR",
        "LEMBUR", "PREMI", "BRUTO", "POTONGAN", "NETTO", "KANTOR", "KOPERASI", "LAINNYA", "POT_BPJS_TK",
        "POT_BPJS_KESEHATAN", "POT_BPJS_TK_PENSIUN", "IDPT", "IDDATA", "PENDAPATAN_HK",
        "JUMLAH_HKNORMAL", "JUMLAH_LBAYAR"
    ];

    [Fact]
    public void Manifest_RegistersPayrollMigration()
    {
        string migratorDirectory = FindMigratorDirectory();
        using JsonDocument manifest = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(migratorDirectory, "migrations.manifest.json")));

        JsonElement root = manifest.RootElement;
        JsonElement[] migrations = root.GetProperty("migrations").EnumerateArray().ToArray();
        JsonElement migration = migrations.Single(entry => entry.GetProperty("id").GetString() == MigrationId);

        Assert.Equal("2026.10.08.2", root.GetProperty("version").GetString());
        Assert.Equal(66, migration.GetProperty("order").GetInt32());

        AssertMigrationFileExists(migratorDirectory, migration, "script");
        AssertMigrationFileExists(migratorDirectory, migration, "rollbackScript");
        AssertMigrationFileExists(migratorDirectory, migration, "checkScript");
    }

    [Fact]
    public void Migration_ContainsCanonicalColumnsAndSafeCompatibilityBehavior()
    {
        string migratorDirectory = FindMigratorDirectory();
        string migrationSql = File.ReadAllText(Path.Combine(
            migratorDirectory,
            "migrations",
            "V20260721_002__hrd_payroll_nonstaff_compat.sql"));
        string checkSql = File.ReadAllText(Path.Combine(
            migratorDirectory,
            "migrations",
            "C20260721_002__hrd_payroll_nonstaff_compat_check.sql"));

        Assert.Equal(41, CanonicalColumns.Length);
        foreach (string column in CanonicalColumns)
        {
            Assert.Matches(
                new Regex($@"add_column_if_missing\s*\(\s*'{column}'", RegexOptions.IgnoreCase),
                migrationSql);
            Assert.Matches(
                new Regex($@"require_column\s*\(\s*'{column}'", RegexOptions.IgnoreCase),
                checkSql);
        }

        Assert.Contains("IDX_HRD_PAYROLL_NS_SCOPE", migrationSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("(IDDATA, PERIODE, REMISE)", migrationSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("require_not_null('PERIODE')", migrationSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("require_not_null('REMISE')", migrationSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("require_not_null('NOABSEN')", migrationSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("require_not_null('NIK')", migrationSql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("INSERT INTO HRD_PAYROLL_NONSTAFF", migrationSql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DROP TABLE HRD_PAYROLL_NONSTAFF", migrationSql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PRIMARY KEY", migrationSql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UNIQUE (", migrationSql, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertMigrationFileExists(
        string migratorDirectory,
        JsonElement migration,
        string propertyName)
    {
        string relativePath = migration.GetProperty(propertyName).GetString()!;
        string fullPath = Path.Combine(migratorDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(fullPath), $"Migration asset was not found: {fullPath}");
    }

    private static string FindMigratorDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory != null)
        {
            string candidate = Path.Combine(directory.FullName, "Accounting", "Utilities", "Sql", "GLMigrator");
            if (File.Exists(Path.Combine(candidate, "migrations.manifest.json")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Accounting/Utilities/Sql/GLMigrator.");
    }
}
