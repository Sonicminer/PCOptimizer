# Update System

## Architecture

```
PCOptimizer.exe          → checks for updates, shows UI
        ↓ (user confirms)
PCOptimizer.Updater.exe  → downloads, verifies, backups, installs, restarts
```

The main application **never** overwrites its own files while running.

## Manifest (`latest.json`)

Served from GitHub Releases (or your own HTTPS server):

```json
{
  "version": "1.3.0",
  "channel": "stable",
  "releaseDate": "2026-09-02T12:00:00Z",
  "downloadUrl": "https://github.com/.../PCOptimizer-1.3.0.zip",
  "sha256": "abc123...",
  "signature": null,
  "releaseNotes": "✓ New gaming tweaks\n✓ Bug fixes",
  "minimumWindowsVersion": "10.0.19041",
  "mandatory": false,
  "fileSizeBytes": 15728640
}
```

Channel-specific files: `latest.json`, `latest-beta.json`, `latest-nightly.json`.

## Security

1. **HTTPS only** – all downloads and manifest fetches
2. **SHA-256** – mandatory; update is aborted if mismatch
3. **Optional signature** – prepared for Authenticode / detached signatures
4. **Rollback** – previous version is backed up before install; restored on failure

## User settings

| Setting | Default |
|---------|---------|
| Automatically check for updates | On |
| Notify about updates | On |
| Automatically download | Off |
| Automatically install | Off |
| Channel | Stable |

## Offline behaviour

If there is no internet connection, the update check fails gracefully with a clear message. All core optimizer functions continue to work.

## Update history

Stored in `%LocalAppData%\PCOptimizer\Config\update_history.json`.
