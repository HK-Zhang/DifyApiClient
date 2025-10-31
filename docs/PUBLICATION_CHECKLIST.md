# Pre-Publication Checklist

Use this checklist before publishing to NuGet:

## Code Quality
- [ ] All unit tests pass (`dotnet test`)
- [ ] Code builds without warnings in Release mode
- [ ] XML documentation comments are complete
- [ ] No hardcoded sensitive information (API keys, URLs, etc.)
- [ ] Code follows .NET naming conventions and best practices

## Package Metadata
- [ ] Version number updated in `.csproj`
- [ ] Version follows semantic versioning (major.minor.patch)
- [ ] `PackageReleaseNotes` updated with changes
- [ ] `Description` is clear and comprehensive
- [ ] `PackageTags` are relevant
- [ ] `Authors` and `Copyright` are correct
- [ ] License is specified (`MIT`)

## Documentation
- [ ] README.md is up to date
- [ ] README.md includes:
  - [ ] Clear description
  - [ ] Installation instructions
  - [ ] Quick start example
  - [ ] API reference
  - [ ] Usage examples
  - [ ] License information
- [ ] CHANGELOG.md updated (if you have one)
- [ ] All sample code in README works

## Repository
- [ ] All changes committed to git
- [ ] Repository is pushed to GitHub
- [ ] Repository URL in `.csproj` is correct
- [ ] Create a git tag for the version (e.g., `v1.0.0`)

## Package Contents
- [ ] Package built successfully (`dotnet pack`)
- [ ] Inspected package contents (use NuGetPackageExplorer or nuget.info)
- [ ] README.md is included in package
- [ ] All necessary assemblies are included
- [ ] No unnecessary files are included
- [ ] Symbol package (.snupkg) is created

## Testing
- [ ] Tested package locally in a separate project
- [ ] Verified package installs correctly
- [ ] Verified basic functionality works
- [ ] Checked that dependencies are resolved correctly

## NuGet Account
- [ ] NuGet.org account created and verified
- [ ] API key generated with appropriate permissions
- [ ] API key stored securely (not in source control)

## Publication
- [ ] Package ID is available (not taken by another package)
- [ ] Ready to publish

## Post-Publication
- [ ] Package appears on NuGet.org (may take 5-15 minutes)
- [ ] Package page displays correctly
- [ ] README renders properly
- [ ] Can install package from NuGet.org
- [ ] Create GitHub release matching the version tag
- [ ] Announce the release (if appropriate)

---

**Note:** Once published, a package version cannot be deleted, only unlisted. Make sure everything is correct before publishing!
