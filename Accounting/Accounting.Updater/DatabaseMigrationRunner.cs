using System.Diagnostics;
using System.Text.Json;

namespace Accounting.Updater
{
    internal enum MigrationStatus
    {
        Skipped,
        Succeeded,
        Failed
    }

    internal sealed record MigrationOutcome(MigrationStatus Status, string? Error = null, string? LogDirectory = null);

    /// <summary>
    /// Runs the GLMigrator.exe shipped in the update package against the site's own
    /// Utilities\config.json: <c>--mode up</c> to apply pending migrations, then <c>--mode verify</c>
    /// because the migrator's "up" output alone is not proof that every script really ran.
    /// GL_MIGRATION_HISTORY makes repeated runs safe, so every database is migrated by the first
    /// client that updates and later clients find nothing pending.
    /// Requires <c>sqlplus</c> on PATH; sites without it can set <c>Update.RunMigrator=false</c>.
    /// </summary>
    internal static class DatabaseMigrationRunner
    {
        internal const string MigratorExeName = "GLMigrator.exe";
        private const int MaxErrorLines = 15;

        private static readonly string[] ConfigCandidates =
        {
            Path.Combine("Utilities", "config.json"),
            "config.json"
        };

        public static MigrationOutcome Run(string targetDirectory, Action<string> log, TimeSpan timeout)
        {
            string migrator = Path.Combine(targetDirectory, MigratorExeName);
            if (!File.Exists(migrator))
            {
                log($"{MigratorExeName} tidak ada di paket, migrasi database dilewati.");
                return new MigrationOutcome(MigrationStatus.Skipped);
            }

            string? config = ConfigCandidates
                .Select(candidate => Path.Combine(targetDirectory, candidate))
                .FirstOrDefault(File.Exists);
            if (config is null)
            {
                return new MigrationOutcome(MigrationStatus.Failed, "config.json koneksi database tidak ditemukan.");
            }

            if (!IsMigratorEnabled(config))
            {
                log("Update.RunMigrator=false di config.json, migrasi database dilewati.");
                return new MigrationOutcome(MigrationStatus.Skipped);
            }

            string logDirectory = Path.Combine(targetDirectory, "logs", $"migrator-update-{DateTime.Now:yyyyMMdd-HHmmss}");
            DateTime deadline = DateTime.UtcNow + timeout;

            foreach (string mode in new[] { "up", "verify" })
            {
                TimeSpan remaining = deadline - DateTime.UtcNow;
                MigrationOutcome step = RunMode(
                    migrator,
                    mode,
                    config,
                    logDirectory,
                    targetDirectory,
                    log,
                    remaining > TimeSpan.Zero ? remaining : TimeSpan.FromSeconds(1),
                    timeout);
                if (step.Status == MigrationStatus.Failed)
                {
                    return step;
                }
            }

            log("Migrasi database selesai dan terverifikasi.");
            return new MigrationOutcome(MigrationStatus.Succeeded, LogDirectory: logDirectory);
        }

        /// <summary>Update.RunMigrator in config.json; absent or unreadable means enabled.</summary>
        internal static bool IsMigratorEnabled(string configPath)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(
                    File.ReadAllText(configPath),
                    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });

                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return true;
                }

                foreach (JsonProperty section in doc.RootElement.EnumerateObject())
                {
                    if (!string.Equals(section.Name, "Update", StringComparison.OrdinalIgnoreCase)
                        || section.Value.ValueKind != JsonValueKind.Object)
                    {
                        continue;
                    }

                    foreach (JsonProperty option in section.Value.EnumerateObject())
                    {
                        if (string.Equals(option.Name, "RunMigrator", StringComparison.OrdinalIgnoreCase)
                            && option.Value.ValueKind == JsonValueKind.False)
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                // Unreadable config: let the migrator itself report the real problem.
            }

            return true;
        }

        private static MigrationOutcome RunMode(
            string migrator,
            string mode,
            string config,
            string logDirectory,
            string workingDirectory,
            Action<string> log,
            TimeSpan timeout,
            TimeSpan totalTimeout)
        {
            var startInfo = new ProcessStartInfo(migrator)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDirectory
            };
            startInfo.ArgumentList.Add("--mode");
            startInfo.ArgumentList.Add(mode);
            startInfo.ArgumentList.Add("--config");
            startInfo.ArgumentList.Add(config);
            startInfo.ArgumentList.Add("--log-dir");
            startInfo.ArgumentList.Add(logDirectory);

            var errors = new List<string>();
            log($"Menjalankan GLMigrator --mode {mode} dengan {config}");

            using var process = new Process { StartInfo = startInfo };
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is not null)
                {
                    log($"[migrator:{mode}] " + e.Data);
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is null)
                {
                    return;
                }

                log($"[migrator:{mode}] " + e.Data);
                lock (errors)
                {
                    errors.Add(e.Data);
                }
            };

            try
            {
                process.Start();
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
            {
                return new MigrationOutcome(MigrationStatus.Failed, $"GLMigrator tidak dapat dijalankan: {ex.Message}", logDirectory);
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException)
                {
                    // Already exited.
                }

                return new MigrationOutcome(
                    MigrationStatus.Failed,
                    $"Migrasi database (--mode {mode}) melebihi {totalTimeout.TotalMinutes:0} menit dan dihentikan.",
                    logDirectory);
            }

            // Flush the async output readers.
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                return new MigrationOutcome(MigrationStatus.Succeeded, LogDirectory: logDirectory);
            }

            string detail;
            lock (errors)
            {
                detail = string.Join(Environment.NewLine, errors.TakeLast(MaxErrorLines));
            }

            return new MigrationOutcome(
                MigrationStatus.Failed,
                string.IsNullOrWhiteSpace(detail) ? $"GLMigrator --mode {mode} keluar dengan kode {process.ExitCode}." : detail,
                logDirectory);
        }
    }
}
