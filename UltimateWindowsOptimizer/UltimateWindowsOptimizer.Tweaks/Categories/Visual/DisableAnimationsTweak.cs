using Microsoft.Win32;
using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Tweaks.Base;

namespace UltimateWindowsOptimizer.Tweaks.Categories.Visual;

/// <summary>
/// Disables Windows UI animations for snappier feel (especially useful for gaming / low-end systems).
/// </summary>
public sealed class DisableAnimationsTweak : RegistryTweakBase
{
    public DisableAnimationsTweak(IAppLogger logger, IBackupService? backup = null)
        : base(logger, backup) { }

    public override string Id => "visual.disable_animations";
    public override string Name => "Disable UI Animations";
    public override TweakCategory Category => TweakCategory.Visual;
    public override string Description => "Disables window animations, menu fading and taskbar animations for faster UI response.";
    public override string DetailedExplanation =>
        "Windows plays many subtle animations when opening/closing windows, menus and the taskbar. " +
        "Disabling them reduces GPU/CPU load slightly and makes the interface feel more responsive. " +
        "This is a well-known and safe optimization used by many performance tools.";

    public override RiskLevel RiskLevel => RiskLevel.Safe;
    public override bool RequiresAdmin => false;
    public override bool RequiresRestart => false;
    public override bool RequiresBackup => false;
    public override string ExpectedEffect => "Snappier UI, slightly lower resource usage when opening windows/menus.";
    public override string RiskDescription => "Very low. You can re-enable animations at any time.";

    protected override RegistryKey RootKey => Registry.CurrentUser;
    protected override string SubKeyPath => @"Control Panel\Desktop\WindowMetrics";
    protected override string ValueName => "MinAnimate";
    protected override object OptimizedValue => "0";
    protected override object DefaultValue => "1";
    protected override RegistryValueKind ValueKind => RegistryValueKind.String;
}
