using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Serilog;

namespace Accounting.Utilities.Update
{
    /// <summary>
    /// Checks <c>latest.json</c> on the HTTPS update server, downloads and verifies the package,
    /// and hands it to Accounting.Updater.exe which replaces the files after this process exits.
    /// </summary>
    internal sealed class UpdateService
    {
        internal const string UpdaterExeName = "Accounting.Updater.exe";
        private static readonly string[] UpdaterFiles =
        {
            "Accounting.Updater.exe",
            "Accounting.Updater.dll",
            "Accounting.Updater.runtimeconfig.json",
            "Accounting.Updater.deps.json"
        };

        private static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(30);
        private static readonly Regex Sha256Pattern = new("^[0-9a-fA-F]{64}$", RegexOptions.Compiled);
        private static readonly HttpClient SharedClient = new() { Timeout = Timeout.InfiniteTimeSpan };

        private readonly UpdateOptions _options;
        private readonly HttpClient _httpClient;
        private readonly string _downloadRoot;

        public UpdateService(UpdateOptions options)
            : this(options, SharedClient, CurrentAppVersion, DefaultDownloadRoot)
        {
        }

        internal UpdateService(UpdateOptions options, HttpClient httpClient, Version currentVersion, string downloadRoot)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            CurrentVersion = currentVersion ?? throw new ArgumentNullException(nameof(currentVersion));
            _downloadRoot = downloadRoot ?? throw new ArgumentNullException(nameof(downloadRoot));
        }

        public static Version CurrentAppVersion => typeof(UpdateService).Assembly.GetName().Version ?? new Version(0, 0, 0, 0);

