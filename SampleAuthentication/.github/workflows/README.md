# CI/CD Pipeline Documentation

## Overview

This repository uses GitHub Actions for continuous integration and deployment. The pipeline supports both .NET Framework 4.8/4.8.1 and .NET 8 projects.

## Workflows

### 1. **Main CI/CD Pipeline** (`.github/workflows/dotnet-ci-cd.yml`)

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches
- Manual trigger (workflow_dispatch)

**Jobs:**
- **Build**: Compiles all projects (DataContext, Web MVC, API Service)
- **Test**: Runs unit tests with code coverage
- **Code Quality**: Analyzes code quality (runs on PRs)
- **Security Scan**: Checks for vulnerable packages
- **Deploy (Dev)**: Deploys to development environment (develop branch)
- **Deploy (Staging)**: Deploys to staging environment (main branch)
- **Deploy (Production)**: Deploys to production (main branch, requires approval)

**Artifacts Generated:**
- `api-service`: Published API Service binaries
- `web-mvc`: Published Web MVC application
- `test-results`: Test execution results
- `code-coverage`: Code coverage reports

### 2. **Pull Request Validation** (`.github/workflows/pr-validation.yml`)

**Triggers:**
- Pull requests to `main` or `develop` branches

**Features:**
- Fast build validation
- Unit test execution
- Code coverage reporting with PR comments
- Vulnerability scanning

### 3. **Release Workflow** (`.github/workflows/release.yml`)

**Triggers:**
- Push of version tags (e.g., `v1.0.0`)

**Features:**
- Creates GitHub releases
- Generates release artifacts (ZIP files)
- Automatic release notes generation

**Usage:**
```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

### 4. **Dependency Updates Check** (`.github/workflows/dependency-check.yml`)

**Triggers:**
- Scheduled: Every Monday at 9 AM UTC
- Manual trigger

**Features:**
- Checks for outdated NuGet packages
- Scans for security vulnerabilities
- Creates GitHub issues with update reports

## Project Structure

```
SampleAuthentication/
??? .github/
?   ??? workflows/
?       ??? dotnet-ci-cd.yml        # Main CI/CD pipeline
?       ??? pr-validation.yml       # PR validation
?       ??? release.yml             # Release automation
?       ??? dependency-check.yml    # Dependency monitoring
??? API_Service/                    # .NET 8 Web API
??? web/                            # .NET Framework 4.8.1 MVC
??? DataContext/                    # .NET Framework 4.8 Library
??? TestProject/                    # .NET 8 Test Project
```

## Setup Instructions

### 1. GitHub Secrets Configuration

Add the following secrets to your GitHub repository (Settings ? Secrets and variables ? Actions):

#### For Azure Deployment:
```
AZURE_CREDENTIALS          # Azure Service Principal credentials
AZURE_WEBAPP_API_NAME      # Azure Web App name for API
AZURE_WEBAPP_WEB_NAME      # Azure Web App name for Web MVC
```

#### For Custom Deployment:
```
DEPLOY_SERVER              # Deployment server address
DEPLOY_USERNAME            # Deployment username
DEPLOY_PASSWORD            # Deployment password
```

### 2. Environment Configuration

Configure environments in GitHub (Settings ? Environments):

1. **Development**
   - Auto-deployment on `develop` branch
   - No approval required

2. **Staging**
   - Auto-deployment on `main` branch
   - Optional: Add protection rules

3. **Production**
   - Manual approval required
   - Add required reviewers
   - Protection rules enforced

### 3. Branch Protection Rules

Recommended settings for `main` branch:
- ? Require pull request reviews (1+ approvers)
- ? Require status checks to pass
  - `Build and Test`
  - `Validate PR`
- ? Require branches to be up to date
- ? Do not allow bypassing the above settings

## CI/CD Pipeline Flow

```
???????????????
?   Commit    ?
???????????????
       ?
       ?
