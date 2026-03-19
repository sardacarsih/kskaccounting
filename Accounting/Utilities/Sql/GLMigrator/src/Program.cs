using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GLMigrator.Cli;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            AppOptions options = AppOptions.Parse(args);
            if (options.ShowHelp)
            {
                Console.WriteLine(AppOptions.HelpText);
                return 0;
            }

            EnsureSqlPlusAvailable();

            Directory.CreateDirectory(options.LogDirectory);
            Manifest manifest = LoadManifest(options.ManifestPath);
            EnsureHistoryTable(options);

            switch (options.Mode)
            {
                case MigrationMode.Up:
                    ApplyUp(options, manifest);
                    break;
                case MigrationMode.Down:
                    ApplyDown(options, manifest);
                    break;
                case MigrationMode.Status:
                    PrintStatus(options, manifest);
                    break;
                case MigrationMode.Verify:
                    RunVerify(options, manifest);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported mode: {options.Mode}");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[ERROR] " + ex.Message);
            return 1;
        }
    }

    private static void ApplyUp(AppOptions options, Manifest manifest)
    {
        Dictionary<string, AppliedMigration> applied = GetAppliedMigrations(options);
        foreach (MigrationItem migration in manifest.Migrations.OrderBy(m => m.Order))
        {
            string scriptPath = ResolvePath(options.RootDirectory, migration.Script);
            string checksum = ComputeSha256(scriptPath);

            if (applied.TryGetValue(migration.Id, out AppliedMigration? existing))
            {
                if (!string.Equals(existing.Checksum, checksum, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Checksum mismatch for '{migration.Id}'. Applied={existing.Checksum}, Current={checksum}");
                }

                Console.WriteLine($"[SKIP] {migration.Id} already applied at {existing.AppliedAt}");
                continue;
            }

            Console.WriteLine($"[APPLY] {migration.Id} -> {migration.Script}");
            Stopwatch sw = Stopwatch.StartNew();
            ExecuteSqlFile(options, scriptPath, $"up_{migration.Id}");
            sw.Stop();

            RegisterMigration(options, migration, checksum, sw.ElapsedMilliseconds);
            Console.WriteLine($"[DONE]  {migration.Id} ({sw.ElapsedMilliseconds}ms)");
        }

        Console.WriteLine("[OK] Migration Up completed.");
    }

    private static void ApplyDown(AppOptions options, Manifest manifest)
    {
        Dictionary<string, AppliedMigration> applied = GetAppliedMigrations(options);
        List<MigrationItem> candidates = manifest.Migrations
            .Where(m => !string.IsNullOrWhiteSpace(m.RollbackScript) && applied.ContainsKey(m.Id))
            .OrderByDescending(m => m.Order)
            .Take(options.Steps)
            .ToList();

        if (candidates.Count == 0)
        {
            Console.WriteLine("[INFO] No applied rollbackable migrations.");
            return;
        }

        foreach (MigrationItem migration in candidates)
        {
            string rollbackPath = ResolvePath(options.RootDirectory, migration.RollbackScript!);
            Console.WriteLine($"[ROLLBACK] {migration.Id} -> {migration.RollbackScript}");
            ExecuteSqlFile(options, rollbackPath, $"down_{migration.Id}");
            RemoveMigrationHistory(options, migration.Id);
            Console.WriteLine($"[DONE]     {migration.Id}");
        }

        Console.WriteLine("[OK] Migration Down completed.");
    }

    private static void PrintStatus(AppOptions options, Manifest manifest)
    {
        Dictionary<string, AppliedMigration> applied = GetAppliedMigrations(options);
        foreach (MigrationItem migration in manifest.Migrations.OrderBy(m => m.Order))
        {
            if (applied.TryGetValue(migration.Id, out AppliedMigration? existing))
            {
                Console.WriteLine($"[APPLIED] {migration.Id} at {existing.AppliedAt}");
            }
            else
            {
                Console.WriteLine($"[PENDING] {migration.Id}");
            }
        }
    }

    private static void RunVerify(AppOptions options, Manifest manifest)
    {
        foreach (MigrationItem migration in manifest.Migrations.OrderBy(m => m.Order))
        {
            if (string.IsNullOrWhiteSpace(migration.CheckScript))
            {
                continue;
            }

            string checkPath = ResolvePath(options.RootDirectory, migration.CheckScript!);
            Console.WriteLine($"[VERIFY] {migration.Id} -> {migration.CheckScript}");
            ExecuteSqlFile(options, checkPath, $"verify_{migration.Id}");
        }

        Console.WriteLine("[OK] Verification scripts completed.");
    }

    private static void EnsureHistoryTable(AppOptions options)
    {
        Console.WriteLine("[INFO] Ensuring GL_MIGRATION_HISTORY exists...");
        ExecuteSqlFile(options, options.BootstrapScriptPath, "bootstrap");
    }

    private static Dictionary<string, AppliedMigration> GetAppliedMigrations(AppOptions options)
    {
        const string sql = """
SET HEADING OFF
SET FEEDBACK OFF
SET PAGESIZE 0
SET LINESIZE 32767
SET VERIFY OFF
SET ECHO OFF
SET TRIMSPOOL ON
SELECT MIGRATION_ID || '|' || CHECKSUM_SHA256 || '|' || TO_CHAR(APPLIED_AT, 'YYYY-MM-DD HH24:MI:SS')
  FROM GL_MIGRATION_HISTORY
 ORDER BY APPLIED_AT;
EXIT
""";

        SqlExecutionResult result = ExecuteSqlInline(options, sql, "status_query");
        string[] lines = result.Output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        Dictionary<string, AppliedMigration> map = new(StringComparer.OrdinalIgnoreCase);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (!line.Contains('|'))
            {
                continue;
            }

            string[] parts = line.Split('|');
            if (parts.Length < 3)
            {
                continue;
            }

            string migrationId = parts[0].Trim();
            string checksum = parts[1].Trim();
            string appliedAt = parts[2].Trim();

            if (string.IsNullOrWhiteSpace(migrationId))
            {
                continue;
            }

            map[migrationId] = new AppliedMigration(checksum, appliedAt);
        }

        return map;
    }

    private static void RegisterMigration(AppOptions options, MigrationItem migration, string checksum, long executionMs)
    {
        string migrationId = EscapeSqlLiteral(migration.Id);
        string migrationChecksum = EscapeSqlLiteral(checksum);
        string migrationDescription = EscapeSqlLiteral(migration.Description ?? migration.Id);
        string migrationScriptPath = EscapeSqlLiteral(migration.Script);

        string sql = $"""
INSERT INTO GL_MIGRATION_HISTORY
    (MIGRATION_ID, CHECKSUM_SHA256, DESCRIPTION, SCRIPT_PATH, STATUS, EXECUTION_MS)
VALUES
    ('{migrationId}', '{migrationChecksum}', '{migrationDescription}', '{migrationScriptPath}', 'SUCCESS', {executionMs});
COMMIT;
EXIT
""";

        ExecuteSqlInline(options, sql, $"register_{migration.Id}");
    }

    private static void RemoveMigrationHistory(AppOptions options, string migrationId)
    {
        string escapedId = EscapeSqlLiteral(migrationId);
        string sql = $"""
DELETE FROM GL_MIGRATION_HISTORY WHERE MIGRATION_ID = '{escapedId}';
COMMIT;
EXIT
""";

        ExecuteSqlInline(options, sql, $"unregister_{migrationId}");
    }

    private static SqlExecutionResult ExecuteSqlInline(AppOptions options, string sql, string logPrefix)
    {
        string tempSqlPath = Path.Combine(Path.GetTempPath(), $"glmigrator_inline_{Guid.NewGuid():N}.sql");
        File.WriteAllText(tempSqlPath, sql, Encoding.ASCII);
        try
        {
            return ExecuteSqlFile(options, tempSqlPath, logPrefix);
        }
        finally
        {
            TryDeleteFile(tempSqlPath);
        }
    }

    private static SqlExecutionResult ExecuteSqlFile(AppOptions options, string sqlPath, string logPrefix)
    {
        if (!File.Exists(sqlPath))
        {
            throw new FileNotFoundException("SQL file not found.", sqlPath);
        }

        string absoluteSqlPath = Path.GetFullPath(sqlPath).Replace('\\', '/');
        string wrapperSql = $$"""
WHENEVER OSERROR EXIT FAILURE ROLLBACK
WHENEVER SQLERROR EXIT FAILURE ROLLBACK
SET SERVEROUTPUT ON
SET DEFINE ON
@{{absoluteSqlPath}}
EXIT
""";

        string wrapperPath = Path.Combine(Path.GetTempPath(), $"glmigrator_wrapper_{Guid.NewGuid():N}.sql");
        File.WriteAllText(wrapperPath, wrapperSql, Encoding.ASCII);

        string logName = $"{SanitizeFileName(logPrefix)}_{DateTime.Now:yyyyMMdd_HHmmss}.log";
        string logPath = Path.Combine(options.LogDirectory, logName);

        try
        {
            ProcessStartInfo psi = new("sqlplus")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("-s");
            psi.ArgumentList.Add(options.Connection);
            psi.ArgumentList.Add("@" + wrapperPath);

            using Process process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start sqlplus process.");

            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            string output = string.IsNullOrWhiteSpace(stderr) ? stdout : stdout + Environment.NewLine + stderr;
            File.WriteAllText(logPath, output, Encoding.ASCII);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"SQL execution failed ({logPrefix}). See log: {logPath}");
            }

            return new SqlExecutionResult(output, logPath);
        }
        finally
        {
            TryDeleteFile(wrapperPath);
        }
    }

    private static Manifest LoadManifest(string manifestPath)
    {
        string fullPath = Path.GetFullPath(manifestPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Manifest not found.", fullPath);
        }

        string json = File.ReadAllText(fullPath, Encoding.UTF8);
        Manifest? manifest = JsonSerializer.Deserialize<Manifest>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (manifest is null)
        {
            throw new InvalidOperationException("Failed to parse manifest.");
        }

        manifest.Migrations ??= [];
        return manifest;
    }

    private static string ResolvePath(string rootDirectory, string path)
    {
        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        return Path.GetFullPath(Path.Combine(rootDirectory, path));
    }

    private static string ComputeSha256(string filePath)
    {
        using SHA256 sha = SHA256.Create();
        using FileStream stream = File.OpenRead(filePath);
        byte[] hash = sha.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string EscapeSqlLiteral(string text) => text.Replace("'", "''");

    private static string SanitizeFileName(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        StringBuilder sb = new(value.Length);
        foreach (char c in value)
        {
            sb.Append(invalid.Contains(c) ? '_' : c);
        }

        return sb.ToString();
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // best effort cleanup
        }
    }

    private static void EnsureSqlPlusAvailable()
    {
        try
        {
            ProcessStartInfo psi = new("sqlplus")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            psi.ArgumentList.Add("-v");

            using Process process = Process.Start(psi)
                ?? throw new InvalidOperationException("Unable to start sqlplus.");

            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException("sqlplus returned non-zero exit code. Ensure Oracle Client is installed.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("sqlplus command not found or unavailable in PATH.", ex);
        }
    }

    private sealed record AppliedMigration(string Checksum, string AppliedAt);
    private sealed record SqlExecutionResult(string Output, string LogPath);
}

