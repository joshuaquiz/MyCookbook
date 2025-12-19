# Test API Authentication

$apiUrl = "http://api-development-mycookbook.g3software.net"

Write-Host "Testing API Authentication..." -ForegroundColor Cyan
Write-Host ""

# Test 1: Health check (should work without auth)
Write-Host "Test 1: Health check (no auth required)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/healthz" -UseBasicParsing -TimeoutSec 5
    Write-Host "Success: $($response.StatusCode) - $($response.Content)" -ForegroundColor Green
}
catch {
    Write-Host "Health check failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 2: Try to access protected endpoint without auth (should fail with 401)
Write-Host "Test 2: Access protected endpoint without auth (should fail with 401)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/api/Home/Popular" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    Write-Host "Unexpected success: $($response.StatusCode)" -ForegroundColor Red
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 401) {
        Write-Host "Correctly rejected with 401 Unauthorized" -ForegroundColor Green
    }
    else {
        Write-Host "Failed with status code $statusCode" -ForegroundColor Red
    }
}
Write-Host ""

# Test 3: Login and get JWT token
Write-Host "Test 3: Login with username/password" -ForegroundColor Yellow
$token = $null
try {
    $loginBody = @{
        Username = "testuser@mycookbook.com"
        Password = "TestPassword123!"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "$apiUrl/api/Account/LogIn" -Method Post -Body $loginBody -ContentType "application/json" -UseBasicParsing -TimeoutSec 5
    $loginResponse = $response.Content | ConvertFrom-Json
    $token = $loginResponse.AccessToken
    Write-Host "Login successful! Token: $($token.Substring(0, 20))..." -ForegroundColor Green
    Write-Host "  User: $($loginResponse.UserProfile.Name) ($($loginResponse.UserProfile.Email))" -ForegroundColor Gray
}
catch {
    Write-Host "Login failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 4: Access protected endpoint with JWT token (should succeed)
if ($token) {
    Write-Host "Test 4: Access protected endpoint with JWT token" -ForegroundColor Yellow
    try {
        $headers = @{
            Authorization = "Bearer $token"
        }
        $response = Invoke-WebRequest -Uri "$apiUrl/api/Home/Popular" -Headers $headers -UseBasicParsing -TimeoutSec 5
        $recipes = $response.Content | ConvertFrom-Json
        Write-Host "Successfully accessed protected endpoint!" -ForegroundColor Green
        Write-Host "  Retrieved $($recipes.Count) popular recipes" -ForegroundColor Gray
    }
    catch {
        Write-Host "Failed to access protected endpoint: $_" -ForegroundColor Red
    }
}
else {
    Write-Host "Test 4: Skipped (no token available)" -ForegroundColor Gray
}
Write-Host ""

Write-Host "Authentication tests complete!" -ForegroundColor Cyan