???????????????????????
?  PR Validation      ?  (Fast feedback)
?  - Build            ?
?  - Test             ?
?  - Coverage         ?
???????????????????????
       ?
       ?
???????????????????????
?  Main Build         ?  (On merge)
?  - Build all        ?
?  - Run tests        ?
?  - Security scan    ?
?  - Create artifacts ?
???????????????????????
       ?
       ???????????????????
       ?                 ?
       ?                 ?
????????????????  ????????????????
? Development  ?  ?   Staging    ?
? (develop)    ?  ?   (main)     ?
????????????????  ????????????????
                         ?
                         ?
                  ????????????????
                  ? Production   ?
                  ? (approval)   ?
                  ????????????????
```

## Local Testing

Test the build locally before pushing:

### .NET Framework projects:
```powershell
# Restore packages
nuget restore web/web.csproj -PackagesDirectory packages
nuget restore DataContext/DataContext.csproj -PackagesDirectory packages

# Build
msbuild DataContext/DataContext.csproj /p:Configuration=Release
msbuild web/web.csproj /p:Configuration=Release
```

### .NET 8 projects:
```bash
# Restore and build
dotnet restore API_Service/API_Service.csproj
dotnet build API_Service/API_Service.csproj --configuration Release

# Run tests
dotnet test TestProject/TestProject.csproj --configuration Release
```

## Deployment

### Azure Web Apps (Recommended)

Uncomment and configure the Azure deployment steps in `dotnet-ci-cd.yml`:

```yaml
- name: Azure Login
  uses: azure/login@v2
  with:
    creds: ${{ secrets.AZURE_CREDENTIALS }}

- name: Deploy API to Azure Web App
  uses: azure/webapps-deploy@v3
  with:
    app-name: ${{ secrets.AZURE_WEBAPP_API_NAME }}
    package: ./api

- name: Deploy Web to Azure Web App
  uses: azure/webapps-deploy@v3
  with:
    app-name: ${{ secrets.AZURE_WEBAPP_WEB_NAME }}
    package: ./web
```

### IIS Deployment

Add IIS deployment steps:

```yaml
- name: Deploy to IIS
  uses: appleboy/scp-action@master
  with:
    host: ${{ secrets.DEPLOY_SERVER }}
    username: ${{ secrets.DEPLOY_USERNAME }}
    password: ${{ secrets.DEPLOY_PASSWORD }}
    source: "./publish/*"
    target: "C:/inetpub/wwwroot/"
```

## Monitoring and Notifications

### Add Slack Notifications

Add to workflow files:

```yaml
- name: Slack Notification
  uses: slackapi/slack-github-action@v1.24.0
  if: always()
  with:
    payload: |
      {
        "text": "Build ${{ job.status }}: ${{ github.repository }}",
        "status": "${{ job.status }}"
      }
  env:
    SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
```

### Email Notifications

Configure in repository settings or add email action.

## Troubleshooting

### Common Issues

1. **MSBuild not found**
   - Solution: Workflow uses `windows-latest` which includes MSBuild

2. **NuGet restore fails**
   - Solution: Check `packages.config` and network access

3. **Test failures**
   - Check test logs in Actions ? Test Results artifact

4. **Deployment fails**
   - Verify secrets are correctly configured
   - Check deployment target accessibility

## Best Practices

1. ? Always create feature branches from `develop`
2. ? Write tests for new features
3. ? Keep dependencies up to date
4. ? Review security scan results
5. ? Use semantic versioning for releases
6. ? Add meaningful commit messages
7. ? Keep secrets secure (never commit them)

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET CLI Reference](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- [MSBuild Reference](https://docs.microsoft.com/en-us/visualstudio/msbuild/)
- [Azure DevOps Integration](https://azure.microsoft.com/en-us/products/devops/)

## Support

For issues or questions:
1. Check the workflow run logs
2. Review this documentation
3. Create an issue in the repository
4. Contact the development team

---

**Last Updated:** January 2025  
**Maintained By:** DevOps Team
