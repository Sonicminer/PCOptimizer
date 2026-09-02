# Code Signing

## Goal

Sign:

- `PCOptimizer.exe`
- `PCOptimizer.Updater.exe`
- `PCOptimizerSetup.exe`

so Windows SmartScreen and enterprise policies trust the binaries.

## Setup (never commit private keys)

1. Obtain an Authenticode code-signing certificate (e.g. DigiCert, Sectigo).
2. Export as `.pfx`.
3. Store in CI secrets (GitHub Actions):

| Secret | Content |
|--------|---------|
| `SIGNING_CERT_BASE64` | Base64-encoded `.pfx` |
| `SIGNING_CERT_PASSWORD` | PFX password |

4. In the release workflow, decode and call `signtool`:

```powershell
$bytes = [Convert]::FromBase64String($env:SIGNING_CERT_BASE64)
[IO.File]::WriteAllBytes("cert.pfx", $bytes)
signtool sign /f cert.pfx /p $env:SIGNING_CERT_PASSWORD `
  /tr http://timestamp.digicert.com /td sha256 /fd sha256 `
  path\to\PCOptimizer.exe
```

## Local signing

```powershell
.\build\build-release.ps1 -Version 1.2.0 `
  -SigningCertPath C:\certs\codesign.pfx `
  -SigningCertPassword "..."
```

## Without a certificate

The build **still succeeds**. Binaries are simply unsigned. Document this clearly in the release notes until signing is configured.
