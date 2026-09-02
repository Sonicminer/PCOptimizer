using System.Diagnostics;
using System.Text.Json;
using UltimateWindowsOptimizer.Update.Security;
using UltimateWindowsOptimizer.Update.Services;

namespace PCOptimizer.Updater;

/// <summary>
/// Standalone updater process.
/// Started by the main app after user confirms an update.
/// Flow: download → verify hash → backup → replace → restart main app → cleanup.
/// Single-instance enforced via named mutex.
/// </summary>
internal static class Program
{
    private const string MutexName = "Global\\PCOptimizer.Updater.SingleInstance";
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private static int Main(string[] args)
    {
        // Single instance
        using var mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            Console.WriteLine("Updater is already running.");
            return 1;
        }

        Console.WriteLine("PCOptimizer Updater");
        Console.WriteLine("===================");

        string? jobPath = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--job" && i + 1 < args.Length)
                jobPath = args[++i];
        }

        if (string.IsNullOrEmpty(jobPath) || !File.Exists(jobPath))
        {
            Console.WriteLine("Usage: PCOptimizer.Updater.exe --job <path-to-update_job.json>");
            return 2;
        }

        try
        {
            return RunUpdate(jobPath).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL: {ex.Message}");
            return 99;
        }
    }

    private static async Task<int> RunUpdate(string jobPath)
    {
        var json = await File.ReadAllTextAsync(jobPath).ConfigureAwait(false);
        var job = JsonSerializer.Deserialize<UpdateJob>(json, JsonOpts)
                  ?? throw new InvalidOperationException("Invalid job file.");

        Console.WriteLine($"Target version : {job.TargetVersion}");
        Console.WriteLine($"Install dir    : {job.InstallDir}");
        Console.WriteLine();

        UpdatePaths.EnsureDirectories();
        var downloadPath = Path.Combine(UpdatePaths.UpdateCacheDir, $"update_{job.TargetVersion}.zip");
        var backupDir = Path.Combine(UpdatePaths.BackupsDir, $"pre_{job.TargetVersion}_{DateTime.Now:yyyyMMdd_HHmmss}");

        // 1. Wait for main process to exit
        Console.WriteLine("Waiting for main application to exit...");
        await WaitForMainExitAsync(job.MainExe, TimeSpan.FromSeconds(30)).ConfigureAwait(false);

        // 2. Download
        Console.WriteLine("Downloading update...");
        try
        {
            await DownloadWithProgressAsync(job.DownloadUrl, downloadPath).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Download failed: {ex.Message}");
            return 10;
        }

        // 3. Verify SHA-256
        Console.WriteLine("Verifying integrity (SHA-256)...");
        if (!await HashVerifier.VerifyFileSha256Async(downloadPath, job.Sha256).ConfigureAwait(false))
        {
            Console.WriteLine("UPDATE VERIFICATION FAILED.");
            Console.WriteLine("The update was not installed.");
            TryDelete(downloadPath);
            return 20;
        }
        Console.WriteLine("✓ Integrity verified");

        // Signature check is optional – if present, would verify here.
        // For now we only enforce SHA-256.
        if (!string.IsNullOrEmpty(job.Signature))
            Console.WriteLine("✓ Signature present (verification deferred to signing infrastructure)");

        // 4. Backup current installation
        Console.WriteLine("Creating backup...");
        try
        {
            Directory.CreateDirectory(backupDir);
            CopyDirectory(job.InstallDir, backupDir, exclude: new[] { "UpdateCache", "Logs", "Backups" });
            Console.WriteLine($"Backup: {backupDir}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Backup failed: {ex.Message}");
            return 30;
        }

        // 5. Extract / install
        Console.WriteLine("Installing...");
        try
        {
            // Expect a zip package containing the new binaries
            if (downloadPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(downloadPath, job.InstallDir, overwriteFiles: true);
            }
            else
            {
                // Single-file replacement
                var target = Path.Combine(job.InstallDir, UpdatePaths.MainExeName);
                File.Copy(downloadPath, target, overwrite: true);
            }
            Console.WriteLine("✓ Installation complete");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Install failed: {ex.Message}");
            Console.WriteLine("Rolling back...");
            try
            {
                CopyDirectory(backupDir, job.InstallDir, exclude: Array.Empty<string>());
                Console.WriteLine("✓ Previous version restored");
            }
            catch (Exception rbEx)
            {
                Console.WriteLine($"Rollback failed: {rbEx.Message}");
            }
            return 40;
        }

        // 6. Cleanup download
        TryDelete(downloadPath);
        TryDelete(jobPath);

        // 7. Restart main app
        Console.WriteLine("Starting PCOptimizer...");
        var mainExe = Path.Combine(job.InstallDir, UpdatePaths.MainExeName);
        if (File.Exists(mainExe))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = mainExe,
                UseShellExecute = true,
                WorkingDirectory = job.InstallDir
            });
        }

        Console.WriteLine("Done.");
        return 0;
    }

    private static async Task WaitForMainExitAsync(string mainExePath, TimeSpan timeout)
    {
        var name = Path.GetFileNameWithoutExtension(mainExePath);
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            var stillRunning = Process.GetProcessesByName(name).Any();
            if (!stillRunning) return;
            await Task.Delay(500).ConfigureAwait(false);
        }
        // Force-kill if still running after timeout
        foreach (var p in Process.GetProcessesByName(name))
        {
            try { p.Kill(entireProcessTree: true); } catch { /* ignore */ }
        }
        await Task.Delay(1000).ConfigureAwait(false);
    }

    private static async Task DownloadWithProgressAsync(string url, string destPath)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("PCOptimizer-Updater/1.0");

        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength ?? -1L;
        await using var remote = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        await using var local = File.Create(destPath);

        var buffer = new byte[81920];
        long read = 0;
        int n;
        while ((n = await remote.ReadAsync(buffer).ConfigureAwait(false)) > 0)
        {
            await local.WriteAsync(buffer.AsMemory(0, n)).ConfigureAwait(false);
            read += n;
            if (total > 0)
            {
                var pct = (int)(read * 100 / total);
                Console.Write($"\rDownloading... {pct}%  ({read / 1024 / 1024} MB / {total / 1024 / 1024} MB)");
            }
        }
        Console.WriteLine();
    }

    private static void CopyDirectory(string source, string dest, string[] exclude)
    {
        Directory.CreateDirectory(dest);
        foreach (var file in Directory.GetFiles(source))
        {
            var name = Path.GetFileName(file);
            File.Copy(file, Path.Combine(dest, name), overwrite: true);
        }
        foreach (var dir in Directory.GetDirectories(source))
        {
            var name = Path.GetFileName(dir);
            if (exclude.Any(e => name.Equals(e, StringComparison.OrdinalIgnoreCase)))
                continue;
            CopyDirectory(dir, Path.Combine(dest, name), exclude);
        }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* ignore */ }
    }

    private sealed class UpdateJob
    {
        public string TargetVersion { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public string Sha256 { get; set; } = "";
        public string? Signature { get; set; }
        public string MainExe { get; set; } = "";
        public string InstallDir { get; set; } = "";
        public string? ReleaseNotes { get; set; }
    }
}
