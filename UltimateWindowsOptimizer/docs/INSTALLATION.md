# Installation

## End-user

1. Download `PCOptimizerSetup.exe` from the [Releases](https://github.com/YourOrg/PCOptimizer/releases) page.
2. Run the installer.
3. Choose install folder (default: `C:\Program Files\PCOptimizer`).
4. Optionally create a desktop shortcut and/or enable autostart.
5. Finish – PCOptimizer starts automatically.

### Upgrade

Running a newer `PCOptimizerSetup.exe` over an existing installation upgrades in place (same AppId).

### Uninstall

Windows Settings → Apps → PCOptimizer → Uninstall  
or Start Menu → PCOptimizer → Uninstall.

You will be asked whether to keep user data (logs, backups, settings).

## Portable (optional)

Unzip `PCOptimizer-x.y.z.zip` to any folder and run `PCOptimizer.exe`.  
User data still goes to `%LocalAppData%\PCOptimizer`.

## Developer – local run

```bash
dotnet restore
dotnet build -c Release
dotnet run --project UltimateWindowsOptimizer.App
```

## First launch

On first start the application:

1. Detects install location
2. Creates `%LocalAppData%\PCOptimizer\{Logs,Backups,Config,Cache,UpdateCache}`
3. Initializes default update settings
4. Optionally checks for updates (if enabled)
