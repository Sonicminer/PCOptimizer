using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.Core.Abstractions;

/// <summary>
/// Abstract base class that implements common ITweak boilerplate.
/// Derive from this to create new tweaks quickly.
/// 
/// Example:
/// public class DisableVisualEffectsTweak : TweakBase
/// {
///     public override string Id => "visual.disable_effects";
///     ...
/// }
/// </summary>
public abstract class TweakBase : ITweak
{
    protected readonly IAppLogger Logger;
    protected readonly IBackupService? BackupService;

    protected TweakBase(IAppLogger logger, IBackupService? backupService = null)
    {
        Logger = logger;
        BackupService = backupService;
    }

    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract TweakCategory Category { get; }
    public abstract string Description { get; }
    public virtual string DetailedExplanation => Description;
    public virtual RiskLevel RiskLevel => RiskLevel.Recommended;
    public virtual bool RequiresAdmin => true;
    public virtual bool RequiresRestart => false;
    public virtual bool RequiresBackup => RiskLevel >= RiskLevel.Advanced;
    public virtual bool CanUndo => true;
    public virtual bool IsAvailable => true;
    public virtual string? UnavailableReason => null;
    public virtual string ExpectedEffect => "Improved system performance or user experience.";
    public virtual string RiskDescription => "Low risk when used as intended.";

    public abstract Task<TweakState> GetCurrentStateAsync(CancellationToken cancellationToken = default);
    public abstract Task<bool> IsAppliedAsync(CancellationToken cancellationToken = default);

    public virtual async Task<TweakResult> ApplyAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return TweakResult.Fail($"Tweak not available: {UnavailableReason}");

        if (await IsAppliedAsync(cancellationToken).ConfigureAwait(false))
            return new TweakResult { Success = true, Result = ChangeResult.AlreadyApplied, Message = "Already applied." };

        string? backupId = null;
        try
        {
            if (RequiresBackup && BackupService != null)
            {
                var snapshot = await BackupService.CreateSnapshotAsync($"Before applying {Name}", cancellationToken)
                    .ConfigureAwait(false);
                backupId = snapshot.Id;
            }

            Logger.Info("Applying tweak: {0} ({1})", Name, Id);
            var result = await ApplyInternalAsync(cancellationToken).ConfigureAwait(false);
            result.BackupId = backupId;

            if (result.Success)
                Logger.Info("Successfully applied: {0}", Name);
            else
                Logger.Warning("Failed to apply {0}: {1}", Name, result.Message);

            return result;
        }
        catch (Exception ex)
        {
            Logger.Error("Exception while applying {0}", ex, Name);
            return TweakResult.Fail($"Exception: {ex.Message}", ex.ToString(), ex);
        }
    }

    public virtual async Task<TweakResult> UndoAsync(CancellationToken cancellationToken = default)
    {
        if (!CanUndo)
            return TweakResult.Fail("This tweak cannot be undone.");

        try
        {
            Logger.Info("Undoing tweak: {0} ({1})", Name, Id);
            var result = await UndoInternalAsync(cancellationToken).ConfigureAwait(false);
            if (result.Success)
                Logger.Info("Successfully undone: {0}", Name);
            return result;
        }
        catch (Exception ex)
        {
            Logger.Error("Exception while undoing {0}", ex, Name);
            return TweakResult.Fail($"Exception: {ex.Message}", ex.ToString(), ex);
        }
    }

    /// <summary>Implement the actual apply logic here</summary>
    protected abstract Task<TweakResult> ApplyInternalAsync(CancellationToken cancellationToken);

    /// <summary>Implement the actual undo logic here</summary>
    protected abstract Task<TweakResult> UndoInternalAsync(CancellationToken cancellationToken);
}
