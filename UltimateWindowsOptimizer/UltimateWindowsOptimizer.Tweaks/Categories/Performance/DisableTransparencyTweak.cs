using Microsoft.Win32;
using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Tweaks.Base;

namespace UltimateWindowsOptimizer.Tweaks.Categories.Performance;

public sealed class DisableTransparencyTweak : RegistryTweakBase
{
    public DisableTransparencyTweak(IAppLogger logger, IBackupService? backup = null)
        : base(logger, backup) { }

    public override string Id => "visual.disable_transparency";
    public override string Name => "Disable Transparency Effects";
    public override TweakCategory Category => TweakCategory.Visual;
    public override string Description => "Turns off Acrylic/Mica transparency effects for lower GPU usage.";
    public override string DetailedExplanation =>
        "Transparency effects look nice but cost a little GPU fill-rate and can increase power consumption. " +
        "Disabling them is a classic performance tweak, especially useful on integrated graphics or laptops.";

    public override RiskLevel RiskLevel => RiskLevel.Safe;
    public override bool RequiresAdmin => false;
    public override bool RequiresRestart => false;
    public override string ExpectedEffect => "Slightly lower GPU usage and potentially better battery life.";
    public override string RiskDescription => "None. Purely visual.";

    protected override RegistryKey RootKey => Registry.CurrentUser;
    protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    protected override string ValueName => "EnableTransparency";
    protected override object OptimizedValue => 0;
    protected override object DefaultValue => 1;
}
