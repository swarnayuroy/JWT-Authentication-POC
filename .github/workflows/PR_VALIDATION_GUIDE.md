# PR Validation Workflow Guide

## Overview

The `pr-validation.yml` GitHub Actions workflow is an **automated quality gate** that runs whenever a pull request is created or updated. It ensures code quality, functionality, and security before merging changes to `main` or `develop` branches.

---

## Purpose & Benefits

### 🎯 What It Does

| Component | Purpose |
|-----------|---------|
| **.NET Validation** | Builds and validates DataContext, Web, and API_Service projects |
| **.NET Testing** | Runs NUnit tests with code coverage collection |
| **Angular Validation** | Builds the Angular 19.x frontend application |
| **Angular Testing** | Runs Jasmine tests with Chrome Headless |
| **Security Scanning** | Detects vulnerable NuGet packages |
| **Artifact Collection** | Preserves test results and build outputs for review |

### ✨ Benefits

- **Prevents Broken Code** - Ensures all code compiles before merge
- **Catches Regressions** - Unit tests catch breaking changes early
- **Security First** - Identifies vulnerable dependencies
- **Visibility** - Full build logs and test results available as artifacts
- **Isolation** - Failed job doesn't prevent other jobs from running

---

## Workflow Architecture

### Job Dependency Graph

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│  validate-dotnet          validate-angular      security-check  │
│      (build)                  (build)                (scan)      │
│        │                         │                    │          │
│        ▼                         ▼                    │          │
│  test-dotnet           test-angular                  │          │
│   (unit tests)         (karma tests)                 │          │
│        │                         │                   │          │
│        └─────────────────┬───────┘                   │          │
│                          ▼                            │          │
│                  validation-summary ◄────────────────┘          │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Job Details

#### 1. **validate-dotnet** (parallel, no dependencies)
- Restores NuGet packages
- Builds DataContext (MSBuild, .NET Framework)
- Builds Web (MSBuild, .NET Framework)
- Builds API_Service (.NET 8)
- **Status**: ⚠️ Continues on error (legacy projects may have issues)

