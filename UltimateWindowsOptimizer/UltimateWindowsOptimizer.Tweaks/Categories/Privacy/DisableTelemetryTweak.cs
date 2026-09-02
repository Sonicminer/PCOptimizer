using Microsoft.Win32;
using UltimateWindowsOptimizer.Core.Enums;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Tweaks.Base;

namespace UltimateWindowsOptimizer.Tweaks.Categories.Privacy;

/// <summary>
/// Reduces Windows diagnostic data level (basic telemetry only).
/// </summary>
public sealed class DisableTelemetryTweak : RegistryTweakBase
{
    public DisableTelemetryTweak(IAppLogger logger, IBackupService? backup = null)
        : base(logger, backup) { }

    public override string Id => "privacy.reduce_telemetry";
    public override string Name => "Reduce Diagnostic Data";
    public override TweakCategory Category => TweakCategory.Privacy;
    public override string Description => "Sets Windows diagnostic data collection to the basic (required) level.";
    public override string DetailedExplanation =>
        "Windows can collect optional diagnostic data. Setting it to 'Basic' (or 'Required' on newer builds) " +
        "limits the amount of data sent to Microsoft while keeping the system fully functional and updatable.";

    public override RiskLevel RiskLevel => RiskLevel.Recommended;
    public override bool RequiresAdmin => true;
    public override bool RequiresRestart => false;
    public override bool RequiresBackup => true;
    public override string ExpectedEffect => "Improved privacy; less diagnostic data leaves the machine.";
    public override string RiskDescription => "Low. Some optional Microsoft services may receive less feedback data.";

    protected override RegistryKey RootKey => Registry.LocalMachine;
    protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\DataCollection";
    protected override string ValueName => "AllowTelemetry";
    protected override object OptimizedValue => 1; // 0=Security, 1=Basic, 2=Enhanced, 3=Full
    protected override object DefaultValue => 3;
}
