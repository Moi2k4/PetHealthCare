# PowerShell script to push to GitHub
# Usage: .\push-to-github.ps1 YOUR_GITHUB_USERNAME

param(
    [Parameter(Mandatory=$true)]
    [string]$GitHubUsername
)

Write-Host "🚀 Pushing PetHealthCare to GitHub..." -ForegroundColor Cyan
Write-Host ""

# Check if git is installed
try {
    git --version | Out-Null
} catch {
    Write-Host "❌ Git is not installed. Please install Git first." -ForegroundColor Red
    exit 1
}

# Navigate to project directory
$projectPath = "F:\PetCare"
Set-Location $projectPath

Write-Host "📁 Current directory: $projectPath" -ForegroundColor Green
Write-Host ""

# Check if .git exists
if (!(Test-Path ".git")) {
    Write-Host "📦 Initializing Git repository..." -ForegroundColor Yellow
    git init
}

# Check if remote already exists
$remoteExists = git remote | Select-String -Pattern "origin"

if ($remoteExists) {
    Write-Host "⚠️  Remote 'origin' already exists. Removing..." -ForegroundColor Yellow
    git remote remove origin
}

# Add remote
$repoUrl = "https://github.com/$GitHubUsername/PetHealthCare.git"
Write-Host "🔗 Adding remote: $repoUrl" -ForegroundColor Cyan
git remote add origin $repoUrl

# Verify remote
Write-Host ""
Write-Host "✅ Remote added successfully:" -ForegroundColor Green
git remote -v

# Rename branch to main
Write-Host ""
Write-Host "🌿 Renaming branch to 'main'..." -ForegroundColor Cyan
git branch -M main

# Show current status
Write-Host ""
Write-Host "📊 Current Git status:" -ForegroundColor Cyan
git status

# Ask for confirmation
Write-Host ""
Write-Host "⚠️  SECURITY WARNING:" -ForegroundColor Yellow
Write-Host "Your appsettings.json contains database credentials!" -ForegroundColor Yellow
Write-Host "Consider removing it before pushing or using environment variables." -ForegroundColor Yellow
Write-Host ""
$confirm = Read-Host "Do you want to proceed with push? (yes/no)"

if ($confirm -eq "yes" -or $confirm -eq "y") {
    Write-Host ""
    Write-Host "🚀 Pushing to GitHub..." -ForegroundColor Cyan
    Write-Host "You may be prompted for GitHub credentials." -ForegroundColor Yellow
    Write-Host ""
    
    git push -u origin main
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Successfully pushed to GitHub!" -ForegroundColor Green
        Write-Host "🌐 View your repository at: $repoUrl" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "📝 Next steps:" -ForegroundColor Yellow
        Write-Host "1. Add repository description and topics on GitHub" -ForegroundColor White
        Write-Host "2. Review README.md on your repository page" -ForegroundColor White
        Write-Host "3. Consider setting up GitHub Actions for CI/CD" -ForegroundColor White
    } else {
        Write-Host ""
        Write-Host "❌ Push failed. Please check the error message above." -ForegroundColor Red
        Write-Host ""
        Write-Host "Common issues:" -ForegroundColor Yellow
        Write-Host "1. Repository doesn't exist - Create it on GitHub first" -ForegroundColor White
        Write-Host "2. Authentication failed - Use Personal Access Token" -ForegroundColor White
        Write-Host "3. Permission denied - Check repository access" -ForegroundColor White
    }
} else {
    Write-Host ""
    Write-Host "❌ Push cancelled by user." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 To remove sensitive data from appsettings.json:" -ForegroundColor Cyan
    Write-Host "git rm --cached PetCare.API/appsettings.json" -ForegroundColor White
    Write-Host "git commit -m 'Remove appsettings.json from tracking'" -ForegroundColor White
}