internal enum MigrationMode
{
    Up,
    Down,
    Status,
    Verify
}

internal sealed class AppOptions
{
    public string Connection { get; private init; } = string.Empty;
    public MigrationMode Mode { get; private init; } = MigrationMode.Up;
    public int Steps { get; private init; } = 1;
    public string RootDirectory { get; private init; } = AppContext.BaseDirectory;
    public string ManifestPath { get; private init; } = string.Empty;
    public string BootstrapScriptPath { get; private init; } = string.Empty;
    public string LogDirectory { get; private init; } = string.Empty;
    public bool ShowHelp { get; private init; }

    public static string HelpText => """
GLMigrator.exe --connection <USER/PASS@//HOST:PORT/SERVICE> [--mode up|down|status|verify] [--steps N] [--root <folder>]

Options:
  --connection   Oracle SQL*Plus connection string (required unless --help)
  --mode         up (default), down, status, verify
  --steps        Number of steps for down mode (default: 1)
  --root         Root folder containing migrations.manifest.json and migrations/
  --manifest     Optional custom manifest path
  --bootstrap    Optional custom bootstrap sql path
  --log-dir      Optional custom log directory
  --help         Show usage
""";

    public static AppOptions Parse(string[] args)
    {
        Dictionary<string, string?> map = new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            string key = arg[2..];
            string? value = null;
            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                value = args[++i];
            }

