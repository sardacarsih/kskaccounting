using System;
using System.IO;
using System.Text.Json.Serialization;

namespace Accounting.Utilities.Update
{
    /// <summary>
    /// Content of <c>latest.json</c> on the update server. See docs/auto-update.md for the contract.
    /// </summary>
    public sealed class UpdateManifest
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("mandatory")]
        public bool Mandatory { get; set; }

        [JsonPropertyName("minimumVersion")]
        public string? MinimumVersion { get; set; }

        [JsonPropertyName("packageUrl")]
        public string PackageUrl { get; set; } = string.Empty;

        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long? Size { get; set; }

        [JsonPropertyName("releaseDate")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }

    public enum UpdateCheckStatus
    {
        Disabled,
        UpToDate,
        Available,
        Failed
    }

    public sealed record UpdateCheckResult(
        UpdateCheckStatus Status,
        Version CurrentVersion,
        Version? LatestVersion = null,
        UpdateManifest? Manifest = null,
        Uri? PackageUri = null,
        bool IsMandatory = false,
        string? Error = null);
}