#### 2. **test-dotnet** (depends on validate-dotnet)
- Runs NUnit tests from TestProject
- Collects XPlat Code Coverage
- Uploads test results as artifacts
- **Status**: ⚠️ Continues on error (warnings won't block merge)

#### 3. **validate-angular** (parallel, no dependencies)
- Installs npm dependencies
- Builds production Angular bundle
- Uploads build output as artifact
- **Status**: ✅ Strict (build failure blocks)

#### 4. **test-angular** (depends on validate-angular)
- Runs Karma tests with ChromeHeadless
- Collects coverage reports
- Uploads test results as artifact
- **Status**: ⚠️ Continues on error

#### 5. **security-check** (parallel, no dependencies)
- Scans API_Service for vulnerable packages
- Generates security report
- Warns instead of blocks
- **Status**: ⚠️ Informational only

#### 6. **validation-summary** (depends on all)
- Final status report
- Summarizes all checks
- **Status**: Always runs (even if others fail)

---

## Key Configuration Details

### .NET Projects

```yaml
# DataContext & Web (Framework 4.8/4.8.1)
- Uses MSBuild for compilation
- Restores NuGet packages to local 'packages' directory
- Continues on error (legacy support)

# API_Service (.NET 8)
- Uses dotnet CLI
- Modern SDK-style project
- Strict error handling
```

### Angular Project

```yaml
# Location: SampleAuthentication/angular-web
- Node 18.x runtime
- npm package manager
- Karma test runner
- Chrome/Chromium headless browser
- Production build optimization
```

### Error Handling Strategy

```yaml
# Strict Mode (blocks PR if failed)
- Angular build validation

# Lenient Mode (warnings only)
- .NET legacy project builds
- .NET unit tests
- Angular unit tests
- Security scanning
```

---

## Artifacts Generated

After each PR validation run, the following artifacts are available:

| Artifact | Location | Retention |
|----------|----------|-----------|
| .NET Test Results | `dotnet-test-results/` | 5 days |
| Angular Build | `angular-build/` | 5 days |
| Angular Test Results | `angular-test-results/` | 5 days |
| Security Report | `security-report.txt` | 5 days |

**Access artifacts:**
1. Go to PR checks
2. Click "Summary" or specific job
3. Download artifacts section

---

## Common Failure Scenarios & Solutions

### ❌ .NET Build Fails

**Cause**: Missing NuGet packages or Framework 4.8 dependencies

**Solution**:
```powershell
# Clean and rebuild locally
dotnet clean
dotnet build
```

### ❌ Angular Build Fails

**Cause**: TypeScript compilation errors or missing dependencies

**Solution**:
```bash
cd SampleAuthentication/angular-web
npm ci  # Clean install
npm run build
```

### ❌ Tests Fail

**Cause**: Code regression or test environment issues

**Solution**:
```bash
# .NET tests
dotnet test SampleAuthentication/TestProject/TestProject.csproj

# Angular tests
cd SampleAuthentication/angular-web
npm test -- --watch=false --browsers=ChromeHeadlessNoSandbox
```

### ❌ Security Warning

**Cause**: Known vulnerable package version

**Solution**:
- Review `security-report.txt` artifact
- Update package version in .csproj
- Re-run workflow

---

## Workflow YAML Structure

```yaml
name: Pull Request Validation          # Workflow name

on:
  pull_request:                        # Trigger on PR events
    branches: [ main, develop ]        # Only for these branches

env:                                   # Shared environment variables
  DOTNET_VERSION: '8.0.x'
  NODE_VERSION: '18.x'
  ANGULAR_WEB_PATH: 'SampleAuthentication/angular-web'

jobs:                                  # 6 parallel/dependent jobs
  validate-dotnet:
    runs-on: windows-latest            # Windows runner required
    steps:                             # Sequential steps
    ...
```

---

## Performance Metrics

| Component | Time | Notes |
|-----------|------|-------|
| Checkout + Setup | ~2 min | Parallel for all |
| .NET Build | ~3-4 min | Includes legacy projects |
| .NET Tests | ~2-3 min | With coverage |
| Angular Build | ~4-5 min | Prod optimization takes time |
| Angular Tests | ~3-4 min | Chrome startup adds overhead |
| Security Scan | ~1 min | Quick dependency check |
| **Total** | **~10-12 min** | Parallel jobs reduce time |

---

## Customization Guide

### To Add More .NET Tests

Edit `.github/workflows/pr-validation.yml`:

```yaml
- name: Run Additional Tests
  run: dotnet test YourTestProject.csproj --configuration Release
```

### To Add Code Quality Analysis

```yaml
- name: Analyze Code Quality
  run: dotnet add package StyleCopAnalyzers
```

### To Require Specific Coverage %

```yaml
- name: Check Code Coverage
  run: |
    # Add threshold validation
    reportgenerator -reports:**/*.xml -targetdir:coverage -reporttypes:Cobertura
```

---

## Troubleshooting

### View Detailed Logs

1. Open failed PR check
2. Click "View all checks"
3. Click job name
4. Expand step logs

### Run Workflow Manually

```bash
# Not yet available via CLI, but can re-open PR to trigger
git push -f origin feature/my-branch  # Force push to retrigger
```

### Disable Specific Check

Edit workflow and set `continue-on-error: true` on step:

```yaml
- name: Potentially Failing Step
  run: your-command
  continue-on-error: true
```

---

## Best Practices

### ✅ DO

- Keep test projects updated with code changes
- Run workflow locally before pushing: `dotnet test`
- Review artifacts after PR validation
- Add meaningful commit messages
- Update dependencies regularly

### ❌ DON'T

- Force push to bypass validation
- Ignore security warnings
- Commit code with failing tests
- Skip PR validation for "small changes"
- Leave build artifacts in commits

---

## Support & Questions

For workflow issues:
1. Check artifact logs first
2. Review this guide
3. Check GitHub Actions documentation
4. Contact DevOps team

---

**Last Updated**: March 19, 2026
**Workflow Version**: 2.0
**Status**: Active ✅
