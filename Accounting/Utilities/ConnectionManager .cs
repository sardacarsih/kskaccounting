using Accounting;
using DevExpress.XtraEditors;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Accounting.Utilities
{
    public class ConnectionManager
    {
        private const string ActiveServerKeyEnvironmentVariable = "ACCOUNTING_DB_ACTIVE_SERVER_KEY";
        private const string HostEnvironmentVariable = "ACCOUNTING_DB_HOST";
        private const string PortEnvironmentVariable = "ACCOUNTING_DB_PORT";
        private const string ServiceNameEnvironmentVariable = "ACCOUNTING_DB_SERVICE_NAME";
        private const string UserIdEnvironmentVariable = "ACCOUNTING_DB_USER_ID";
        private const string PasswordEnvironmentVariable = "ACCOUNTING_DB_PASSWORD";

        public static string GetOracleConnection()
        {
            ResolvedConnectionSettings? settings = LoadConfiguration();
            if (settings == null)
            {
                return string.Empty;
            }

            return BuildConnectionString(
                settings.Host,
                settings.Port,
                settings.UserId,
                settings.Password,
                settings.ServiceName);
        }

        private static ResolvedConnectionSettings? LoadConfiguration()
        {
            try
            {
                string configFilePath = Path.Combine(AppContext.BaseDirectory, "Utilities", "config.json");
                if (!File.Exists(configFilePath))
                {
                    ShowMessage($"File konfigurasi tidak ditemukan: {configFilePath}", "Info", MessageBoxIcon.Information);
                    return null;
                }

                string[] lines = File.ReadAllLines(configFilePath);
                string json = string.Join(
                    Environment.NewLine,
                    lines.Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));

                AppConfig? config = JsonSerializer.Deserialize<AppConfig>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (config == null)
                {
                    ShowMessage("Konfigurasi server tidak dapat dibaca.", "Error", MessageBoxIcon.Error);
                    return null;
                }

                string activeServerKey = ResolveActiveServerKey(config);
                bool hasConfiguredServer = TryGetConfiguredServer(config, activeServerKey, out OracleServerConfig? configuredServer);
                OracleServerConfig resolvedServer = CloneServerConfig(configuredServer);

                if (config.AllowEnvironmentFallback)
                {
                    ApplyEnvironmentFallback(resolvedServer);
                }

                if (!IsServerConfigComplete(resolvedServer, out string missingFields))
                {
                    string message = hasConfiguredServer
                        ? $"Konfigurasi server '{activeServerKey}' tidak lengkap. Field yang wajib diisi: {missingFields}."
                        : config.AllowEnvironmentFallback
                            ? $"Server aktif '{activeServerKey}' tidak ditemukan pada config dan fallback environment tidak lengkap. Field yang wajib diisi: {missingFields}."
                            : $"Server aktif '{activeServerKey}' tidak ditemukan pada config.json.";

                    ShowMessage(message, "Error", MessageBoxIcon.Error);
                    return null;
                }

                return new ResolvedConnectionSettings(
                    activeServerKey,
                    resolvedServer.Host,
                    resolvedServer.Port,
                    resolvedServer.ServiceName,
                    resolvedServer.UserId,
                    resolvedServer.Password);
            }
            catch (JsonException ex)
            {
                ShowMessage($"Format config.json tidak valid: {ex.Message}", "Error", MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                ShowMessage($"Error loading configuration: {ex.Message}", "Error", MessageBoxIcon.Error);
                return null;
            }
        }

        private static string ResolveActiveServerKey(AppConfig config)
        {
            if (config.AllowEnvironmentFallback)
            {
                string environmentActiveServerKey = GetEnvironmentValue(ActiveServerKeyEnvironmentVariable);
                if (!string.IsNullOrWhiteSpace(environmentActiveServerKey))
                {
                    return environmentActiveServerKey;
                }
            }

            return config.ActiveServerKey?.Trim() ?? string.Empty;
        }

        private static bool TryGetConfiguredServer(AppConfig config, string activeServerKey, out OracleServerConfig? serverConfig)
        {
            serverConfig = null;

            if (string.IsNullOrWhiteSpace(activeServerKey) || config.Servers == null)
            {
                return false;
            }

            return config.Servers.TryGetValue(activeServerKey, out serverConfig);
        }

        private static OracleServerConfig CloneServerConfig(OracleServerConfig? serverConfig)
        {
            if (serverConfig == null)
            {
                return new OracleServerConfig();
            }

            return new OracleServerConfig
            {
                Host = serverConfig.Host?.Trim() ?? string.Empty,
                Port = serverConfig.Port,
                ServiceName = serverConfig.ServiceName?.Trim() ?? string.Empty,
                UserId = serverConfig.UserId?.Trim() ?? string.Empty,
                Password = serverConfig.Password ?? string.Empty
            };
        }

        private static void ApplyEnvironmentFallback(OracleServerConfig serverConfig)
        {
            serverConfig.Host = GetFallbackValue(serverConfig.Host, HostEnvironmentVariable);
            serverConfig.ServiceName = GetFallbackValue(serverConfig.ServiceName, ServiceNameEnvironmentVariable);
            serverConfig.UserId = GetFallbackValue(serverConfig.UserId, UserIdEnvironmentVariable);
            serverConfig.Password = GetFallbackValue(serverConfig.Password, PasswordEnvironmentVariable);

            if (serverConfig.Port <= 0)
            {
                string rawPort = GetEnvironmentValue(PortEnvironmentVariable);
                if (int.TryParse(rawPort, out int port) && port > 0)
                {
                    serverConfig.Port = port;
                }
            }
        }

        private static string GetFallbackValue(string currentValue, string environmentVariableName)
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

        private static bool IsServerConfigComplete(OracleServerConfig serverConfig, out string missingFields)
        {
            string[] missing = new[]
            {
                string.IsNullOrWhiteSpace(serverConfig.Host) ? "Host" : null,
                serverConfig.Port <= 0 ? "Port" : null,
                string.IsNullOrWhiteSpace(serverConfig.ServiceName) ? "ServiceName" : null,
                string.IsNullOrWhiteSpace(serverConfig.UserId) ? "UserId" : null,
                string.IsNullOrWhiteSpace(serverConfig.Password) ? "Password" : null
            }
            .Where(value => value != null)
            .Cast<string>()
            .ToArray();

            missingFields = string.Join(", ", missing);
            return missing.Length == 0;
        }

        private static string BuildConnectionString(string host, int port, string userId, string password, string serviceName)
        {
            return $@"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST={host})(PORT={port}))(CONNECT_DATA=(SERVICE_NAME={serviceName})));User Id={userId};Password={password}";
        }

        private static void ShowMessage(string message, string caption, MessageBoxIcon icon)
        {
            XtraMessageBox.Show(message, caption, MessageBoxButtons.OK, icon);
        }

        private sealed record ResolvedConnectionSettings(
            string ActiveServerKey,
            string Host,
            int Port,
            string ServiceName,
            string UserId,
            string Password);
    }
}
