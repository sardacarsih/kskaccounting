using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Accounting.Updater
{
    internal static class Program
    {
        private const int ErrorCancelled = 1223;
        private static readonly TimeSpan ExitTimeout = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan MigrationTimeout = TimeSpan.FromMinutes(30);
        private static readonly object LogLock = new();
        private static string? _logPath;

        [STAThread]
        private static int Main(string[] args)
        {
            UpdaterArguments arguments;
            try
            {
                arguments = UpdaterArguments.Parse(args);
            }
            catch (ArgumentException ex)
            {
                ShowError($"Argumen updater tidak valid.{Environment.NewLine}{ex.Message}");
                return 2;
            }

            _logPath = Path.Combine(arguments.TargetDirectory, "logs", $"updater-{DateTime.Now:yyyyMMdd}.log");
            Log($"Mulai update ke versi {arguments.Version} di {arguments.TargetDirectory}");

            if (!UpdateInstaller.WaitForProcessExit(arguments.ProcessId, ExitTimeout))
            {
                Log("Aplikasi tidak tertutup dalam batas waktu, update dibatalkan.");
                ShowError("Update dibatalkan karena aplikasi Accounting masih berjalan. Tutup aplikasi lalu coba lagi.");
                return 3;
            }

            if (!UpdateInstaller.CanWrite(arguments.TargetDirectory))
            {
                return arguments.Elevated ? FailNoAccess(arguments) : RelaunchElevated(arguments);
            }

            int exitCode = 0;
            string? errorMessage = null;
            try
            {
                StatusWindow.Run(arguments.Version, status =>
                {
                    (exitCode, errorMessage) = InstallAndMigrate(arguments, status);
                });
            }
            catch (Exception ex)
            {
                Log($"Update gagal: {ex}");
                exitCode = 4;
                errorMessage = $"Update gagal dipasang, versi lama tetap digunakan.{Environment.NewLine}{ex.Message}";
            }

            if (errorMessage is not null)
            {
                ShowError(errorMessage);
            }

            StartApplication(arguments);
            return exitCode;
        }

        private static (int ExitCode, string? Error) InstallAndMigrate(UpdaterArguments arguments, Action<string> status)
        {
            var installer = new UpdateInstaller(Log);

            status("Memasang file update...");
            InstallResult installed = installer.Install(arguments.ZipPath, arguments.TargetDirectory, CurrentVersionLabel(arguments));

            status("Memperbarui struktur database...");
            MigrationOutcome migration = DatabaseMigrationRunner.Run(arguments.TargetDirectory, Log, MigrationTimeout);
            if (migration.Status == MigrationStatus.Failed)
            {
                Log($"Migrasi database gagal: {migration.Error}");
                status("Migrasi gagal, mengembalikan versi lama...");
                installer.Rollback(installed);
                return (7,
                    "Migrasi database gagal, aplikasi dikembalikan ke versi sebelumnya." + Environment.NewLine + Environment.NewLine +
                    migration.Error + Environment.NewLine + Environment.NewLine +
                    $"Log: {migration.LogDirectory ?? Path.Combine(arguments.TargetDirectory, "logs")}" + Environment.NewLine +
                    "Sebagian perubahan database mungkin sudah diterapkan. Hubungi IT sebelum memakai modul yang terdampak.");
            }

            Log("Update selesai.");
            TryDelete(arguments.ZipPath);
            return (0, null);
        }

        private static int RelaunchElevated(UpdaterArguments arguments)
        {
            Log("Folder aplikasi tidak dapat ditulis, meminta hak administrator.");
            var startInfo = new ProcessStartInfo(Environment.ProcessPath!)
            {
                UseShellExecute = true,
                Verb = "runas"
            };
            // The original app has already exited; don't make the elevated copy wait on its PID.
            foreach (string argument in (arguments with { ProcessId = null }).ToArgumentList(elevated: true))
            {
                startInfo.ArgumentList.Add(argument);
            }

            try
            {
                Process.Start(startInfo);
                return 0;
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == ErrorCancelled)
            {
                Log("Permintaan hak administrator dibatalkan pengguna.");
                ShowError("Update membutuhkan hak administrator dan dibatalkan.");
                StartApplication(arguments);
                return 5;
            }
        }

        private static int FailNoAccess(UpdaterArguments arguments)
        {
            Log("Tetap tidak bisa menulis ke folder aplikasi meski sudah elevated.");
            ShowError($"Tidak bisa menulis ke folder aplikasi:{Environment.NewLine}{arguments.TargetDirectory}");
            StartApplication(arguments);
            return 6;
        }

        private static string CurrentVersionLabel(UpdaterArguments arguments)
        {
            string exePath = Path.Combine(arguments.TargetDirectory, arguments.ExeName);
            string? version = File.Exists(exePath) ? FileVersionInfo.GetVersionInfo(exePath).FileVersion : null;
            return string.IsNullOrWhiteSpace(version) ? "sebelum-" + arguments.Version : version;
        }

        private static void StartApplication(UpdaterArguments arguments)
        {
            string exePath = Path.Combine(arguments.TargetDirectory, arguments.ExeName);
            if (!File.Exists(exePath))
            {
                Log($"Tidak menemukan {exePath} untuk dijalankan ulang.");
                return;
            }

            try
            {
                // Started via the shell so an elevated updater does not hand admin rights to the app.
                Process.Start(new ProcessStartInfo("explorer.exe")
                {
                    ArgumentList = { exePath },
                    UseShellExecute = false,
                    WorkingDirectory = arguments.TargetDirectory
                });
            }
            catch (Exception ex)
            {
                Log($"Gagal menjalankan ulang aplikasi: {ex.Message}");
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Log($"Gagal menghapus paket {path}: {ex.Message}");
            }
        }

        private static void Log(string message)
        {
            if (_logPath is null)
            {
                return;
            }

            try
            {
                lock (LogLock)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
                    File.AppendAllText(_logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Logging must never block the update.
            }
        }

        private static void ShowError(string message)
        {
            string title = Assembly.GetExecutingAssembly().GetName().Name ?? "Accounting.Updater";
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
