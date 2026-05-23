using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GLMigrator.Cli;

internal static class Program
{
    private static readonly EmbeddedAssetStore AssetStore = EmbeddedAssetStore.Create();

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

            options.Connection = ConnectionResolver.Resolve(options);

            EnsureSqlPlusAvailable();

            Directory.CreateDirectory(options.LogDirectory);
            Manifest manifest = LoadManifest(options);
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
            AssetContent script = ResolveAsset(options, migration.Script);
            string checksum = ComputeSha256(script.Content);

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
            ExecuteSqlAsset(options, script, $"up_{migration.Id}");
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
            AssetContent rollback = ResolveAsset(options, migration.RollbackScript!);
            Console.WriteLine($"[ROLLBACK] {migration.Id} -> {migration.RollbackScript}");
            ExecuteSqlAsset(options, rollback, $"down_{migration.Id}");
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

            AssetContent check = ResolveAsset(options, migration.CheckScript!);
            Console.WriteLine($"[VERIFY] {migration.Id} -> {migration.CheckScript}");
            ExecuteSqlAsset(options, check, $"verify_{migration.Id}");
        }

        Console.WriteLine("[OK] Verification scripts completed.");
    }

    private static void EnsureHistoryTable(AppOptions options)
    {
        Console.WriteLine("[INFO] Ensuring GL_MIGRATION_HISTORY exists...");
        ExecuteSqlAsset(options, ResolveBootstrapAsset(options), "bootstrap");
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
        return ExecuteSqlAsset(options, AssetContent.FromText($"inline/{logPrefix}.sql", sql), logPrefix);
    }

    private static SqlExecutionResult ExecuteSqlAsset(AppOptions options, AssetContent asset, string logPrefix)
    {
        string tempFilePath = WriteAssetToTempFile(asset);
        try
        {
            return ExecuteSqlFile(options, tempFilePath, logPrefix);
        }
        finally
        {
            TryDeleteFile(tempFilePath);
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

    private static Manifest LoadManifest(AppOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ManifestPathOverride))
        {
            string fullPath = Path.GetFullPath(options.ManifestPathOverride);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Manifest not found.", fullPath);
            }

            string externalJson = File.ReadAllText(fullPath, Encoding.UTF8);
            return DeserializeManifest(externalJson, fullPath);
        }

        AssetContent manifestAsset = AssetStore.GetRequired("migrations.manifest.json");
        return DeserializeManifest(manifestAsset.ReadAsString(), manifestAsset.DisplayName);
    }

    private static Manifest DeserializeManifest(string json, string displayName)
    {
        Manifest? manifest = JsonSerializer.Deserialize<Manifest>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (manifest is null)
        {
            throw new InvalidOperationException($"Failed to parse manifest: {displayName}");
        }

        manifest.Migrations ??= [];
        return manifest;
    }

    private static AssetContent ResolveBootstrapAsset(AppOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.BootstrapScriptPathOverride))
        {
            string path = Path.GetFullPath(options.BootstrapScriptPathOverride);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Bootstrap SQL not found.", path);
            }

            return AssetContent.FromFile(path);
        }

        return AssetStore.GetRequired("000_bootstrap_gl_migration_history.sql");
    }

    private static AssetContent ResolveAsset(AppOptions options, string relativePath)
    {
        if (!string.IsNullOrWhiteSpace(options.RootDirectoryOverride))
        {
            string candidate = Path.GetFullPath(Path.Combine(options.RootDirectoryOverride, relativePath));
            if (!File.Exists(candidate))
            {
                throw new FileNotFoundException("Migration file not found.", candidate);
            }

            return AssetContent.FromFile(candidate);
        }

        return AssetStore.GetRequired(relativePath);
    }

    private static string WriteAssetToTempFile(AssetContent asset)
    {
        string extension = Path.GetExtension(asset.DisplayName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".sql";
        }

        string tempFilePath = Path.Combine(Path.GetTempPath(), $"glmigrator_asset_{Guid.NewGuid():N}{extension}");
        File.WriteAllBytes(tempFilePath, asset.Content);
        return tempFilePath;
    }

    private static string ComputeSha256(byte[] content)
    {
        byte[] hash = SHA256.HashData(content);
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

internal static class ConnectionResolver
{
    private const string ActiveServerKeyEnvironmentVariable = "ACCOUNTING_DB_ACTIVE_SERVER_KEY";
    private const string HostEnvironmentVariable = "ACCOUNTING_DB_HOST";
    private const string PortEnvironmentVariable = "ACCOUNTING_DB_PORT";
    private const string ServiceNameEnvironmentVariable = "ACCOUNTING_DB_SERVICE_NAME";
    private const string UserIdEnvironmentVariable = "ACCOUNTING_DB_USER_ID";
    private const string PasswordEnvironmentVariable = "ACCOUNTING_DB_PASSWORD";

    public static string Resolve(AppOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Connection))
        {
            return options.Connection.Trim();
        }

        string configPath = ResolveConfigPath(options.ConfigPathOverride);
        AppConfig config = LoadConfig(configPath);

        string activeServerKey = ResolveActiveServerKey(config, options.ServerKeyOverride);
        OracleServerConfig? serverConfig = ResolveServerConfig(config, activeServerKey);
        if (serverConfig is null && !config.AllowEnvironmentFallback)
        {
            throw new InvalidOperationException($"Server key '{activeServerKey}' was not found in config '{configPath}'.");
        }

        string dbHost = serverConfig?.Host?.Trim() ?? string.Empty;
        string dbServiceName = serverConfig?.ServiceName?.Trim() ?? string.Empty;
        string dbUserId = serverConfig?.UserId?.Trim() ?? string.Empty;
        string dbPassword = serverConfig?.Password ?? string.Empty;
        int dbPort = serverConfig?.Port ?? 0;

        if (config.AllowEnvironmentFallback)
        {
            dbHost = GetConfigValueOrFallback(dbHost, HostEnvironmentVariable);
            dbServiceName = GetConfigValueOrFallback(dbServiceName, ServiceNameEnvironmentVariable);
            dbUserId = GetConfigValueOrFallback(dbUserId, UserIdEnvironmentVariable);
            dbPassword = GetConfigValueOrFallback(dbPassword, PasswordEnvironmentVariable);

            if (dbPort <= 0)
            {
                string rawPort = GetEnvironmentValue(PortEnvironmentVariable);
                if (!string.IsNullOrWhiteSpace(rawPort))
                {
                    if (!int.TryParse(rawPort, out dbPort) || dbPort <= 0)
                    {
                        throw new InvalidOperationException($"Environment variable {PortEnvironmentVariable} is invalid: '{rawPort}'");
                    }
                }
            }
        }

        List<string> missingFields = [];
        if (string.IsNullOrWhiteSpace(dbHost)) { missingFields.Add("Host"); }
        if (dbPort <= 0) { missingFields.Add("Port"); }
        if (string.IsNullOrWhiteSpace(dbServiceName)) { missingFields.Add("ServiceName"); }
        if (string.IsNullOrWhiteSpace(dbUserId)) { missingFields.Add("UserId"); }
        if (string.IsNullOrWhiteSpace(dbPassword)) { missingFields.Add("Password"); }

        if (missingFields.Count > 0)
        {
            throw new InvalidOperationException(
                $"Server configuration '{activeServerKey}' is incomplete. Missing: {string.Join(", ", missingFields)}");
        }

        return $"{dbUserId}/{dbPassword}@//{dbHost}:{dbPort}/{dbServiceName}";
    }

    private static string ResolveConfigPath(string configPathOverride)
    {
        if (!string.IsNullOrWhiteSpace(configPathOverride))
        {
            string explicitPath = Path.GetFullPath(configPathOverride);
            if (!File.Exists(explicitPath))
            {
                throw new FileNotFoundException("Config file not found.", explicitPath);
            }

            return explicitPath;
        }

        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "config.json"),
            Path.Combine(AppContext.BaseDirectory, "Utilities", "config.json"),
            Path.Combine(AppContext.BaseDirectory, "Accounting", "Utilities", "config.json"),
            Path.Combine(Environment.CurrentDirectory, "config.json"),
            Path.Combine(Environment.CurrentDirectory, "Utilities", "config.json"),
            Path.Combine(Environment.CurrentDirectory, "Accounting", "Utilities", "config.json")
        ];

        foreach (string candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string fullPath = Path.GetFullPath(candidate);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        throw new FileNotFoundException(
            "No config.json file was found. Use --config <path> or --connection <USER/PASS@//HOST:PORT/SERVICE>.");
    }

    private static AppConfig LoadConfig(string configPath)
    {
        string json = string.Join(
            Environment.NewLine,
            File.ReadLines(configPath).Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));

        AppConfig? config = JsonSerializer.Deserialize<AppConfig>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return config ?? throw new InvalidOperationException($"Unable to parse config file '{configPath}'.");
    }

    private static string ResolveActiveServerKey(AppConfig config, string serverKeyOverride)
    {
        if (!string.IsNullOrWhiteSpace(serverKeyOverride))
        {
            return serverKeyOverride.Trim();
        }

        if (config.AllowEnvironmentFallback)
        {
            string environmentServerKey = GetEnvironmentValue(ActiveServerKeyEnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(environmentServerKey))
            {
                return environmentServerKey;
            }
        }

        if (!string.IsNullOrWhiteSpace(config.ActiveServerKey))
        {
            return config.ActiveServerKey.Trim();
        }

        throw new InvalidOperationException("ActiveServerKey is missing in config and no --server-key override was provided.");
    }

    private static OracleServerConfig? ResolveServerConfig(AppConfig config, string activeServerKey)
    {
        if (config.Servers is null)
        {
            return null;
        }

        foreach ((string key, OracleServerConfig value) in config.Servers)
        {
            if (string.Equals(key, activeServerKey, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        return null;
    }

    private static string GetConfigValueOrFallback(string currentValue, string environmentVariableName)
    {
        if (!string.IsNullOrWhiteSpace(currentValue))
        {
            return currentValue.Trim();
        }

        return GetEnvironmentValue(environmentVariableName);
    }

    private static string GetEnvironmentValue(string environmentVariableName)
    {
        return Environment.GetEnvironmentVariable(environmentVariableName)?.Trim() ?? string.Empty;
    }
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
    public string Connection { get; set; } = string.Empty;
    public MigrationMode Mode { get; private init; } = MigrationMode.Up;
    public int Steps { get; private init; } = 1;
    public string RootDirectoryOverride { get; private init; } = string.Empty;
    public string ManifestPathOverride { get; private init; } = string.Empty;
    public string BootstrapScriptPathOverride { get; private init; } = string.Empty;
    public string LogDirectory { get; private init; } = string.Empty;
    public string ConfigPathOverride { get; private init; } = string.Empty;
    public string ServerKeyOverride { get; private init; } = string.Empty;
    public bool ShowHelp { get; private init; }

    public static string HelpText => """
GLMigrator.exe [--connection <USER/PASS@//HOST:PORT/SERVICE>] [--config <path>] [--server-key KEY] [--mode up|down|status|verify] [--steps N]

Options:
  --connection   Oracle SQL*Plus connection string. If omitted, the executable reads config.json.
  --config       Optional config.json path. If omitted, the executable probes common config.json locations.
  --server-key   Optional server key override for config.json resolution
  --mode         up (default), down, status, verify
  --steps        Number of steps for down mode (default: 1)
  --root         Optional external root folder to override embedded migrations
  --manifest     Optional custom manifest path to override embedded manifest
  --bootstrap    Optional custom bootstrap sql path to override embedded bootstrap
  --log-dir      Optional custom log directory
  --help         Show usage

Default behavior:
  The executable uses embedded manifest/bootstrap/migration SQL resources.
  If --connection is omitted, it resolves the Oracle connection from config.json.
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
            : string.Empty;

        string manifest = map.TryGetValue("manifest", out string? manifestArg) && !string.IsNullOrWhiteSpace(manifestArg)
            ? Path.GetFullPath(manifestArg)
            : string.Empty;

        string bootstrap = map.TryGetValue("bootstrap", out string? bootstrapArg) && !string.IsNullOrWhiteSpace(bootstrapArg)
            ? Path.GetFullPath(bootstrapArg)
            : string.Empty;

        string logDir = map.TryGetValue("log-dir", out string? logArg) && !string.IsNullOrWhiteSpace(logArg)
            ? Path.GetFullPath(logArg)
            : Path.Combine(AppContext.BaseDirectory, "logs");

        string config = map.TryGetValue("config", out string? configArg) && !string.IsNullOrWhiteSpace(configArg)
            ? Path.GetFullPath(configArg)
            : string.Empty;

        string serverKey = map.TryGetValue("server-key", out string? serverKeyArg) && !string.IsNullOrWhiteSpace(serverKeyArg)
            ? serverKeyArg.Trim()
            : string.Empty;

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

        return new AppOptions
        {
            Connection = connection,
            Mode = mode,
            Steps = steps,
            RootDirectoryOverride = root,
            ManifestPathOverride = manifest,
            BootstrapScriptPathOverride = bootstrap,
            LogDirectory = logDir,
            ConfigPathOverride = config,
            ServerKeyOverride = serverKey,
            ShowHelp = help
        };
    }
}

internal sealed class AppConfig
{
    public string ActiveServerKey { get; set; } = string.Empty;
    public bool AllowEnvironmentFallback { get; set; }
    public Dictionary<string, OracleServerConfig> Servers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class OracleServerConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 1521;
    public string ServiceName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
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

internal sealed class EmbeddedAssetStore
{
    private const string ResourcePrefix = "EmbeddedAssets/";
    private readonly Dictionary<string, AssetContent> assets;

    private EmbeddedAssetStore(Dictionary<string, AssetContent> assets)
    {
        this.assets = assets;
    }

    public static EmbeddedAssetStore Create()
    {
        Assembly assembly = typeof(Program).Assembly;
        Dictionary<string, AssetContent> assets = new(StringComparer.OrdinalIgnoreCase);

        foreach (string resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            {
                continue;
            }

            using Stream stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded resource not found: {resourceName}");
            using MemoryStream ms = new();
            stream.CopyTo(ms);

            string logicalPath = resourceName[ResourcePrefix.Length..];
            logicalPath = logicalPath.Replace('\\', '/');
            assets[logicalPath] = new AssetContent(logicalPath, ms.ToArray());
        }

        return new EmbeddedAssetStore(assets);
    }

    public AssetContent GetRequired(string logicalPath)
    {
        string normalized = logicalPath.Replace('\\', '/');
        if (assets.TryGetValue(normalized, out AssetContent? asset))
        {
            return asset;
        }

        throw new FileNotFoundException("Embedded asset not found.", normalized);
    }
}

internal sealed class AssetContent
{
    public AssetContent(string displayName, byte[] content)
    {
        DisplayName = displayName;
        Content = content;
    }

    public string DisplayName { get; }
    public byte[] Content { get; }

    public static AssetContent FromFile(string path)
    {
        return new AssetContent(path, File.ReadAllBytes(path));
    }

    public static AssetContent FromText(string displayName, string text)
    {
        return new AssetContent(displayName, Encoding.ASCII.GetBytes(text));
    }

    public string ReadAsString()
    {
        return Encoding.UTF8.GetString(Content);
    }
}
