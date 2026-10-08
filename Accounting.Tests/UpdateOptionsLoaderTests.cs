using Accounting.Utilities.Update;

namespace Accounting.Tests;

public sealed class UpdateOptionsLoaderTests : IDisposable
{
    private readonly string configPath = Path.Combine(Path.GetTempPath(), $"accounting-update-config-{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        if (File.Exists(configPath))
        {
            File.Delete(configPath);
        }
    }

    [Fact]
    public void Load_ValidSection_ReturnsOptions()
    {
        File.WriteAllText(configPath, """
            {
              "ActiveServerKey": "X",
              "Update": { "Enabled": true, "ManifestUrl": " https://host/accounting/latest.json ", "CheckIntervalMinutes": 60, "TimeoutSeconds": 10 }
            }
            """);

        UpdateOptions options = UpdateOptionsLoader.Load(configPath);

        Assert.True(options.Enabled);
        Assert.Equal("https://host/accounting/latest.json", options.ManifestUrl);
        Assert.Equal(60, options.CheckIntervalMinutes);
        Assert.Equal(10, options.TimeoutSeconds);
    }

    [Fact]
    public void Load_IntervalBelowMinimum_IsClamped()
    {
        File.WriteAllText(configPath, """{ "Update": { "Enabled": true, "ManifestUrl": "https://h/l.json", "CheckIntervalMinutes": 1 } }""");

        Assert.Equal(15, UpdateOptionsLoader.Load(configPath).CheckIntervalMinutes);
    }

    [Theory]
    [InlineData("""{ "ActiveServerKey": "X" }""")]
    [InlineData("""{ "Update": { "Enabled": false, "ManifestUrl": "https://h/l.json" } }""")]
    [InlineData("""{ "Update": { "Enabled": true, "ManifestUrl": "" } }""")]
    [InlineData("{ rusak")]
    public void Load_MissingDisabledOrBrokenSection_ReturnsDisabled(string json)
    {
        File.WriteAllText(configPath, json);

        Assert.False(UpdateOptionsLoader.Load(configPath).Enabled);
    }

    [Fact]
    public void Load_MissingFile_ReturnsDisabled()
    {
        Assert.False(UpdateOptionsLoader.Load(configPath).Enabled);
    }
}
