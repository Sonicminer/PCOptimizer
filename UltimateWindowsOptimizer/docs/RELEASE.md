# Release Process

## Semantic Versioning

`MAJOR.MINOR.PATCH` – e.g. `1.2.0`

- **PATCH** – bug fixes
- **MINOR** – new features, backward compatible
- **MAJOR** – breaking changes

## Steps to release

1. Update version in relevant `.csproj` files / build script.
2. Commit and push.
3. Create an annotated tag:
   ```bash
   git tag -a v1.2.0 -m "Release 1.2.0"
   git push origin v1.2.0
   ```
4. GitHub Actions workflow `release.yml` runs automatically:
   - Restore, build, test
   - Publish app + updater
   - Create zip + SHA-256
   - Generate `latest.json`
   - Build installer (Inno Setup)
   - Create GitHub Release and upload assets

## Manual build

```powershell
.\build\build-release.ps1 -Version 1.2.0 -Channel stable
```

Outputs under `artifacts/`:

- `publish/app/` – application files
- `publish/updater/` – updater
- `package/PCOptimizer-1.2.0.zip`
- `package/SHA256SUMS.txt`
- `package/latest.json`
- `installer/PCOptimizerSetup.exe` (if Inno Setup installed)

## Channels

| Tag / input | Manifest |
|-------------|----------|
| `v1.2.0` or channel=stable | `latest.json` |
| channel=beta | `latest-beta.json` |
| channel=nightly | `latest-nightly.json` |
