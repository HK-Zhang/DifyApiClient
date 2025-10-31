# GitHub Actions Setup for NuGet Publishing

This repository is configured with GitHub Actions to automatically build, test, and publish to NuGet.

## 🔄 Workflows

### 1. Build and Test (`build-test.yml`)

**Triggers:**
- Push to `master`, `main`, or `develop` branches
- Pull requests to these branches

**What it does:**
- ✅ Restores dependencies
- ✅ Builds the project in Release mode
- ✅ Runs all unit tests
- ✅ Generates test reports
- ✅ Creates a test package (not published)
- ✅ Uploads package as artifact

**Status:** Check the badge in README.md

### 2. Publish to NuGet (`publish-nuget.yml`)

**Triggers:**
- Push of version tags (e.g., `v1.0.0`, `v1.2.3`)

**What it does:**
- ✅ Runs all tests
- ✅ Extracts version from git tag
- ✅ Updates version in `.csproj`
- ✅ Creates NuGet package
- ✅ Publishes to NuGet.org
- ✅ Creates GitHub Release with package attachments
- ✅ Generates release notes

## 🔐 Required Secrets

You must add your NuGet API key as a GitHub secret:

### Step 1: Get NuGet API Key

1. Go to [nuget.org/account/apikeys](https://www.nuget.org/account/apikeys)
2. Click **"Create"**
3. Configure the key:
   - **Key Name:** `GitHub Actions - DifyApiClient`
   - **Expiration:** 365 days (or custom)
   - **Scopes:** Select "Push new packages and package versions"
   - **Glob Pattern:** `DifyApiClient*`
4. Click **"Create"**
5. **Copy the API key immediately** (you won't see it again!)

### Step 2: Add Secret to GitHub

1. Go to your repository on GitHub
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **"New repository secret"**
4. Add the secret:
   - **Name:** `NUGET_API_KEY`
   - **Value:** Paste your NuGet API key
5. Click **"Add secret"**

## 📋 Publishing Workflow

### Automated Publishing (Recommended)

1. **Update CHANGELOG.md** with your changes

2. **Commit all changes:**
   ```bash
   git add .
   git commit -m "Release v1.0.0"
   git push origin master
   ```

3. **Create and push a version tag:**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

4. **Watch the magic happen:**
   - GitHub Actions automatically triggers
   - Runs tests
   - Builds package
   - Publishes to NuGet
   - Creates GitHub Release

5. **Verify:**
   - Check **Actions** tab for workflow status
   - Visit https://www.nuget.org/packages/DifyApiClient/
   - Check **Releases** tab for GitHub release

### Manual Publishing (Alternative)

If you prefer manual control, use the PowerShell script:

```powershell
.\Publish-DifyApiClient.ps1 -ApiKey "your-key"
```

Then create the tag and release manually.

## 🏷️ Version Tags

### Creating Tags

**For initial release:**
```bash
git tag v1.0.0
git push origin v1.0.0
```

**For updates:**
```bash
# After updating code and CHANGELOG
git tag v1.0.1
git push origin v1.0.1
```

### Tag Format

- Use semantic versioning: `vMAJOR.MINOR.PATCH`
- Examples: `v1.0.0`, `v1.2.3`, `v2.0.0-beta`
- The `v` prefix is required

### Deleting Tags (if needed)

```bash
# Delete local tag
git tag -d v1.0.0

# Delete remote tag
git push origin :refs/tags/v1.0.0
```

## 📊 Monitoring Workflows

### View Workflow Runs

1. Go to **Actions** tab in your repository
2. Select the workflow (Build and Test / Publish to NuGet)
3. View run details, logs, and artifacts

### Workflow Status

Add badges to your README.md:

```markdown
[![Build and Test](https://github.com/HK-Zhang/DifyApiClient/actions/workflows/build-test.yml/badge.svg)](https://github.com/HK-Zhang/DifyApiClient/actions/workflows/build-test.yml)
[![Publish to NuGet](https://github.com/HK-Zhang/DifyApiClient/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/HK-Zhang/DifyApiClient/actions/workflows/publish-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/DifyApiClient.svg)](https://www.nuget.org/packages/DifyApiClient/)
```

## 🔧 Workflow Customization

### Change Target Branches

Edit `.github/workflows/build-test.yml`:

```yaml
on:
  push:
    branches: [ master, main, develop, feature/* ]
  pull_request:
    branches: [ master, main ]
```

### Add Pre-release Support

Create tags with pre-release identifiers:

```bash
git tag v1.0.0-beta
git push origin v1.0.0-beta
```

Edit `.github/workflows/publish-nuget.yml` to mark as prerelease:

```yaml
- name: Create GitHub Release
  uses: softprops/action-gh-release@v1
  with:
    prerelease: ${{ contains(github.ref, 'beta') || contains(github.ref, 'alpha') }}
```

### Add Code Coverage

Add to `.github/workflows/build-test.yml`:

```yaml
- name: Generate coverage report
  run: dotnet test -c Release --no-build --collect:"XPlat Code Coverage"

- name: Upload coverage to Codecov
  uses: codecov/codecov-action@v3
```

## 🚨 Troubleshooting

### Workflow Fails on Test

- Check the **Actions** tab for error details
- Fix the failing tests locally
- Commit and push again

### Publishing Fails

**Error: "Package already exists"**
- You cannot overwrite a published version
- Increment version and create new tag

**Error: "401 Unauthorized"**
- Check that `NUGET_API_KEY` secret is set correctly
- Verify API key has "Push" permissions
- Check if API key has expired

**Error: "Package validation failed"**
- Check package metadata in `.csproj`
- Ensure README.md exists
- Verify all required fields are present

### Tag Not Triggering Workflow

- Ensure tag format matches `v*.*.*`
- Check **.github/workflows/publish-nuget.yml** exists
- Verify you pushed the tag: `git push origin v1.0.0`

## 📝 Best Practices

### Before Creating a Release Tag

1. ✅ Update version in `CHANGELOG.md`
2. ✅ Run tests locally: `dotnet test`
3. ✅ Build package locally: `dotnet pack`
4. ✅ Test package locally in another project
5. ✅ Commit all changes
6. ✅ Create and push tag

### Version Management

- **Patch** (1.0.X): Bug fixes, no new features
- **Minor** (1.X.0): New features, backward compatible
- **Major** (X.0.0): Breaking changes

### Security

- 🔒 Never commit API keys to the repository
- 🔒 Use GitHub Secrets for sensitive data
- 🔒 Rotate API keys periodically
- 🔒 Use scoped API keys (limit to specific packages)

## 📚 Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [NuGet Publishing Guide](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package)
- [Semantic Versioning](https://semver.org/)
- [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github)

## 🎯 Quick Reference

```bash
# Complete release workflow
git add .
git commit -m "Release v1.0.0"
git push origin master
git tag v1.0.0
git push origin v1.0.0

# Then watch GitHub Actions do the rest!
```

---

**Your repository is now configured for automated NuGet publishing!** 🚀
