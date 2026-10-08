using Accounting.Updater;
using Accounting.Utilities.Update;

namespace Accounting.Tests;

public sealed class UpdateAppConfigFallbackTests : IDisposable
{
    private readonly string configPath = Path.Combine(Path.GetTempPath(), $"accounting-update-fallback-{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(configPath))
        {
            File.Delete(configPath);
        }
    }

    [Fact]
    public void Load_NoConfigFile_UsesAppConfigManifestUrl()
    {
        UpdateOptions options = UpdateOptionsLoader.Load(configPath, "https://host/accounting/latest.json");

        Assert.True(options.Enabled);
        Assert.Equal("https://host/accounting/latest.json", options.ManifestUrl);
    }

    [Fact]
    public void Load_ConfigWithoutUpdateSection_UsesAppConfigManifestUrl()
    {
        File.WriteAllText(configPath, """{ "ActiveServerKey": "X" }""");

        UpdateOptions options = UpdateOptionsLoader.Load(configPath, "https://host/accounting/latest.json");

        Assert.True(options.Enabled);
    }

    [Fact]
    public void Load_UpdateSectionDisabled_WinsOverAppConfig()
    {
        File.WriteAllText(configPath, """{ "Update": { "Enabled": false, "ManifestUrl": "https://x/y.json" } }""");

        Assert.False(UpdateOptionsLoader.Load(configPath, "https://host/accounting/latest.json").Enabled);
    }

    [Fact]
    public void Load_NoConfigAndNoAppConfigUrl_IsDisabled()
    {
        Assert.False(UpdateOptionsLoader.Load(configPath, null).Enabled);
    }

    [Theory]
    [InlineData("""{ "Update": { "RunMigrator": false } }""", false)]
    [InlineData("""{ "Update": { "RunMigrator": true } }""", true)]
    [InlineData("""{ "Update": { "Enabled": true } }""", true)]
    [InlineData("""{ "ActiveServerKey": "X" }""", true)]
    [InlineData("not json", true)]
    public void IsMigratorEnabled_ReadsRunMigratorFlag(string json, bool expected)
    {
        File.WriteAllText(configPath, json);

        Assert.Equal(expected, DatabaseMigrationRunner.IsMigratorEnabled(configPath));
    }

    [Fact]
    public void MigrationRun_WithoutMigratorExe_IsSkipped()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"accounting-nomigrator-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        try
        {
            MigrationOutcome outcome = DatabaseMigrationRunner.Run(dir, _ => { }, TimeSpan.FromSeconds(5));

            Assert.Equal(MigrationStatus.Skipped, outcome.Status);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}