        private static string DefaultDownloadRoot => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Accounting",
            "updates");

        public Version CurrentVersion { get; }

        public bool IsEnabled => _options.Enabled && !string.IsNullOrWhiteSpace(_options.ManifestUrl);

        public async Task<UpdateCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            if (!IsEnabled)
            {
                return new UpdateCheckResult(UpdateCheckStatus.Disabled, CurrentVersion);
            }

            if (!TryCreateSecureUri(_options.ManifestUrl, null, out Uri? manifestUri))
            {
                return Fail("URL manifest update harus menggunakan https.");
            }

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, manifestUri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
                AddUserAgent(request);

                using HttpResponseMessage response = await _httpClient.SendAsync(request, timeoutCts.Token).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    return Fail($"Server update menjawab HTTP {(int)response.StatusCode}.");
                }

                string body = await response.Content.ReadAsStringAsync(timeoutCts.Token).ConfigureAwait(false);
                UpdateManifest? manifest = JsonSerializer.Deserialize<UpdateManifest>(body);
                return Evaluate(manifest, manifestUri!);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Fail($"Timeout setelah {_options.TimeoutSeconds} detik.");
            }
            catch (HttpRequestException ex)
            {
                return Fail("Koneksi ke server update gagal: " + ex.Message);
            }
            catch (JsonException ex)
            {
                return Fail("Format manifest update tidak valid: " + ex.Message);
            }
        }

        internal UpdateCheckResult Evaluate(UpdateManifest? manifest, Uri manifestUri)
        {
            if (manifest is null)
            {
                return Fail("Manifest update kosong.");
            }

            if (!Version.TryParse(manifest.Version, out Version? latest))
            {
                return Fail($"Versi pada manifest tidak valid: '{manifest.Version}'.");
            }

            if (string.IsNullOrWhiteSpace(manifest.Sha256) || !Sha256Pattern.IsMatch(manifest.Sha256.Trim()))
            {
                return Fail("Checksum sha256 pada manifest tidak valid.");
            }

            if (!TryCreateSecureUri(manifest.PackageUrl, manifestUri, out Uri? packageUri))
            {
                return Fail("URL paket update harus menggunakan https.");
            }

            Version? minimum = null;
            if (!string.IsNullOrWhiteSpace(manifest.MinimumVersion) && !Version.TryParse(manifest.MinimumVersion, out minimum))
            {
                return Fail($"minimumVersion pada manifest tidak valid: '{manifest.MinimumVersion}'.");
            }

            if (latest <= CurrentVersion)
            {
                return new UpdateCheckResult(UpdateCheckStatus.UpToDate, CurrentVersion, latest, manifest, packageUri);
            }

            bool mandatory = manifest.Mandatory || (minimum is not null && CurrentVersion < minimum);
            Log.Information("Update tersedia: {Current} -> {Latest} (wajib: {Mandatory})", CurrentVersion, latest, mandatory);
            return new UpdateCheckResult(UpdateCheckStatus.Available, CurrentVersion, latest, manifest, packageUri, mandatory);
        }

        /// <summary>Downloads the package and verifies size and SHA-256. Returns the local zip path.</summary>
        public async Task<string> DownloadAsync(UpdateCheckResult update, IProgress<double>? progress, CancellationToken cancellationToken)
        {
            if (update.Status != UpdateCheckStatus.Available || update.Manifest is null || update.PackageUri is null || update.LatestVersion is null)
            {
                throw new InvalidOperationException("Tidak ada update yang bisa diunduh.");
            }

            string versionText = update.LatestVersion.ToString();
            string folder = Path.Combine(_downloadRoot, versionText);
            RemoveOtherDownloads(versionText);
            Directory.CreateDirectory(folder);

            string zipPath = Path.Combine(folder, $"Accounting-{versionText}.zip");
            string partPath = zipPath + ".part";

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(DownloadTimeout);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, update.PackageUri);
                AddUserAgent(request);
                using HttpResponseMessage response = await _httpClient
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Server update menjawab HTTP {(int)response.StatusCode} saat mengunduh paket." +
                        $"{Environment.NewLine}{update.PackageUri}" +
                        $"{Environment.NewLine}Coba lagi beberapa menit lagi; bila tetap gagal hubungi IT.");
                }

                long? total = response.Content.Headers.ContentLength ?? update.Manifest.Size;
                long received = 0;
                using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

                await using (Stream source = await response.Content.ReadAsStreamAsync(timeoutCts.Token).ConfigureAwait(false))
                await using (var target = new FileStream(partPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    byte[] buffer = new byte[81920];
                    int read;
                    while ((read = await source.ReadAsync(buffer, timeoutCts.Token).ConfigureAwait(false)) > 0)
                    {
                        hash.AppendData(buffer, 0, read);
                        await target.WriteAsync(buffer.AsMemory(0, read), timeoutCts.Token).ConfigureAwait(false);
                        received += read;
                        if (total is > 0)
                        {
                            progress?.Report(Math.Min(1d, (double)received / total.Value));
                        }
                    }
                }

                if (update.Manifest.Size is > 0 && received != update.Manifest.Size)
                {
                    throw new InvalidDataException($"Ukuran paket tidak sesuai (diterima {received} byte, manifest {update.Manifest.Size} byte).");
                }

                string actual = Convert.ToHexString(hash.GetHashAndReset());
                if (!string.Equals(actual, update.Manifest.Sha256.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException("Checksum SHA-256 paket update tidak cocok. Paket ditolak.");
                }

                File.Move(partPath, zipPath, overwrite: true);
                progress?.Report(1d);
                Log.Information("Paket update {Version} diunduh ke {Path} ({Bytes} byte)", versionText, zipPath, received);
                return zipPath;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                TryDelete(partPath);
                throw new TimeoutException("Unduhan paket update melebihi batas waktu.");
            }
            catch
            {
                TryDelete(partPath);
                throw;
            }
        }

        /// <summary>
        /// Starts Accounting.Updater.exe from a temp copy (so it can update its own files) and lets it
        /// wait for this process to exit. The caller must close the application afterwards.
        /// </summary>
        public static void LaunchInstaller(string zipPath, Version version)
        {
            string appDirectory = AppDirectory;
            string tempDirectory = Path.Combine(Path.GetTempPath(), "AccountingUpdater", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);

            foreach (string file in UpdaterFiles)
            {
                string source = Path.Combine(appDirectory, file);
                if (!File.Exists(source))
                {
                    if (file.EndsWith(".deps.json", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    throw new FileNotFoundException($"{file} tidak ditemukan di folder aplikasi. Pasang ulang aplikasi secara manual.", source);
                }

                File.Copy(source, Path.Combine(tempDirectory, file), overwrite: true);
            }

            var startInfo = new ProcessStartInfo(Path.Combine(tempDirectory, UpdaterExeName))
            {
                UseShellExecute = false,
                WorkingDirectory = tempDirectory
            };
            startInfo.ArgumentList.Add("--pid");
            startInfo.ArgumentList.Add(Environment.ProcessId.ToString());
            startInfo.ArgumentList.Add("--zip");
            startInfo.ArgumentList.Add(zipPath);
            startInfo.ArgumentList.Add("--target");
            startInfo.ArgumentList.Add(appDirectory);
            startInfo.ArgumentList.Add("--exe");
            startInfo.ArgumentList.Add(Path.GetFileName(Environment.ProcessPath) ?? "Accounting.exe");
            startInfo.ArgumentList.Add("--version");
            startInfo.ArgumentList.Add(version.ToString());

            Process.Start(startInfo);
            Log.Information("Accounting.Updater dijalankan untuk versi {Version} dari {Path}", version, tempDirectory);
        }

        public static string AppDirectory =>
            AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        /// <summary>
        /// Whether the current Windows user can replace the application files. When false, Accounting.Updater
        /// has to ask for administrator rights (UAC), which a standard user cannot grant.
        /// </summary>
        public static bool CanWriteAppDirectory() => CanWriteDirectory(AppDirectory);

        internal static bool CanWriteDirectory(string directory)
        {
            string probe = Path.Combine(directory, $".update_probe_{Guid.NewGuid():N}");
            try
            {
                using (new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose))
                {
                }

                return true;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                return false;
            }
        }

        internal static bool TryCreateSecureUri(string? text, Uri? baseUri, out Uri? uri)
        {
            uri = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (!Uri.TryCreate(text.Trim(), UriKind.Absolute, out Uri? candidate)
                && (baseUri is null || !Uri.TryCreate(baseUri, text.Trim(), out candidate)))
            {
                return false;
            }

            bool secure = candidate.Scheme == Uri.UriSchemeHttps
                || (candidate.Scheme == Uri.UriSchemeHttp && candidate.IsLoopback);
            if (!secure)
            {
                return false;
            }

            uri = candidate;
            return true;
        }

        private void RemoveOtherDownloads(string keepVersion)
        {
            if (!Directory.Exists(_downloadRoot))
            {
                return;
            }

            foreach (string directory in Directory.GetDirectories(_downloadRoot))
            {
                if (string.Equals(Path.GetFileName(directory), keepVersion, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    Directory.Delete(directory, recursive: true);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    Log.Debug(ex, "Gagal menghapus unduhan update lama {Directory}", directory);
                }
            }
        }

        private void AddUserAgent(HttpRequestMessage request)
        {
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Accounting", CurrentVersion.ToString()));
        }

        private UpdateCheckResult Fail(string error)
        {
            Log.Warning("Cek update gagal: {Error}", error);
            return new UpdateCheckResult(UpdateCheckStatus.Failed, CurrentVersion, Error: error);
        }

        private static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Leftover .part files are removed on the next download.
            }
        }
    }
}