            map[key] = value;
        }

        bool help = map.ContainsKey("help") || map.ContainsKey("h");
        string root = map.TryGetValue("root", out string? rootArg) && !string.IsNullOrWhiteSpace(rootArg)
            ? Path.GetFullPath(rootArg)
            : AppContext.BaseDirectory;

        string manifest = map.TryGetValue("manifest", out string? manifestArg) && !string.IsNullOrWhiteSpace(manifestArg)
            ? Path.GetFullPath(manifestArg)
            : Path.Combine(root, "migrations.manifest.json");

        string bootstrap = map.TryGetValue("bootstrap", out string? bootstrapArg) && !string.IsNullOrWhiteSpace(bootstrapArg)
            ? Path.GetFullPath(bootstrapArg)
            : Path.Combine(root, "000_bootstrap_gl_migration_history.sql");

        string logDir = map.TryGetValue("log-dir", out string? logArg) && !string.IsNullOrWhiteSpace(logArg)
            ? Path.GetFullPath(logArg)
            : Path.Combine(root, "logs");

        MigrationMode mode = MigrationMode.Up;
        if (map.TryGetValue("mode", out string? modeArg) && !string.IsNullOrWhiteSpace(modeArg))
        {
            if (!Enum.TryParse(modeArg, true, out mode))
            {
                throw new ArgumentException($"Invalid mode '{modeArg}'. Valid: up, down, status, verify.");
            }
        }

        int steps = 1;
        if (map.TryGetValue("steps", out string? stepsArg) && !string.IsNullOrWhiteSpace(stepsArg))
        {
            if (!int.TryParse(stepsArg, out steps) || steps < 1)
            {
                throw new ArgumentException("Invalid --steps value. Must be integer >= 1.");
            }
        }

        string connection = map.TryGetValue("connection", out string? connArg) && !string.IsNullOrWhiteSpace(connArg)
            ? connArg.Trim()
            : string.Empty;

        if (!help && string.IsNullOrWhiteSpace(connection))
        {
            throw new ArgumentException("Missing required --connection.");
        }

        return new AppOptions
        {
            Connection = connection,
            Mode = mode,
            Steps = steps,
            RootDirectory = root,
            ManifestPath = manifest,
            BootstrapScriptPath = bootstrap,
            LogDirectory = logDir,
            ShowHelp = help
        };
    }
}

internal sealed class Manifest
{
    public string PackageName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public List<MigrationItem> Migrations { get; set; } = [];
}

internal sealed class MigrationItem
{
    public string Id { get; set; } = string.Empty;
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Script { get; set; } = string.Empty;
    public string? RollbackScript { get; set; }
    public string? CheckScript { get; set; }
}
