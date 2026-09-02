# CI/CD

## Workflow

File: `.github/workflows/release.yml`

**Triggers**

- Push of tag `v*.*.*` (e.g. `v1.2.0`)
- Manual `workflow_dispatch` with version + channel

**Runner:** `windows-latest` (needed for WPF publish and Inno Setup)

**Steps**

1. Checkout
2. Setup .NET 8
3. Determine version from tag or input
4. Restore / Build / Test
5. Publish app + updater
6. Zip + SHA-256 + `latest.json`
7. Optional code signing (secrets)
8. Build installer with Inno Setup
9. Create GitHub Release + upload assets

## Secrets

| Name | Required | Purpose |
|------|----------|---------|
| `GITHUB_TOKEN` | Yes (automatic) | Create release |
| `SIGNING_CERT_BASE64` | No | Code signing |
| `SIGNING_CERT_PASSWORD` | No | Code signing |

Never store certificates or passwords in the repository.
