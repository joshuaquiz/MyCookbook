# PowerShell script to generate a password hash for the auth configuration

param(
    [Parameter(Mandatory=$true)]
    [string]$Password
)

$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($Password)
$hash = $sha256.ComputeHash($bytes)
$passwordHash = [Convert]::ToBase64String($hash)

Write-Host "Password Hash: $passwordHash"
Write-Host ""
Write-Host "Use this hash in your auth-config.json file:"
Write-Host ""
Write-Host @"
{
  "username": "your-username",
  "passwordHash": "$passwordHash",
  "field1": "value1",
  "field2": "value2"
}
"@

