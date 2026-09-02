# PCOptimizer – Ultimate Windows Optimizer & Tweaking Suite

Professionelle, modulare Windows 10/11 Optimization Suite mit Installer, Auto-Updater und CI/CD-Release-Pipeline.

## Komponenten

| Datei | Beschreibung |
|-------|--------------|
| **PCOptimizer.exe** | Hauptanwendung |
| **PCOptimizer.Updater.exe** | Separater sicherer Updater |
| **PCOptimizerSetup.exe** | Windows-Installer (Inno Setup) |

## Projektstruktur

```
UltimateWindowsOptimizer/
├── UltimateWindowsOptimizer.Core/       # Domain, ITweak, TweakEngine
├── UltimateWindowsOptimizer.Infrastructure/
├── UltimateWindowsOptimizer.Tweaks/     # Alle Tweaks (modular erweiterbar)
├── UltimateWindowsOptimizer.Update/     # Manifest, Version, Hash, UpdateService
├── UltimateWindowsOptimizer.App/        # WPF UI (Dark Theme)
├── PCOptimizer.Updater/                 # Standalone Updater-Prozess
├── installer/PCOptimizer.iss            # Inno Setup Script
├── build/build-release.ps1              # Reproduzierbarer Release-Build
├── .github/workflows/release.yml        # CI/CD
├── docs/                                # INSTALLATION, UPDATES, RELEASE, SIGNING, CI-CD
└── tests/                               # Unit Tests (VersionComparer, …)
```

## Neue Tweaks hinzufügen

1. Klasse von `RegistryTweakBase` / `TweakBase` ableiten
2. Eine Zeile in `TweakRegistration.RegisterAll(...)`:

```csharp
engine.Register(new MyNewTweak(logger, backup));
```

## Build (Entwicklung)

```bash
dotnet restore
dotnet build -c Release
dotnet test
dotnet run --project UltimateWindowsOptimizer.App   # nur auf Windows
```

## Release

```powershell
# Lokal
.\build\build-release.ps1 -Version 1.2.0 -Channel stable

# Oder Git-Tag → GitHub Actions
git tag -a v1.2.0 -m "Release 1.2.0"
git push origin v1.2.0
```

## Update-Flow

```
PCOptimizer.exe → Check → User bestätigt → App schließt
       → PCOptimizer.Updater.exe
       → Download → SHA-256 → Backup → Install → Restart
```

Bei Fehler: automatischer Rollback auf die vorherige Version.

## Dokumentation

- [INSTALLATION.md](docs/INSTALLATION.md)
- [UPDATES.md](docs/UPDATES.md)
- [RELEASE.md](docs/RELEASE.md)
- [SIGNING.md](docs/SIGNING.md)
- [CI-CD.md](docs/CI-CD.md)

## Sicherheit

- Keine versteckten Änderungen
- Backup vor kritischen Tweaks
- Updates nur über HTTPS + SHA-256
- Code-Signing vorbereitet (Secrets, keine Keys im Repo)
- Offline-fähig: Kernfunktionen brauchen kein Internet
