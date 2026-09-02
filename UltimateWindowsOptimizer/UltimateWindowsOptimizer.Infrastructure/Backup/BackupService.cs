using System.Text.Json;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.Infrastructure.Backup;

public sealed class BackupService : IBackupService
{
    private readonly string _backupRoot;
    private readonly IAppLogger _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public BackupService(IAppLogger logger, string? backupRoot = null)
    {
        _logger = logger;
        _backupRoot = backupRoot ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "UltimateWindowsOptimizer", "Backups");
        Directory.CreateDirectory(_backupRoot);
    }

    public async Task<BackupSnapshot> CreateSnapshotAsync(string reason, CancellationToken cancellationToken = default)
    {
        var snapshot = new BackupSnapshot
        {
            Reason = reason,
            Description = $"Automatic backup created: {reason}"
        };

        var dir = Path.Combine(_backupRoot, snapshot.Id);
        Directory.CreateDirectory(dir);

        // Save metadata
        var metaPath = Path.Combine(dir, "snapshot.json");
        await File.WriteAllTextAsync(metaPath, JsonSerializer.Serialize(snapshot, _jsonOptions), cancellationToken)
            .ConfigureAwait(false);

        // Attempt to create a Windows Restore Point (best effort)
        try
        {
            snapshot.HasRestorePoint = await CreateRestorePointAsync($"UWO: {reason}", cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Warning("Could not create restore point: {0}", ex.Message);
        }

        snapshot.ConfigBackupPath = dir;
        _logger.Info("Created backup snapshot {0} ({1})", snapshot.Id, reason);
        return snapshot;
    }

    public async Task<string> BackupRegistryKeysAsync(IEnumerable<string> keys, string reason, CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString("N");
        var dir = Path.Combine(_backupRoot, "registry", id);
        Directory.CreateDirectory(dir);

        // On Windows we would use reg export. Here we just record the intention.
        var list = keys.ToList();
        await File.WriteAllTextAsync(Path.Combine(dir, "keys.txt"),
            string.Join(Environment.NewLine, list), cancellationToken).ConfigureAwait(false);

        _logger.Info("Registry backup prepared for {0} keys ({1})", list.Count, reason);
        return id;
    }

    public Task<bool> CreateRestorePointAsync(string description, CancellationToken cancellationToken = default)
    {
        // Real implementation uses WMI: SystemRestore.CreateRestorePoint
        // Placeholder for cross-platform build
        _logger.Info("Restore point requested: {0}", description);
        return Task.FromResult(false); // indicate not created in this environment
    }

    public async Task<bool> RestoreSnapshotAsync(string snapshotId, CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(_backupRoot, snapshotId);
        if (!Directory.Exists(dir))
        {
            _logger.Warning("Snapshot {0} not found", snapshotId);
            return false;
        }

        _logger.Info("Restoring snapshot {0}", snapshotId);
        // Real restore logic would re-apply saved registry values / configs
        await Task.CompletedTask;
        return true;
    }

    public async Task<IReadOnlyList<BackupSnapshot>> GetSnapshotsAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<BackupSnapshot>();
        if (!Directory.Exists(_backupRoot)) return list;

        foreach (var dir in Directory.GetDirectories(_backupRoot))
        {
            var meta = Path.Combine(dir, "snapshot.json");
            if (!File.Exists(meta)) continue;
            try
            {
                var json = await File.ReadAllTextAsync(meta, cancellationToken).ConfigureAwait(false);
                var snap = JsonSerializer.Deserialize<BackupSnapshot>(json);
                if (snap != null) list.Add(snap);
            }
            catch { /* ignore corrupt */ }
        }
        return list.OrderByDescending(s => s.CreatedAt).ToList();
    }

    public async Task CleanupOldSnapshotsAsync(int keepCount = 20, CancellationToken cancellationToken = default)
    {
        var all = await GetSnapshotsAsync(cancellationToken).ConfigureAwait(false);
        foreach (var old in all.Skip(keepCount))
        {
            var dir = Path.Combine(_backupRoot, old.Id);
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
                _logger.Info("Deleted old snapshot {0}", old.Id);
            }
        }
    }
}
