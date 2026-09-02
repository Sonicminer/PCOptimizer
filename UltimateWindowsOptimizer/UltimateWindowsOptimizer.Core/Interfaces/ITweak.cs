using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.Core.Interfaces;

/// <summary>
/// Core interface for every optimization/tweak in the system.
/// New tweaks are added by implementing this interface - no core changes required.
/// This enables easy scaling to 100-200+ tweaks.
/// </summary>
public interface ITweak
{
    /// <summary>Unique stable identifier (e.g. "perf.disable_visual_effects")</summary>
    string Id { get; }

    /// <summary>Display name (localized)</summary>
    string Name { get; }

    /// <summary>Category for grouping in UI</summary>
    TweakCategory Category { get; }

    /// <summary>Short description of what this does</summary>
    string Description { get; }

    /// <summary>Detailed explanation: what, why, expected effect</summary>
    string DetailedExplanation { get; }

    /// <summary>Risk level</summary>
    RiskLevel RiskLevel { get; }

    /// <summary>Requires elevated privileges</summary>
    bool RequiresAdmin { get; }

    /// <summary>Requires system restart to take effect</summary>
    bool RequiresRestart { get; }

    /// <summary>Whether a backup is created before applying</summary>
    bool RequiresBackup { get; }

    /// <summary>Whether this tweak can be safely undone</summary>
    bool CanUndo { get; }

    /// <summary>Is this tweak currently available on this Windows version / hardware</summary>
    bool IsAvailable { get; }

    /// <summary>Reason if not available</summary>
    string? UnavailableReason { get; }

    /// <summary>Expected positive effect description</summary>
    string ExpectedEffect { get; }

    /// <summary>Potential side effects / risks description</summary>
    string RiskDescription { get; }

    /// <summary>Get the current state of this setting on the system</summary>
    Task<TweakState> GetCurrentStateAsync(CancellationToken cancellationToken = default);

    /// <summary>Apply the tweak. Must create backup if RequiresBackup is true.</summary>
    Task<TweakResult> ApplyAsync(CancellationToken cancellationToken = default);

    /// <summary>Undo the tweak to previous state</summary>
    Task<TweakResult> UndoAsync(CancellationToken cancellationToken = default);

    /// <summary>Check if the tweak is currently applied (optimized state)</summary>
    Task<bool> IsAppliedAsync(CancellationToken cancellationToken = default);
}
