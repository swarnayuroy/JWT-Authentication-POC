# CI/CD Quick Start Guide

## Prerequisites
- GitHub repository with admin access
- Projects pushed to GitHub
- (Optional) Azure subscription for deployment

## Step 1: Verify Workflow Files

All workflow files should be in `.github/workflows/`:
- ? `dotnet-ci-cd.yml` - Main CI/CD pipeline
- ? `pr-validation.yml` - PR validation
- ? `release.yml` - Release automation
- ? `dependency-check.yml` - Dependency monitoring

## Step 2: Enable GitHub Actions

1. Go to your repository on GitHub
2. Click **Settings** ? **Actions** ? **General**
3. Under "Actions permissions", select:
   - ? "Allow all actions and reusable workflows"
4. Click **Save**

## Step 3: Configure Environments (Optional but Recommended)

1. Go to **Settings** ? **Environments**
2. Create three environments:

### Development
- Click **New environment** ? Name: `Development`
- No protection rules needed
- Add environment variables if needed

### Staging
- Click **New environment** ? Name: `Staging`
- Optional: Add deployment branch rule (main only)
- Add environment variables

### Production
- Click **New environment** ? Name: `Production`
- ? Check "Required reviewers"
- Add yourself or team members as reviewers
- ? Add deployment branch rule: `main` only
- Add production environment variables

## Step 4: Test the Pipeline

### Option A: Push to trigger workflow
```bash
git add .
git commit -m "feat: Add CI/CD pipeline"
git push origin main
```

### Option B: Manual trigger
1. Go to **Actions** tab
2. Select ".NET CI/CD Pipeline"
3. Click **Run workflow**
4. Select branch and click **Run workflow**

## Step 5: Monitor the Build

1. Go to **Actions** tab
2. Click on the running workflow
3. Watch the build progress
4. Review logs if there are failures

## Step 6: Test Pull Request Validation

```bash
# Create a new branch
git checkout -b feature/test-ci

# Make a small change
echo "# Test" >> test.txt

# Commit and push
git add test.txt
git commit -m "test: CI/CD validation"
git push origin feature/test-ci

# Create PR on GitHub
# The PR validation workflow will run automatically
```

## Step 7: Create a Release

```bash
# Tag your commit
git tag -a v1.0.0 -m "Release version 1.0.0"

# Push the tag
git push origin v1.0.0

# The release workflow will:
# - Build all projects
# - Run tests
# - Create release artifacts
# - Create GitHub release
```

## Troubleshooting

### Build Fails on First Run?

**Problem:** MSBuild or NuGet restore issues

**Solution:**
1. Check that all `.csproj` files are committed
2. Verify `packages.config` exists for .NET Framework projects
3. Check workflow logs for specific errors

### Tests Fail?

**Problem:** Test project doesn't build

**Solution:**
1. Run tests locally first: `dotnet test TestProject/TestProject.csproj`
2. Fix any test failures
3. Commit and push again

### Deployment Issues?

**Problem:** Deployment steps are commented out

**Solution:**
1. Configure Azure credentials or deployment target
2. Uncomment deployment steps in workflow
3. Add necessary secrets

## Next Steps

### Add Status Badges to README
Copy badges from `.github/workflows/BADGES.md` to your main README.md

### Configure Secrets for Deployment

Go to **Settings** ? **Secrets and variables** ? **Actions** ? **New repository secret**

Add secrets as needed:
```
AZURE_CREDENTIALS
AZURE_WEBAPP_API_NAME
AZURE_WEBAPP_WEB_NAME
```

### Enable Branch Protection

Go to **Settings** ? **Branches** ? **Add branch protection rule**

For `main` branch:
- ? Require pull request reviews
- ? Require status checks: Select "Build and Test"
- ? Require branches to be up to date

### Set Up Notifications

Options:
1. Email notifications (automatic via GitHub)
2. Slack integration (add webhook to secrets)
3. Microsoft Teams webhook
4. Discord webhook

## Common Commands

```bash
# Check workflow status
gh run list --workflow=dotnet-ci-cd.yml

# View workflow logs
gh run view

# Trigger workflow manually
gh workflow run dotnet-ci-cd.yml

# Check for outdated packages locally
dotnet list package --outdated

# Check for vulnerabilities locally
dotnet list package --vulnerable --include-transitive

# Create and push release tag
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin v1.0.0
```

## Success Criteria

? Workflows appear in Actions tab  
? Build completes successfully  
? Tests pass  
? Artifacts are created  
? PR validation runs on pull requests  
? Status checks appear on PRs  

## Need Help?

1. Check workflow logs in Actions tab
2. Review `.github/workflows/README.md` for detailed documentation
3. Visit [GitHub Actions Documentation](https://docs.github.com/en/actions)

---

?? **Congratulations! Your CI/CD pipeline is now set up!**
