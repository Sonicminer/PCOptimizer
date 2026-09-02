using System.Text.Json;
using UltimateWindowsOptimizer.Update.Models;
using UltimateWindowsOptimizer.Update.Security;

namespace UltimateWindowsOptimizer.Update.Services;

/// <summary>
/// Used by the main application to check for updates and launch the external updater.
/// Does NOT download or replace files itself – that is the Updater's job.
/// </summary>
public sealed class UpdateService
{
    private readonly HttpClient _http;
    private readonly string _currentVersion;
    private readonly string _manifestBaseUrl;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <param name="currentVersion">Semantic version of the running app</param>
    /// <param name="manifestBaseUrl">
    /// Base URL without trailing slash, e.g.
    /// https://github.com/YourOrg/PCOptimizer/releases/latest/download
    /// or https://updates.example.com/pcoptimizer
    /// </param>
    public UpdateService(string currentVersion, string? manifestBaseUrl = null, HttpClient? httpClient = null)
    {
        _currentVersion = currentVersion;
        _manifestBaseUrl = (manifestBaseUrl ?? "https://github.com/YourOrg/PCOptimizer/releases/latest/download").TrimEnd('/');
        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("PCOptimizer-Updater/1.0");
    }

    public string CurrentVersion => _currentVersion;

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(UpdateChannel channel = UpdateChannel.Stable, CancellationToken ct = default)
    {
        var result = new UpdateCheckResult
        {
            CurrentVersion = _currentVersion,
            Success = false
        };

        try
        {
            var manifestName = channel switch
            {
                UpdateChannel.Beta => "latest-beta.json",
                UpdateChannel.Nightly => "latest-nightly.json",
                _ => "latest.json"
            };

            var url = $"{_manifestBaseUrl}/{manifestName}";
            using var response = await _http.GetAsync(url, ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                result.ErrorMessage = response.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? "No update information available for this channel."
                    : $"Server returned {(int)response.StatusCode}.";
                return result;
            }

            var json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            var manifest = JsonSerializer.Deserialize<UpdateManifest>(json, JsonOpts);

            if (manifest == null || !VersionComparer.IsValid(manifest.Version))
            {
                result.ErrorMessage = "Invalid update manifest.";
                return result;
            }

            result.Success = true;
            result.Manifest = manifest;
            result.LatestVersion = manifest.Version;
            result.IsMandatory = manifest.Mandatory;
            result.UpdateAvailable = VersionComparer.IsNewer(manifest.Version, _currentVersion);

            // Persist last check
            var settings = LoadSettings();
            settings.LastCheckUtc = DateTime.UtcNow;
            SaveSettings(settings);

            return result;
        }
        catch (HttpRequestException)
        {
            result.ErrorMessage = "No internet connection or server unreachable.";
            return result;
        }
        catch (TaskCanceledException)
        {
            result.ErrorMessage = "Update check timed out.";
            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Update check failed: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Launches the external updater and signals the main app to exit.
    /// </summary>
    public bool LaunchUpdater(UpdateManifest manifest, out string? error)
    {
        error = null;
        try
        {
            UpdatePaths.EnsureDirectories();

            // Write a small job file the updater will read
            var jobPath = Path.Combine(UpdatePaths.UpdateCacheDir, "update_job.json");
            var job = new
            {
                targetVersion = manifest.Version,
                downloadUrl = manifest.DownloadUrl,
                sha256 = manifest.Sha256,
                signature = manifest.Signature,
                mainExe = Path.Combine(UpdatePaths.InstallDir, UpdatePaths.MainExeName),
                installDir = UpdatePaths.InstallDir,
                releaseNotes = manifest.ReleaseNotes
            };
            File.WriteAllText(jobPath, JsonSerializer.Serialize(job, JsonOpts));

            var updaterPath = Path.Combine(UpdatePaths.InstallDir, UpdatePaths.UpdaterExeName);
            if (!File.Exists(updaterPath))
            {
                // Fallback: look next to current process
                updaterPath = Path.Combine(AppContext.BaseDirectory, UpdatePaths.UpdaterExeName);
            }

            if (!File.Exists(updaterPath))
            {
                error = "Updater executable not found.";
                return false;
            }

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = updaterPath,
                Arguments = $"--job \"{jobPath}\"",
                UseShellExecute = true,
                WorkingDirectory = UpdatePaths.InstallDir
            };

            System.Diagnostics.Process.Start(psi);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public UpdateSettings LoadSettings()
    {
        try
        {
            UpdatePaths.EnsureDirectories();
            if (File.Exists(UpdatePaths.UpdateSettingsFile))
            {
                var json = File.ReadAllText(UpdatePaths.UpdateSettingsFile);
                return JsonSerializer.Deserialize<UpdateSettings>(json, JsonOpts) ?? new UpdateSettings();
            }
        }
        catch { /* ignore */ }
        return new UpdateSettings();
    }

    public void SaveSettings(UpdateSettings settings)
    {
        try
        {
            UpdatePaths.EnsureDirectories();
            File.WriteAllText(UpdatePaths.UpdateSettingsFile,
                JsonSerializer.Serialize(settings, JsonOpts));
        }
        catch { /* ignore */ }
    }

    public IReadOnlyList<UpdateHistoryEntry> LoadHistory()
    {
        try
        {
            if (File.Exists(UpdatePaths.UpdateHistoryFile))
            {
                var json = File.ReadAllText(UpdatePaths.UpdateHistoryFile);
                return JsonSerializer.Deserialize<List<UpdateHistoryEntry>>(json, JsonOpts)
                       ?? new List<UpdateHistoryEntry>();
            }
        }
        catch { /* ignore */ }
        return Array.Empty<UpdateHistoryEntry>();
    }

    public void AddHistoryEntry(UpdateHistoryEntry entry)
    {
        try
        {
            UpdatePaths.EnsureDirectories();
            var list = LoadHistory().ToList();
            list.Insert(0, entry);
            // keep last 50
            if (list.Count > 50) list = list.Take(50).ToList();
            File.WriteAllText(UpdatePaths.UpdateHistoryFile,
                JsonSerializer.Serialize(list, JsonOpts));
        }
        catch { /* ignore */ }
    }
}
