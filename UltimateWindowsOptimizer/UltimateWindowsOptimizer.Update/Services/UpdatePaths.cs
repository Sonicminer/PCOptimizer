namespace UltimateWindowsOptimizer.Update.Services;

/// <summary>
/// Central definition of all data directories.
/// Application files stay in install dir; user data goes to LocalAppData.
/// </summary>
public static class UpdatePaths
{
    public const string AppName = "PCOptimizer";
    public const string Publisher = "PCOptimizer";

    /// <summary>LocalAppData\PCOptimizer</summary>
    public static string UserDataRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);

    public static string LogsDir => Path.Combine(UserDataRoot, "Logs");
    public static string BackupsDir => Path.Combine(UserDataRoot, "Backups");
    public static string ConfigDir => Path.Combine(UserDataRoot, "Config");
    public static string CacheDir => Path.Combine(UserDataRoot, "Cache");
    public static string UpdateCacheDir => Path.Combine(UserDataRoot, "UpdateCache");
    public static string UpdateHistoryFile => Path.Combine(ConfigDir, "update_history.json");
    public static string UpdateSettingsFile => Path.Combine(ConfigDir, "update_settings.json");

    /// <summary>Directory of the running executable (install location).</summary>
    public static string InstallDir
    {
        get
        {
            var exe = Environment.ProcessPath
                      ?? AppContext.BaseDirectory;
            return Path.GetDirectoryName(exe) ?? AppContext.BaseDirectory;
        }
    }

    public static string MainExeName => "PCOptimizer.exe";
    public static string UpdaterExeName => "PCOptimizer.Updater.exe";

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(UserDataRoot);
        Directory.CreateDirectory(LogsDir);
        Directory.CreateDirectory(BackupsDir);
        Directory.CreateDirectory(ConfigDir);
        Directory.CreateDirectory(CacheDir);
        Directory.CreateDirectory(UpdateCacheDir);
    }
}
