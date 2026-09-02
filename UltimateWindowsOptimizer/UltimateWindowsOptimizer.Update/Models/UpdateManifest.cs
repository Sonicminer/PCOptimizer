using System.Text.Json.Serialization;

namespace UltimateWindowsOptimizer.Update.Models;

/// <summary>
/// Remote update manifest (latest.json / channel-specific).
/// Served over HTTPS from GitHub Releases or a custom update server.
/// </summary>
public sealed class UpdateManifest
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "0.0.0";

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = "stable";

    [JsonPropertyName("releaseDate")]
    public DateTimeOffset ReleaseDate { get; set; }

    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("sha256")]
    public string Sha256 { get; set; } = string.Empty;

    /// <summary>Optional detached signature (base64) for the package.</summary>
    [JsonPropertyName("signature")]
    public string? Signature { get; set; }

    [JsonPropertyName("releaseNotes")]
    public string ReleaseNotes { get; set; } = string.Empty;

    [JsonPropertyName("minimumWindowsVersion")]
    public string MinimumWindowsVersion { get; set; } = "10.0.19041";

    [JsonPropertyName("mandatory")]
    public bool Mandatory { get; set; }

    [JsonPropertyName("fileSizeBytes")]
    public long FileSizeBytes { get; set; }

    [JsonPropertyName("installerUrl")]
    public string? InstallerUrl { get; set; }

    [JsonPropertyName("installerSha256")]
    public string? InstallerSha256 { get; set; }
}

public enum UpdateChannel
{
    Stable,
    Beta,
    Nightly
}

public sealed class UpdateSettings
{
    public UpdateChannel Channel { get; set; } = UpdateChannel.Stable;
    public bool AutomaticallyCheckForUpdates { get; set; } = true;
    public bool NotifyAboutUpdates { get; set; } = true;
    public bool AutomaticallyDownloadUpdates { get; set; } = false;
    public bool AutomaticallyInstallUpdates { get; set; } = false;
    public DateTime? LastCheckUtc { get; set; }
    public string? SkippedVersion { get; set; }
}

public sealed class UpdateCheckResult
{
    public bool Success { get; set; }
    public bool UpdateAvailable { get; set; }
    public string CurrentVersion { get; set; } = string.Empty;
    public string? LatestVersion { get; set; }
    public UpdateManifest? Manifest { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsMandatory { get; set; }
}

public sealed class UpdateHistoryEntry
{
    public string Version { get; set; } = string.Empty;
    public DateTime InstalledAt { get; set; }
    public string Channel { get; set; } = "stable";
    public string Status { get; set; } = "Installed"; // Installed, Failed, RolledBack
}
