using System.Text.Json;

namespace Accounting.Tests;

public sealed class PksMigrationApplicabilityTests
{
    private static readonly string[] PksMigrationIds =
    {
        "20260729_001_pks_global_report_settings",
        "20260729_002_pks_global_labarugi_repair"
    };

    [Fact]
    public void PksMigrations_AreGatedByAnExistingApplicabilityScript()
    {
        string root = FindMigratorDirectory();
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "migrations.manifest.json")));
        JsonElement[] migrations = manifest.RootElement.GetProperty("migrations").EnumerateArray().ToArray();

        foreach (string id in PksMigrationIds)
        {
            JsonElement migration = migrations.Single(entry => entry.GetProperty("id").GetString() == id);
            string relative = migration.GetProperty("applicableScript").GetString()!;
            Assert.True(File.Exists(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar))), $"Missing {relative}");
        }
    }

    [Fact]
    public void OtherMigrations_HaveNoApplicabilityGate()
    {
        string root = FindMigratorDirectory();
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "migrations.manifest.json")));

        string[] gated = manifest.RootElement.GetProperty("migrations").EnumerateArray()
            .Where(entry => entry.TryGetProperty("applicableScript", out _))
            .Select(entry => entry.GetProperty("id").GetString()!)
            .ToArray();

        Assert.Equal(PksMigrationIds.OrderBy(x => x), gated.OrderBy(x => x));
    }

    [Fact]
    public void ApplicabilityScript_PrintsMarkersAndNeverWrites()
    {
        string sql = File.ReadAllText(Path.Combine(FindMigratorDirectory(), "migrations", "A20260729_001__pks_scope_applicable.sql"));

        Assert.Contains("DBMS_OUTPUT.PUT_LINE('APPLICABLE')", sql);
        Assert.Contains("NOT_APPLICABLE:", sql);
        Assert.Contains("ACCT_RPT_PKS_SCOPE_BAK", sql);
        foreach (string forbidden in new[] { "INSERT ", "UPDATE ", "DELETE ", "MERGE ", "DROP ", "ALTER ", "CREATE ", "TRUNCATE " })
        {
            Assert.DoesNotContain(forbidden, sql.Replace("CREATE TABLE", string.Empty), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ExistingPksScripts_AreUnchanged_SoAppliedServersKeepTheirChecksum()
    {
        string migrations = Path.Combine(FindMigratorDirectory(), "migrations");

        Assert.Equal(
            "BDC936EC4DCFFF7002B71094EB8A2C1C774DDEC82F54F3AE637FAAEAD9EE55CB",
            Sha256Normalized(Path.Combine(migrations, "V20260729_001__pks_global_report_settings.sql")));
        Assert.Equal(
            "170DCF5745FE9F07E1B51CFCB89E41854E51E7FB6F372C74A89CE3509381A6DC",
            Sha256Normalized(Path.Combine(migrations, "V20260729_002__pks_global_labarugi_repair.sql")));
    }

    private static string Sha256Normalized(string path)
    {
        string normalized = File.ReadAllText(path).Replace("\r\n", "\n");
        using var sha = System.Security.Cryptography.SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(normalized)));
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
