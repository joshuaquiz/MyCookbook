# Optimized deployment script for MyCookbook Infrastructure
# This script uses Docker BuildKit and CDK optimizations for faster deployments

param(
    [string]$Environment = "Development",
    [string]$Profile = "joshua"
)

Write-Host "Starting optimized deployment for $Environment environment..." -ForegroundColor Green

# Enable Docker BuildKit for faster builds
$env:DOCKER_BUILDKIT = "1"
$env:BUILDKIT_PROGRESS = "plain"

# Set environment variable for CDK
$env:ENVIRONMENT = $Environment

# Build Lambda functions first (if needed)
Write-Host "`nBuilding Lambda functions..." -ForegroundColor Yellow
Push-Location ..\MyCookbook.Lambda
dotnet publish -c Release
Pop-Location

# Deploy with CDK using optimized settings
Write-Host "`nDeploying infrastructure with CDK..." -ForegroundColor Yellow
cdk deploy `
    --require-approval never `
    --profile $Profile `
    --outputs-file deploy-output.json `
    --verbose

Write-Host "`nDeployment complete!" -ForegroundColor Green
Write-Host "Check deploy-output.json for stack outputs" -ForegroundColor Cyan

