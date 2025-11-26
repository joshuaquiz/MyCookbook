# PowerShell script to build and deploy the Lambda functions

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Development"
)

Write-Host "Building Lambda Docker images..." -ForegroundColor Green
Write-Host "Note: Docker images will be built automatically by CDK during deployment" -ForegroundColor Yellow
Write-Host ""

# Verify Docker is running
try {
    docker info | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Docker is not running. Please start Docker Desktop and try again." -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Docker is not installed or not running. Please install Docker Desktop and try again." -ForegroundColor Red
    exit 1
}

Write-Host "Docker is running." -ForegroundColor Green
Write-Host ""
Write-Host "Deploying infrastructure..." -ForegroundColor Green
Write-Host "CDK will build the Docker images and push them to ECR automatically." -ForegroundColor Yellow
Write-Host ""

# Deploy the infrastructure
Push-Location "./MyCookbook.Infrastructure"
$env:ENVIRONMENT = $Environment
cdk bootstrap --profile joshua --verbose
cdk deploy --require-approval never --profile joshua --verbose
if ($LASTEXITCODE -ne 0) {
    Write-Host "Deployment failed!" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "To get the API Gateway URL, run:" -ForegroundColor Yellow
Write-Host "aws cloudformation describe-stacks --stack-name MyCookbookInfrastructure-$Environment --query ""Stacks[0].Outputs"" --profile joshua"

