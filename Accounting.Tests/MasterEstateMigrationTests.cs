using System.Text.Json;

namespace Accounting.Tests;

public sealed class MasterEstateMigrationTests
{
    private const string MigrationId = "20260721_001_master_estate_compat";

    [Fact]
    public void Manifest_RegistersMasterEstateMigration()
    {
        string migratorDirectory = FindMigratorDirectory();
        string manifestPath = Path.Combine(migratorDirectory, "migrations.manifest.json");
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));

        JsonElement root = manifest.RootElement;
        Assert.Equal("2026.10.08.1", root.GetProperty("version").GetString());

        JsonElement[] migrations = root
            .GetProperty("migrations")
            .EnumerateArray()
            .ToArray();
        JsonElement migration = migrations
            .Single(entry => entry.GetProperty("id").GetString() == MigrationId);

        Assert.Equal(65, migration.GetProperty("order").GetInt32());

        AssertMigrationFileExists(migratorDirectory, migration, "script");
        AssertMigrationFileExists(migratorDirectory, migration, "rollbackScript");
        AssertMigrationFileExists(migratorDirectory, migration, "checkScript");
    }

    [Fact]
    public void Migration_ContainsIdempotentMasterEstateContract()
    {
        string migrationPath = Path.Combine(
            FindMigratorDirectory(),
            "migrations",
            "V20260721_001__master_estate_compat.sql");
        string sql = File.ReadAllText(migrationPath);

        Assert.Contains("FROM user_tables", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE MASTER_ESTATE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("add_column_if_missing('ID'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("add_column_if_missing('ESTATEID'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("add_column_if_missing('NAMA'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("add_column_if_missing('IDDATA'", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("PRIMARY KEY (ID)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("UNIQUE (IDDATA, ESTATEID)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IDX_MASTER_ESTATE_IDDATA", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("INSERT INTO MASTER_ESTATE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DROP TABLE MASTER_ESTATE", sql, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertMigrationFileExists(
        string migratorDirectory,
        JsonElement migration,
        string propertyName)
    {
        string relativePath = migration.GetProperty(propertyName).GetString()!;
        string fullPath = Path.Combine(
            migratorDirectory,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.True(File.Exists(fullPath), $"Migration asset was not found: {fullPath}");
    }

    private static string FindMigratorDirectory()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory != null)
        {
            string candidate = Path.Combine(
                directory.FullName,
                "Accounting",
                "Utilities",
                "Sql",
                "GLMigrator");

            if (File.Exists(Path.Combine(candidate, "migrations.manifest.json")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Accounting/Utilities/Sql/GLMigrator.");
    }
}
