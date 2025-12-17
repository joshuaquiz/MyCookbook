#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sets up OAuth provider secrets in AWS Secrets Manager for MyCookbook

.DESCRIPTION
    This script creates or updates OAuth provider secrets (Google and Facebook) 
    in AWS Secrets Manager. The secrets are used by the CDK infrastructure 
    to configure Cognito User Pool Identity Providers.

.PARAMETER GoogleClientId
    Google OAuth Client ID

.PARAMETER GoogleClientSecret
    Google OAuth Client Secret

.PARAMETER FacebookAppId
    Facebook App ID

.PARAMETER FacebookAppSecret
    Facebook App Secret

.PARAMETER Region
    AWS Region where secrets will be stored (default: us-east-1)

.PARAMETER Update
    If specified, updates existing secrets instead of creating new ones

.EXAMPLE
    .\setup-oauth-secrets.ps1 -GoogleClientId "your-client-id" -GoogleClientSecret "your-secret"

.EXAMPLE
    .\setup-oauth-secrets.ps1 -Update -GoogleClientId "new-client-id" -GoogleClientSecret "new-secret"
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$GoogleClientId,

    [Parameter(Mandatory=$false)]
    [string]$GoogleClientSecret,

    [Parameter(Mandatory=$false)]
    [string]$FacebookAppId,

    [Parameter(Mandatory=$false)]
    [string]$FacebookAppSecret,

    [Parameter(Mandatory=$false)]
    [string]$Region = "us-east-1",

    [Parameter(Mandatory=$false)]
    [switch]$Update
)

$ErrorActionPreference = "Stop"

Write-Host "MyCookbook OAuth Secrets Setup" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Check if AWS CLI is installed
try {
    $null = aws --version
} catch {
    Write-Error "AWS CLI is not installed or not in PATH. Please install AWS CLI first."
    exit 1
}

# Function to create or update a secret
function Set-OAuthSecret {
    param(
        [string]$SecretName,
        [string]$ClientId,
        [string]$ClientSecret,
        [string]$Description
    )

    if ([string]::IsNullOrWhiteSpace($ClientId) -or [string]::IsNullOrWhiteSpace($ClientSecret)) {
        Write-Host "Skipping $SecretName (credentials not provided)" -ForegroundColor Yellow
        return
    }

    $secretJson = @{
        client_id = $ClientId
        client_secret = $ClientSecret
    } | ConvertTo-Json -Compress

    try {
        if ($Update) {
            Write-Host "Updating secret: $SecretName..." -ForegroundColor Green
            aws secretsmanager update-secret `
                --secret-id $SecretName `
                --secret-string $secretJson `
                --region $Region 2>&1 | Out-Null
            Write-Host "✓ Updated $SecretName" -ForegroundColor Green
        } else {
            Write-Host "Creating secret: $SecretName..." -ForegroundColor Green
            aws secretsmanager create-secret `
                --name $SecretName `
                --description $Description `
                --secret-string $secretJson `
                --region $Region 2>&1 | Out-Null
            Write-Host "✓ Created $SecretName" -ForegroundColor Green
        }
    } catch {
        if ($_.Exception.Message -like "*ResourceExistsException*") {
            Write-Host "Secret $SecretName already exists. Use -Update to update it." -ForegroundColor Yellow
        } else {
            Write-Error "Failed to set secret $SecretName: $_"
        }
    }
}

# Set up Google OAuth secret
if ($GoogleClientId -or $GoogleClientSecret) {
    Set-OAuthSecret `
        -SecretName "mycookbook/oauth/google" `
        -ClientId $GoogleClientId `
        -ClientSecret $GoogleClientSecret `
        -Description "Google OAuth credentials for MyCookbook"
}

# Set up Facebook OAuth secret
if ($FacebookAppId -or $FacebookAppSecret) {
    Set-OAuthSecret `
        -SecretName "mycookbook/oauth/facebook" `
        -ClientId $FacebookAppId `
        -ClientSecret $FacebookAppSecret `
        -Description "Facebook OAuth credentials for MyCookbook"
}

Write-Host ""
Write-Host "Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "To verify the secrets were created:" -ForegroundColor Cyan
Write-Host "  aws secretsmanager list-secrets --region $Region" -ForegroundColor Gray
Write-Host ""
Write-Host "To view a secret value:" -ForegroundColor Cyan
Write-Host "  aws secretsmanager get-secret-value --secret-id mycookbook/oauth/google --region $Region" -ForegroundColor Gray
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Verify the secrets are correct" -ForegroundColor Gray
Write-Host "  2. Deploy the CDK infrastructure: cdk deploy --all" -ForegroundColor Gray
Write-Host ""

