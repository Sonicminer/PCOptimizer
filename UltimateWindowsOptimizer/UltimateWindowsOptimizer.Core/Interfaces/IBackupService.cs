using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.Core.Interfaces;

public interface IBackupService
{
    /// <summary>Create a full configuration snapshot before critical changes</summary>
    Task<BackupSnapshot> CreateSnapshotAsync(string reason, CancellationToken cancellationToken = default);

    /// <summary>Create a registry backup for specific keys</summary>
    Task<string> BackupRegistryKeysAsync(IEnumerable<string> keys, string reason, CancellationToken cancellationToken = default);

    /// <summary>Create a Windows System Restore Point</summary>
    Task<bool> CreateRestorePointAsync(string description, CancellationToken cancellationToken = default);

    /// <summary>Restore a previous snapshot</summary>
    Task<bool> RestoreSnapshotAsync(string snapshotId, CancellationToken cancellationToken = default);

    /// <summary>List all available snapshots</summary>
    Task<IReadOnlyList<BackupSnapshot>> GetSnapshotsAsync(CancellationToken cancellationToken = default);

    /// <summary>Delete old snapshots (keep last N)</summary>
    Task CleanupOldSnapshotsAsync(int keepCount = 20, CancellationToken cancellationToken = default);
}

public class BackupSnapshot
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> IncludedTweaks { get; set; } = new();
    public string? RegistryBackupPath { get; set; }
    public string? ConfigBackupPath { get; set; }
    public bool HasRestorePoint { get; set; }
    public long SizeBytes { get; set; }
}
