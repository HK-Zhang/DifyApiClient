# Quick Start: GitHub Actions for NuGet

## One-Time Setup

### 1. Add NuGet API Key to GitHub

1. Get API key from https://www.nuget.org/account/apikeys
2. Go to your GitHub repo → **Settings** → **Secrets and variables** → **Actions**
3. Click **"New repository secret"**
4. Name: `NUGET_API_KEY`
5. Paste your key and save

## Publishing a New Version

### Simple 3-Step Process

```bash
# 1. Update CHANGELOG.md and commit changes
git add .
git commit -m "Release v1.0.0"
git push origin master

# 2. Create and push version tag
git tag v1.0.0
git push origin v1.0.0

# 3. Watch GitHub Actions publish automatically!
```

### What Happens Automatically

✅ Tests run  
✅ Package builds  
✅ Publishes to NuGet  
✅ Creates GitHub Release  

## Check Status

- **Actions Tab**: https://github.com/HK-Zhang/DifyApiClient/actions
- **NuGet**: https://www.nuget.org/packages/DifyApiClient/

## Version Numbering

- Bug fixes: `v1.0.1` (patch)
- New features: `v1.1.0` (minor)
- Breaking changes: `v2.0.0` (major)

---

**See `.github/GITHUB_ACTIONS_SETUP.md` for detailed documentation**
