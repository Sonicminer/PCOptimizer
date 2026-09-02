using UltimateWindowsOptimizer.Core.Enums;

namespace UltimateWindowsOptimizer.Core.Models;

/// <summary>
/// Immutable record of every change made by the optimizer.
/// Powers the Change History and full Undo capability.
/// </summary>
public class ChangeRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TweakId { get; set; } = string.Empty;
    public string TweakName { get; set; } = string.Empty;
    public TweakCategory Category { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public ChangeResult Result { get; set; }
    public string? Message { get; set; }
    public string? BackupId { get; set; }
    public bool CanUndo { get; set; }
    public bool WasUndone { get; set; }
    public string? UserNote { get; set; }
    public string MachineName { get; set; } = Environment.MachineName;
}
