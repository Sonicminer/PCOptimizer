using Microsoft.Win32;
using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Tweaks.Base;

namespace UltimateWindowsOptimizer.Tweaks.Categories.Performance;

/// <summary>
/// Removes the artificial startup delay Windows adds for some apps.
/// </summary>
public sealed class DisableStartupDelayTweak : RegistryTweakBase
{
    public DisableStartupDelayTweak(IAppLogger logger, IBackupService? backup = null)
        : base(logger, backup) { }

    public override string Id => "perf.disable_startup_delay";
    public override string Name => "Disable Startup Delay";
    public override TweakCategory Category => TweakCategory.Performance;
    public override string Description => "Removes the artificial delay Windows applies before launching some startup apps.";
    public override string DetailedExplanation =>
        "Windows intentionally delays some startup applications to improve perceived boot performance. " +
        "On modern SSDs this delay is often unnecessary. Removing it can make desktop ready faster.";

    public override RiskLevel RiskLevel => RiskLevel.Recommended;
    public override bool RequiresAdmin => false;
    public override bool RequiresRestart => true;
    public override string ExpectedEffect => "Slightly faster desktop readiness after login.";
    public override string RiskDescription => "Very low. Only affects timing of startup apps.";

    protected override RegistryKey RootKey => Registry.CurrentUser;
    protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize";
    protected override string ValueName => "StartupDelayInMSec";
    protected override object OptimizedValue => 0;
    protected override object DefaultValue => 0; // value often does not exist by default
}
