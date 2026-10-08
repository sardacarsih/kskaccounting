using Serilog;
using System;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Accounting.Services
{
    public sealed class UpdateInfo
    {
        public Version CurrentVersion { get; init; }
        public Version LatestVersion { get; init; }
        public string DownloadUrl { get; init; }
        public string ReleaseNotes { get; init; }
    }

    /// <summary>
    /// Memeriksa ketersediaan versi baru dengan mengambil manifest JSON lewat HTTPS.
    /// Format manifest: { "version": "2.1.0", "downloadUrl": "https://...", "notes": "..." }
    /// Konfigurasi: appSettings key "Update:ManifestUrl" (wajib https). Kosong = fitur nonaktif.
    /// </summary>
    public static class UpdateCheckService
    {
        public const string ManifestUrlKey = "Update:ManifestUrl";
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

        public static Version GetCurrentVersion()
        {
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var fileVersion = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
            return Version.TryParse(fileVersion, out var v) ? v : asm.GetName().Version ?? new Version(0, 0);
        }

        /// <summary>Mengembalikan UpdateInfo bila ada versi lebih baru; null bila tidak ada / gagal / nonaktif.</summary>
        public static async Task<UpdateInfo> CheckAsync(CancellationToken ct = default)
        {
            try
            {
                string manifestUrl = ConfigurationManager.AppSettings[ManifestUrlKey];
                if (string.IsNullOrWhiteSpace(manifestUrl)) return null;

                if (!Uri.TryCreate(manifestUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                {
                    Log.Warning("Update check dilewati: {Key} harus berupa URL https", ManifestUrlKey);
                    return null;
                }

                using var response = await Http.GetAsync(uri, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.TryGetProperty("version", out var vProp) || !Version.TryParse(vProp.GetString(), out var latest))
                    return null;

                var current = GetCurrentVersion();
                if (latest <= current) return null;

                string download = root.TryGetProperty("downloadUrl", out var d) ? d.GetString() : null;
                // Hanya terima link unduhan https agar tidak diarahkan ke skema lain.
                if (!Uri.TryCreate(download, UriKind.Absolute, out var dl) || dl.Scheme != Uri.UriSchemeHttps)
                    download = null;

                return new UpdateInfo
                {
                    CurrentVersion = current,
                    LatestVersion = latest,
                    DownloadUrl = download,
                    ReleaseNotes = root.TryGetProperty("notes", out var n) ? n.GetString() : null
                };
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Pemeriksaan update gagal");
                return null;
            }
        }
    }
}
