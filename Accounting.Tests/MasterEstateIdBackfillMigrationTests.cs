using System.Text.Json;

namespace Accounting.Tests;

public sealed class MasterEstateIdBackfillMigrationTests
{
    private const string BackfillId = "20261008_001_master_estate_id_backfill";
    private const string CompatId = "20260721_001_master_estate_compat";

    [Fact]
    public void Manifest_RunsBackfillImmediatelyBeforeMasterEstateCompat()
    {
        string migratorDirectory = FindMigratorDirectory();
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(migratorDirectory, "migrations.manifest.json")));
        JsonElement[] migrations = manifest.RootElement.GetProperty("migrations").EnumerateArray().ToArray();

        JsonElement backfill = migrations.Single(entry => entry.GetProperty("id").GetString() == BackfillId);
        JsonElement compat = migrations.Single(entry => entry.GetProperty("id").GetString() == CompatId);

        Assert.Equal(compat.GetProperty("order").GetInt32() - 1, backfill.GetProperty("order").GetInt32());
        foreach (string asset in new[] { "script", "rollbackScript", "checkScript" })
        {
            string path = Path.Combine(migratorDirectory, backfill.GetProperty(asset).GetString()!.Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(path), $"Migration asset was not found: {path}");
        }
    }

    [Fact]
    public void Manifest_OrdersAreUnique()
    {
        string migratorDirectory = FindMigratorDirectory();
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(migratorDirectory, "migrations.manifest.json")));
        int[] orders = manifest.RootElement.GetProperty("migrations").EnumerateArray()
            .Select(entry => entry.GetProperty("order").GetInt32()).ToArray();

        Assert.Equal(orders.Length, orders.Distinct().Count());
    }

    [Fact]
    public void Migration_BackfillsNullIdsBeforePrimaryKeyAndIsIdempotent()
    {
        string sql = File.ReadAllText(Path.Combine(FindMigratorDirectory(), "migrations", "V20261008_001__master_estate_id_backfill.sql"));

        Assert.Contains("FROM user_tables", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ID IS NULL", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ROW_NUMBER() OVER (ORDER BY ROWID)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("NVL(MAX(ID), 0)", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HAVING COUNT(*) > 1", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DROP TABLE", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DELETE FROM", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("&", sql);
    }

    [Fact]
    public void ExistingMasterEstateScript_IsUnchanged_SoAppliedServersKeepTheirChecksum()
    {
        string path = Path.Combine(FindMigratorDirectory(), "migrations", "V20260721_001__master_estate_compat.sql");
        string normalized = File.ReadAllText(path).Replace("\r\n", "\n");

        using var sha = System.Security.Cryptography.SHA256.Create();
        string hash = Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(normalized)));

        // Editing an applied migration forces `rebaselinechecksum` on every server; fix forward with a new migration instead.
        Assert.Equal(ExpectedCompatHash, hash);
    }

    private const string ExpectedCompatHash = "DD705A61FDF1E6DCA4156263B254954FB79CB11F0E0075F726C3376C96616451";

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
