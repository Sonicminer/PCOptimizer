using Microsoft.Win32;
using UltimateWindowsOptimizer.Core.Abstractions;
using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Core.Models;

namespace UltimateWindowsOptimizer.Tweaks.Base;

/// <summary>
/// Base class for registry-based tweaks. Makes creating new registry tweaks trivial.
/// Just specify the key path, value name, type and desired value.
/// </summary>
public abstract class RegistryTweakBase : TweakBase
{
    protected RegistryTweakBase(IAppLogger logger, IBackupService? backupService = null)
        : base(logger, backupService) { }

    /// <summary>Registry hive (e.g. Registry.CurrentUser or Registry.LocalMachine)</summary>
    protected abstract RegistryKey RootKey { get; }

    /// <summary>Full path under the root (e.g. @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced")</summary>
    protected abstract string SubKeyPath { get; }

    /// <summary>Value name</summary>
    protected abstract string ValueName { get; }

    /// <summary>Desired optimized value</summary>
    protected abstract object OptimizedValue { get; }

    /// <summary>Default / original value (for undo)</summary>
    protected abstract object DefaultValue { get; }

    /// <summary>Registry value kind</summary>
    protected virtual RegistryValueKind ValueKind => RegistryValueKind.DWord;

    public override bool RequiresAdmin => RootKey == Registry.LocalMachine || RootKey == Registry.ClassesRoot;

    public override async Task<TweakState> GetCurrentStateAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var key = RootKey.OpenSubKey(SubKeyPath, false);
                var current = key?.GetValue(ValueName);
                var currentStr = current?.ToString() ?? "(not set)";
                var optStr = OptimizedValue.ToString() ?? "";
                var defStr = DefaultValue.ToString() ?? "";

                return new TweakState
                {
                    CurrentValue = currentStr,
                    OptimizedValue = optStr,
                    DefaultValue = defStr,
                    IsOptimized = ValuesEqual(current, OptimizedValue),
                    IsDefault = ValuesEqual(current, DefaultValue)
                };
            }
            catch (Exception ex)
            {
                Logger.Warning("Failed to read registry for {0}: {1}", Id, ex.Message);
                return new TweakState { CurrentValue = "Error reading", IsOptimized = false };
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    public override async Task<bool> IsAppliedAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetCurrentStateAsync(cancellationToken).ConfigureAwait(false);
        return state.IsOptimized;
    }

    protected override async Task<TweakResult> ApplyInternalAsync(CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var key = RootKey.CreateSubKey(SubKeyPath, true)
                    ?? throw new InvalidOperationException($"Cannot open/create key: {SubKeyPath}");

                var previous = key.GetValue(ValueName)?.ToString();
                key.SetValue(ValueName, OptimizedValue, ValueKind);

                Logger.Trace("Registry", $"Set {RootKey.Name}\\{SubKeyPath}\\{ValueName} = {OptimizedValue}");

                return RequiresRestart
                    ? TweakResult.RequiresRestart($"Applied {Name}. Restart required.", previous, OptimizedValue.ToString())
                    : TweakResult.Ok($"Applied {Name}.", previous, OptimizedValue.ToString());
            }
            catch (UnauthorizedAccessException)
            {
                return TweakResult.Fail("Access denied. Please run as Administrator.");
            }
            catch (Exception ex)
            {
                return TweakResult.Fail(ex.Message, ex.ToString(), ex);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    protected override async Task<TweakResult> UndoInternalAsync(CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var key = RootKey.CreateSubKey(SubKeyPath, true)
                    ?? throw new InvalidOperationException($"Cannot open key: {SubKeyPath}");

                var previous = key.GetValue(ValueName)?.ToString();
                key.SetValue(ValueName, DefaultValue, ValueKind);

                Logger.Trace("Registry", $"Restored {RootKey.Name}\\{SubKeyPath}\\{ValueName} = {DefaultValue}");

                return TweakResult.Ok($"Undone {Name}.", previous, DefaultValue.ToString());
            }
            catch (Exception ex)
            {
                return TweakResult.Fail(ex.Message, ex.ToString(), ex);
            }
        }, cancellationToken).ConfigureAwait(false);
    }

    private static bool ValuesEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        return a.ToString() == b.ToString();
    }
}
