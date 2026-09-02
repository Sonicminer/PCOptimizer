using System.Windows;
using UltimateWindowsOptimizer.Core.Interfaces;
using UltimateWindowsOptimizer.Core.Services;
using UltimateWindowsOptimizer.Infrastructure.Backup;
using UltimateWindowsOptimizer.Infrastructure.Logging;
using UltimateWindowsOptimizer.Infrastructure.Services;
using UltimateWindowsOptimizer.Tweaks;
using UltimateWindowsOptimizer.Update.Services;

namespace UltimateWindowsOptimizer.App;

public partial class App : Application
{
    public static IAppLogger Logger { get; private set; } = null!;
    public static ITweakEngine TweakEngine { get; private set; } = null!;
    public static IBackupService BackupService { get; private set; } = null!;
    public static IChangeHistoryService HistoryService { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Ensure user data directories exist (first launch)
        UpdatePaths.EnsureDirectories();

        // Composition root – simple DI without external container for transparency
        Logger = new FileLogger();
        BackupService = new BackupService(Logger);
        HistoryService = new ChangeHistoryService(Logger);
        TweakEngine = new TweakEngine(Logger, HistoryService, BackupService);

        // Register all modular tweaks – this is the only place that grows when adding tweaks
        TweakRegistration.RegisterAll(TweakEngine, Logger, BackupService);

        Logger.Info("Ultimate Windows Optimizer started. {0} tweaks loaded.", TweakEngine.AllTweaks.Count);
    }
}
