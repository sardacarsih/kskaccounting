using System;
using System.Configuration;
using System.IO;
using System.Text.Json;
using Serilog;

namespace Accounting.Utilities.Update
{
    /// <summary>
    /// Section <c>Update</c> in Utilities/config.json. When that section is absent, the shipped
    /// App.config key <c>Update:ManifestUrl</c> supplies the default so a fresh install checks for updates.
    /// </summary>
    public sealed class UpdateOptions
    {
        public bool Enabled { get; set; }
        public string ManifestUrl { get; set; } = string.Empty;
        public int CheckIntervalMinutes { get; set; } = 240;
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>Read by Accounting.Updater: set false on sites without sqlplus to skip GLMigrator.</summary>
        public bool RunMigrator { get; set; } = true;

        public static UpdateOptions Disabled => new();
    }

    internal static class UpdateOptionsLoader
    {
        internal const string ManifestUrlAppSettingKey = "Update:ManifestUrl";
        private const int MinimumIntervalMinutes = 15;

        public static UpdateOptions Load()
        {
            try
            {
                string configPath = Path.Combine(AppContext.BaseDirectory, "Utilities", "config.json");
                return Load(configPath, ConfigurationManager.AppSettings[ManifestUrlAppSettingKey]);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Konfigurasi update tidak dapat dibaca, fitur update dinonaktifkan.");
                return UpdateOptions.Disabled;
            }
        }

        /// <summary>Never throws: a missing or broken section only disables the update check.</summary>
        internal static UpdateOptions Load(string configPath, string? defaultManifestUrl = null)
        {
            try
            {
                UpdateOptions? options = null;
                if (File.Exists(configPath))
                {
                    ConfigRoot? root = JsonSerializer.Deserialize<ConfigRoot>(
                        File.ReadAllText(configPath),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            AllowTrailingCommas = true
                        });
                    options = root?.Update;
                }

                if (options is null && !string.IsNullOrWhiteSpace(defaultManifestUrl))
                {
                    options = new UpdateOptions { Enabled = true, ManifestUrl = defaultManifestUrl };
                }

                if (options is null || !options.Enabled || string.IsNullOrWhiteSpace(options.ManifestUrl))
                {
                    return UpdateOptions.Disabled;
                }

                options.ManifestUrl = options.ManifestUrl.Trim();
                options.CheckIntervalMinutes = Math.Max(options.CheckIntervalMinutes, MinimumIntervalMinutes);
                options.TimeoutSeconds = options.TimeoutSeconds <= 0 ? 30 : options.TimeoutSeconds;
                return options;
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
            {
                Log.Warning(ex, "Section Update di {ConfigPath} tidak valid, fitur update dinonaktifkan.", configPath);
                return UpdateOptions.Disabled;
            }
        }

        private sealed class ConfigRoot
        {
            public UpdateOptions? Update { get; set; }
        }
    }
}
