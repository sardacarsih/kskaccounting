using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Accounting.Utilities.Update;

namespace Accounting.Tests;

public sealed class UpdateServiceTests : IDisposable
{
    private const string ManifestUrl = "https://update.example.com/accounting/latest.json";
    private readonly string downloadRoot = Path.Combine(Path.GetTempPath(), "AccountingUpdateTests", Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(downloadRoot))
        {
            Directory.Delete(downloadRoot, recursive: true);
        }
    }

    [Fact]
    public async Task CheckAsync_NewerVersion_ReturnsAvailableWithResolvedPackageUri()
    {
        var service = CreateService(_ => Json(Manifest("2.1.0.0", packageUrl: "Accounting-2.1.0.0.zip")));

        UpdateCheckResult result = await service.CheckAsync(CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.Available, result.Status);
        Assert.Equal(new Version(2, 1, 0, 0), result.LatestVersion);
        Assert.Equal("https://update.example.com/accounting/Accounting-2.1.0.0.zip", result.PackageUri!.ToString());
        Assert.False(result.IsMandatory);
    }

    [Theory]
    [InlineData("2.0.0.0")]
    [InlineData("1.9.9.9")]
    public async Task CheckAsync_SameOrOlderVersion_ReturnsUpToDate(string version)
    {
        var service = CreateService(_ => Json(Manifest(version)));

        UpdateCheckResult result = await service.CheckAsync(CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.UpToDate, result.Status);
    }

    [Theory]
    [InlineData(true, null, true)]
    [InlineData(false, "2.0.5.0", true)]
    [InlineData(false, "2.0.0.0", false)]
    public async Task CheckAsync_MandatoryFlagOrMinimumVersion_MarksMandatory(bool mandatory, string? minimumVersion, bool expected)
    {
        var service = CreateService(_ => Json(Manifest("2.1.0.0", mandatory: mandatory, minimumVersion: minimumVersion)));

        UpdateCheckResult result = await service.CheckAsync(CancellationToken.None);

        Assert.Equal(expected, result.IsMandatory);
    }

    [Theory]
    [InlineData("http://update.example.com/accounting/latest.json")]
    [InlineData("ftp://update.example.com/latest.json")]
    public async Task CheckAsync_InsecureManifestUrl_Fails(string url)
    {
        var handler = new StubHandler(_ => Json(Manifest("2.1.0.0")));
        var service = new UpdateService(Options(url), new HttpClient(handler), new Version(2, 0, 0, 0), downloadRoot);

        UpdateCheckResult result = await service.CheckAsync(CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.Failed, result.Status);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task CheckAsync_InsecurePackageUrl_Fails()
    {
        var service = CreateService(_ => Json(Manifest("2.1.0.0", packageUrl: "http://update.example.com/accounting/pkg.zip")));

        UpdateCheckResult result = await service.CheckAsync(CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.Failed, result.Status);
    }

    [Theory]
    [InlineData("{\"version\":\"abc\",\"packageUrl\":\"p.zip\",\"sha256\":\"" + ValidHash + "\"}")]
    [InlineData("{\"version\":\"2.1.0.0\",\"packageUrl\":\"p.zip\",\"sha256\":\"123\"}")]
    [InlineData("not json")]
    public async Task CheckAsync_InvalidManifest_Fails(string body)
    {
        var service = CreateService(_ => Json(body));

        UpdateCheckResult result = await service.CheckAsync(CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.Failed, result.Status);
    }

    [Fact]
    public async Task CheckAsync_HttpError_Fails()
    {
        var service = CreateService(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        UpdateCheckResult result = await service.CheckAsync(CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.Failed, result.Status);
        Assert.Contains("404", result.Error);
    }

    [Fact]
    public async Task CheckAsync_Disabled_DoesNotCallServer()
    {
        var handler = new StubHandler(_ => Json(Manifest("2.1.0.0")));
        var service = new UpdateService(UpdateOptions.Disabled, new HttpClient(handler), new Version(2, 0, 0, 0), downloadRoot);

        UpdateCheckResult result = await service.CheckAsync(CancellationToken.None);

        Assert.Equal(UpdateCheckStatus.Disabled, result.Status);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task DownloadAsync_MatchingHash_SavesPackage()
    {
        byte[] package = Encoding.UTF8.GetBytes("isi paket update");
        string hash = Convert.ToHexString(SHA256.HashData(package));
        var service = CreateService(request => request.RequestUri!.AbsolutePath.EndsWith(".json")
            ? Json(Manifest("2.1.0.0", sha256: hash, size: package.Length))
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(package) });

        UpdateCheckResult update = await service.CheckAsync(CancellationToken.None);
        var reported = new List<double>();
        string path = await service.DownloadAsync(update, new SyncProgress(reported.Add), CancellationToken.None);

        Assert.Equal(package, await File.ReadAllBytesAsync(path));
        Assert.Equal(1d, reported.Last());
        Assert.False(File.Exists(path + ".part"));
    }

    [Fact]
    public async Task DownloadAsync_HashMismatch_RejectsAndDeletesPackage()
    {
        byte[] package = Encoding.UTF8.GetBytes("paket yang dimodifikasi");
        var service = CreateService(request => request.RequestUri!.AbsolutePath.EndsWith(".json")
            ? Json(Manifest("2.1.0.0", sha256: ValidHash))
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(package) });

        UpdateCheckResult update = await service.CheckAsync(CancellationToken.None);

        await Assert.ThrowsAsync<InvalidDataException>(() => service.DownloadAsync(update, null, CancellationToken.None));
        string folder = Path.Combine(downloadRoot, "2.1.0.0");
        Assert.Empty(Directory.GetFiles(folder));
    }

    [Theory]
    [InlineData("https://host/a.zip", true)]
    [InlineData("http://localhost:8080/a.zip", true)]
    [InlineData("http://127.0.0.1/a.zip", true)]
    [InlineData("http://host/a.zip", false)]
    [InlineData("", false)]
    public void TryCreateSecureUri_OnlyAllowsHttpsOrLoopback(string url, bool expected)
    {
        Assert.Equal(expected, UpdateService.TryCreateSecureUri(url, null, out _));
    }

    [Fact]
    public void CanWriteDirectory_WritableFolder_ReturnsTrueAndLeavesNoProbe()
    {
        Directory.CreateDirectory(downloadRoot);

        Assert.True(UpdateService.CanWriteDirectory(downloadRoot));
        Assert.Empty(Directory.GetFiles(downloadRoot));
    }

    [Fact]
    public void CanWriteDirectory_MissingFolder_ReturnsFalse()
    {
        Assert.False(UpdateService.CanWriteDirectory(Path.Combine(downloadRoot, "tidak-ada")));
    }

    [Fact]
    public void CanWriteDirectory_ReadOnlyForUser_ReturnsFalse()
    {
        Directory.CreateDirectory(downloadRoot);
        var directory = new DirectoryInfo(downloadRoot);
        var deny = new System.Security.AccessControl.FileSystemAccessRule(
            System.Security.Principal.WindowsIdentity.GetCurrent().User!,
            System.Security.AccessControl.FileSystemRights.CreateFiles,
            System.Security.AccessControl.AccessControlType.Deny);

        System.Security.AccessControl.DirectorySecurity security = directory.GetAccessControl();
        security.AddAccessRule(deny);
        directory.SetAccessControl(security);
        try
        {
            Assert.False(UpdateService.CanWriteDirectory(downloadRoot));
        }
        finally
        {
            security.RemoveAccessRule(deny);
            directory.SetAccessControl(security);
        }
    }

    private const string ValidHash = "0000000000000000000000000000000000000000000000000000000000000000";

    private UpdateService CreateService(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        return new UpdateService(Options(ManifestUrl), new HttpClient(new StubHandler(respond)), new Version(2, 0, 0, 0), downloadRoot);
    }

    private static UpdateOptions Options(string url) => new() { Enabled = true, ManifestUrl = url, TimeoutSeconds = 5 };

    private static string Manifest(
        string version,
        bool mandatory = false,
        string? minimumVersion = null,
        string packageUrl = "https://update.example.com/accounting/pkg.zip",
        string sha256 = ValidHash,
        long? size = null)
    {
        var parts = new List<string>
        {
            $"\"version\":\"{version}\"",
            $"\"mandatory\":{mandatory.ToString().ToLowerInvariant()}",
            $"\"packageUrl\":\"{packageUrl}\"",
            $"\"sha256\":\"{sha256}\"",
            "\"notes\":\"Perbaikan\""
        };
        if (minimumVersion != null)
        {
            parts.Add($"\"minimumVersion\":\"{minimumVersion}\"");
        }

        if (size != null)
        {
            parts.Add($"\"size\":{size}");
        }

        return "{" + string.Join(",", parts) + "}";
    }

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private sealed class SyncProgress : IProgress<double>
    {
        private readonly Action<double> report;

        public SyncProgress(Action<double> report) => this.report = report;

        public void Report(double value) => report(value);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> respond;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => this.respond = respond;

        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(respond(request));
        }
    }
}
