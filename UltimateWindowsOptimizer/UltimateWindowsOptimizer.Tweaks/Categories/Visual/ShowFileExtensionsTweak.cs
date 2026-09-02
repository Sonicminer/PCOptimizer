using Microsoft.Win32;
using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Tweaks.Base;

namespace UltimateWindowsOptimizer.Tweaks.Categories.Visual;

public sealed class ShowFileExtensionsTweak : RegistryTweakBase
{
    public ShowFileExtensionsTweak(IAppLogger logger, IBackupService? backup = null)
        : base(logger, backup) { }

    public override string Id => "explorer.show_file_extensions";
    public override string Name => "Show File Extensions";
    public override TweakCategory Category => TweakCategory.Explorer;
    public override string Description => "Always show file extensions in Explorer (security + clarity).";
    public override string DetailedExplanation =>
        "By default Windows hides known file extensions. This can be abused by malware " +
        "(e.g. report.pdf.exe appears as report.pdf). Showing extensions is a recommended security practice.";

    public override RiskLevel RiskLevel => RiskLevel.Safe;
    public override bool RequiresAdmin => false;
    public override bool RequiresRestart => false;
    public override string ExpectedEffect => "Improved security awareness and easier file identification.";
    public override string RiskDescription => "None.";

    protected override RegistryKey RootKey => Registry.CurrentUser;
    protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    protected override string ValueName => "HideFileExt";
    protected override object OptimizedValue => 0;
    protected override object DefaultValue => 1;
}
