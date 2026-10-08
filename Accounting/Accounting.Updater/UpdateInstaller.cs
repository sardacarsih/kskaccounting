using System.Diagnostics;
using System.IO.Compression;

namespace Accounting.Updater
{
    /// <summary>
    /// Replaces the application files in a target folder with the content of an update package.
    /// Site-specific files (connection config, logs) are never touched, and any failure restores
    /// the files that were already overwritten.
    /// </summary>
    internal sealed class UpdateInstaller
    {
        internal const string StagingFolderName = "_update_staging";
        internal const string BackupFolderName = "_update_backup";
        private const int BackupsToKeep = 2;

        private static readonly string[] PreservedFiles =
        {
            "Utilities/config.json",
            "config.json"
        };

        private static readonly string[] PreservedFolders =
        {
            "logs/",
            StagingFolderName + "/",
            BackupFolderName + "/"
        };

        private readonly Action<string> _log;

        public UpdateInstaller(Action<string> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public static bool WaitForProcessExit(int? processId, TimeSpan timeout)
        {
            if (!processId.HasValue)
            {
                return true;
            }

            try
            {
                using Process process = Process.GetProcessById(processId.Value);
                return process.WaitForExit((int)timeout.TotalMilliseconds);
            }
            catch (ArgumentException)
            {
                // Process already exited.
                return true;
            }
        }

        public static bool CanWrite(string directory)
        {
            try
            {
                string probe = Path.Combine(directory, $".update_probe_{Guid.NewGuid():N}");
                File.WriteAllText(probe, string.Empty);
                File.Delete(probe);
                return true;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                return false;
            }
        }

        internal static bool IsPreserved(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/').TrimStart('/');

            foreach (string file in PreservedFiles)
            {
                if (string.Equals(normalized, file, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            foreach (string folder in PreservedFolders)
            {
                if (normalized.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Installs the package. The returned result can undo the install later, e.g. when the
        /// database migration that follows it fails.
        /// </summary>
        public InstallResult Install(string zipPath, string targetDirectory, string backupLabel)
        {
            if (!File.Exists(zipPath))
            {
                throw new FileNotFoundException("Paket update tidak ditemukan.", zipPath);
            }

            string target = Path.GetFullPath(targetDirectory);
            string staging = Path.Combine(target, StagingFolderName);
            string backup = Path.Combine(
                target,
                BackupFolderName,
                $"{Sanitize(backupLabel)}-{DateTime.Now:yyyyMMdd-HHmmss}");

            DeleteDirectory(staging);
            ExtractSafely(zipPath, staging);

            var backedUp = new List<(string Destination, string BackupCopy)>();
            var created = new List<string>();
            int written = 0;

            try
            {
                foreach (string source in Directory.EnumerateFiles(staging, "*", SearchOption.AllDirectories))
                {
                    string relative = Path.GetRelativePath(staging, source);
                    if (IsPreserved(relative))
                    {
                        _log($"Lewati file terlindungi: {relative}");
                        continue;
                    }

                    string destination = Path.Combine(target, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

                    if (File.Exists(destination))
                    {
                        string backupCopy = Path.Combine(backup, relative);
                        Directory.CreateDirectory(Path.GetDirectoryName(backupCopy)!);
                        File.Copy(destination, backupCopy, overwrite: true);
                        backedUp.Add((destination, backupCopy));
                    }
                    else
                    {
                        created.Add(destination);
                    }

                    File.Copy(source, destination, overwrite: true);
                    written++;
                }
            }
            catch (Exception ex)
            {
                _log($"Instalasi gagal, mengembalikan file lama: {ex.Message}");
                Rollback(backedUp, created);
                throw;
            }
            finally
            {
                DeleteDirectory(staging);
            }

            _log($"{written} file diperbarui, {backedUp.Count} file lama dicadangkan di {backup}");
            PruneBackups(Path.Combine(target, BackupFolderName));
            return new InstallResult(written, backedUp, created);
        }

        /// <summary>Restores the files replaced by <paramref name="result"/> and removes the ones it added.</summary>
        public void Rollback(InstallResult result)
        {
            _log("Mengembalikan file aplikasi ke versi sebelumnya.");
            Rollback(result.BackedUp.ToList(), result.Created.ToList());
        }

        internal static void ExtractSafely(string zipPath, string destination)
        {
            string root = Path.GetFullPath(destination);
            string rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(root);

            using ZipArchive archive = ZipFile.OpenRead(zipPath);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string fullPath = Path.GetFullPath(Path.Combine(root, entry.FullName));
                if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException($"Entry paket tidak aman: {entry.FullName}");
                }

                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(fullPath);
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                entry.ExtractToFile(fullPath, overwrite: true);
            }
        }

        private void Rollback(List<(string Destination, string BackupCopy)> backedUp, List<string> created)
        {
            foreach ((string destination, string backupCopy) in backedUp)
            {
                try
                {
                    File.Copy(backupCopy, destination, overwrite: true);
                }
                catch (Exception ex)
                {
                    _log($"Gagal mengembalikan {destination}: {ex.Message}");
                }
            }

            foreach (string path in created)
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    _log($"Gagal menghapus file baru {path}: {ex.Message}");
                }
            }
        }

        private void PruneBackups(string backupRoot)
        {
            if (!Directory.Exists(backupRoot))
            {
                return;
            }

            foreach (DirectoryInfo old in new DirectoryInfo(backupRoot)
                         .GetDirectories()
                         .OrderByDescending(d => d.CreationTimeUtc)
                         .Skip(BackupsToKeep))
            {
                try
                {
                    old.Delete(recursive: true);
                }
                catch (Exception ex)
                {
                    _log($"Gagal menghapus backup lama {old.FullName}: {ex.Message}");
                }
            }
        }

        private static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }

        private static string Sanitize(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return "backup";
            }

            char[] invalid = Path.GetInvalidFileNameChars();
            return new string(label.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        }
    }

    internal sealed record InstallResult(
        int FilesWritten,
        IReadOnlyList<(string Destination, string BackupCopy)> BackedUp,
        IReadOnlyList<string> Created);
}
