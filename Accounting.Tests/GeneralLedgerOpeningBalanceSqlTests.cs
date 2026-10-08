using System.Text.Json;

namespace Accounting.Tests;

public sealed class GeneralLedgerOpeningBalanceSqlTests
{
    private const string MigrationId = "20260728_001_bukubesar_all_account_opening_balances";

    [Fact]
    public void Manifest_RegistersAllAccountOpeningBalanceMigrationAsLatestEntry()
    {
        string migratorDirectory = FindMigratorDirectory();
        using JsonDocument manifest = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(migratorDirectory, "migrations.manifest.json")));

        JsonElement root = manifest.RootElement;
        JsonElement[] migrations = root.GetProperty("migrations").EnumerateArray().ToArray();
        JsonElement migration = migrations.Single(entry => entry.GetProperty("id").GetString() == MigrationId);

        Assert.Equal("2026.10.08.2", root.GetProperty("version").GetString());
        Assert.Equal(67, migration.GetProperty("order").GetInt32());
        Assert.Equal(69, migrations.Max(entry => entry.GetProperty("order").GetInt32()));

        AssertMigrationFileExists(migratorDirectory, migration, "script");
        AssertMigrationFileExists(migratorDirectory, migration, "checkScript");
    }

    [Fact]
    public void Migration_RemovesAccountTypeAndNonZeroOpeningFilters()
    {
        string migratorDirectory = FindMigratorDirectory();
        string migrationSql = File.ReadAllText(Path.Combine(
            migratorDirectory,
            "migrations",
            "V20260728_001__bukubesar_all_account_opening_balances.sql"));
        string checkSql = File.ReadAllText(Path.Combine(
            migratorDirectory,
            "migrations",
            "C20260728_001__bukubesar_all_account_opening_balances_check.sql"));
        string previousOpeningCheckSql = File.ReadAllText(Path.Combine(
            migratorDirectory,
            "migrations",
            "C20260714_001__bukubesar_opening_balance_check.sql"));
        string previousPeriodCheckSql = File.ReadAllText(Path.Combine(
            migratorDirectory,
            "migrations",
            "C20260714_002__bukubesar_period_opening_balances_check.sql"));

        Assert.Contains("v_procedure := REPLACE(v_procedure, v_group_filter, '');", migrationSql);
        Assert.Contains("v_procedure := REPLACE(v_procedure, v_non_zero_filter, '');", migrationSql);
        Assert.Contains("FOR month_number IN 1 .. 12 LOOP", checkSql);
        Assert.Contains("Account-type-independent opening", checkSql);
        Assert.Contains("Zero opening", checkSql);
        Assert.DoesNotContain("Profit and loss exclusion", previousOpeningCheckSql);
        Assert.DoesNotContain("Profit and loss exclusion", previousPeriodCheckSql);
        Assert.DoesNotContain("Zero opening exclusion", previousPeriodCheckSql);
        Assert.Contains("IF v_is_header = 'D' THEN", previousPeriodCheckSql);
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
