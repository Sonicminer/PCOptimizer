using Microsoft.Win32;
using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Tweaks.Base;

namespace UltimateWindowsOptimizer.Tweaks.Categories.Privacy;

public sealed class DisableAdvertisingIdTweak : RegistryTweakBase
{
    public DisableAdvertisingIdTweak(IAppLogger logger, IBackupService? backup = null)
        : base(logger, backup) { }

    public override string Id => "privacy.disable_advertising_id";
    public override string Name => "Disable Advertising ID";
    public override TweakCategory Category => TweakCategory.Privacy;
    public override string Description => "Disables the Windows Advertising ID used for personalized ads.";
    public override string DetailedExplanation =>
        "Windows generates a unique Advertising ID that apps can use to show personalized advertisements. " +
        "Disabling it improves privacy without affecting system functionality.";

    public override RiskLevel RiskLevel => RiskLevel.Safe;
    public override bool RequiresAdmin => false;
    public override bool RequiresRestart => false;
    public override string ExpectedEffect => "Improved privacy; apps can no longer use the advertising identifier.";
    public override string RiskDescription => "None. Some apps may show less personalized ads.";

    protected override RegistryKey RootKey => Registry.CurrentUser;
    protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo";
    protected override string ValueName => "Enabled";
    protected override object OptimizedValue => 0;
    protected override object DefaultValue => 1;
}
