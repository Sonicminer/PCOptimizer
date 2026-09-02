using Microsoft.Win32;
using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Tweaks.Base;

namespace UltimateWindowsOptimizer.Tweaks.Categories.Gaming;

public sealed class EnableGameModeTweak : RegistryTweakBase
{
    public EnableGameModeTweak(IAppLogger logger, IBackupService? backup = null)
        : base(logger, backup) { }

    public override string Id => "gaming.enable_game_mode";
    public override string Name => "Enable Windows Game Mode";
    public override TweakCategory Category => TweakCategory.Gaming;
    public override string Description => "Enables the built-in Windows Game Mode for better resource prioritization while gaming.";
    public override string DetailedExplanation =>
        "Windows Game Mode prioritizes CPU/GPU resources for the foreground game and reduces background activity. " +
        "It is a first-party Microsoft feature and generally safe. Effectiveness varies by title.";

    public override RiskLevel RiskLevel => RiskLevel.Safe;
    public override bool RequiresAdmin => false;
    public override bool RequiresRestart => false;
    public override string ExpectedEffect => "Better frame-time consistency and slightly higher average FPS in some games.";
    public override string RiskDescription => "Very low. Official Windows feature.";

    protected override RegistryKey RootKey => Registry.CurrentUser;
    protected override string SubKeyPath => @"Software\Microsoft\GameBar";
    protected override string ValueName => "AutoGameModeEnabled";
    protected override object OptimizedValue => 1;
    protected override object DefaultValue => 1;
}
