using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Tweaks.Categories.Gaming;
using UltimateWindowsOptimizer.Tweaks.Categories.Performance;
using UltimateWindowsOptimizer.Tweaks.Categories.Privacy;
using UltimateWindowsOptimizer.Tweaks.Categories.Visual;

namespace UltimateWindowsOptimizer.Tweaks;

/// <summary>
/// Central registration point for all built-in tweaks.
/// To add a new tweak:
///   1. Create a class implementing ITweak (or derive from TweakBase / RegistryTweakBase)
///   2. Add one line here: engine.Register(new YourTweak(logger, backup));
/// That's it. No other changes required. Scales easily to 100-200 tweaks.
/// </summary>
public static class TweakRegistration
{
    public static void RegisterAll(ITweakEngine engine, IAppLogger logger, IBackupService backup)
    {
        // ===== Visual / Explorer =====
        engine.Register(new DisableAnimationsTweak(logger, backup));
        engine.Register(new ShowFileExtensionsTweak(logger, backup));
        engine.Register(new DisableTransparencyTweak(logger, backup));

        // ===== Performance =====
        engine.Register(new DisableStartupDelayTweak(logger, backup));

        // ===== Privacy =====
        engine.Register(new DisableAdvertisingIdTweak(logger, backup));
        engine.Register(new DisableTelemetryTweak(logger, backup));

        // ===== Gaming =====
        engine.Register(new EnableGameModeTweak(logger, backup));

        // ---------------------------------------------------------------
        // ADD NEW TWEAKS BELOW – one line each. Nothing else needs changing.
        // engine.Register(new YourAwesomeTweak(logger, backup));
        // ---------------------------------------------------------------
    }
}
