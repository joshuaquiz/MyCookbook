using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Storage;

namespace MyCookbook.App.Services;

/// <summary>
/// Implementation of Cognito OAuth authentication using MAUI WebAuthenticator
/// </summary>
public class CognitoAuthService : ICognitoAuthService
{
    private readonly ISecureStorage _secureStorage;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    private const string AccessTokenKey = "cognito_access_token";
    private const string IdTokenKey = "cognito_id_token";
    private const string RefreshTokenKey = "cognito_refresh_token";
    private const string ExpiresAtKey = "cognito_expires_at";

    // These will be loaded from configuration after deployment
    private readonly string _cognitoDomain;
    private readonly string _clientId;
    private readonly string _redirectUri = "mycookbook://oauth/callback";

    public CognitoAuthService(
        ISecureStorage secureStorage,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _secureStorage = secureStorage;
        _configuration = configuration;
        _httpClient = httpClient;

        // Load from configuration (you'll need to add these to appsettings.json after deployment)
        _cognitoDomain = configuration["Cognito:Domain"] ?? "mycookbook-development.auth.us-east-1.amazoncognito.com";
        // TEMPORARY: Bypass Cognito configuration requirement for development
        _clientId = configuration["Cognito:ClientId"] ?? "temporary-bypass-client-id";
    }

    public async Task<CognitoAuthResult> AuthenticateWithGoogleAsync()
    {
        return await AuthenticateWithProviderAsync("Google");
    }

    public async Task<CognitoAuthResult> AuthenticateWithFacebookAsync()
    {
        return await AuthenticateWithProviderAsync("Facebook");
    }

    private async Task<CognitoAuthResult> AuthenticateWithProviderAsync(string provider)
    {
        try
        {
            // Build the Cognito Hosted UI URL with identity provider
            var authUrl = $"https://{_cognitoDomain}/oauth2/authorize?" +
                         $"identity_provider={provider}&" +
                         $"redirect_uri={Uri.EscapeDataString(_redirectUri)}&" +
                         $"response_type=code&" +
                         $"client_id={_clientId}&" +
                         $"scope=email+openid+profile";

            var callbackUrl = new Uri(_redirectUri);

            // Launch the browser for OAuth authentication
            var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authUrl),
                callbackUrl);

            // Extract the authorization code
            if (!authResult.Properties.TryGetValue("code", out var code))
            {
                return new CognitoAuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = "No authorization code received"
                };
            }

            // Exchange the code for tokens
            var tokenResult = await ExchangeCodeForTokensAsync(code);
            if (!tokenResult.IsSuccess)
            {
                return tokenResult;
            }

            // Store tokens securely
            await StoreTokensAsync(tokenResult);

            return tokenResult;
        }
        catch (TaskCanceledException)
        {
            return new CognitoAuthResult
            {
                IsSuccess = false,
                ErrorMessage = "Authentication cancelled by user"
            };
        }
        catch (Exception ex)
        {
            return new CognitoAuthResult
            {
                IsSuccess = false,
                ErrorMessage = $"Authentication failed: {ex.Message}"
            };
        }
    }

    private async Task<CognitoAuthResult> ExchangeCodeForTokensAsync(string code)
    {
        try
        {
            var tokenUrl = $"https://{_cognitoDomain}/oauth2/token";

            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "client_id", _clientId },
                { "code", code },
                { "redirect_uri", _redirectUri }
            };

            var response = await _httpClient.PostAsync(
                tokenUrl,
                new FormUrlEncodedContent(requestBody));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return new CognitoAuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Token exchange failed: {error}"
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

            if (tokenResponse == null)
            {
                return new CognitoAuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Failed to parse token response"
                };
            }

            return new CognitoAuthResult
            {
                IsSuccess = true,
                AccessToken = tokenResponse.access_token,
                IdToken = tokenResponse.id_token,
                RefreshToken = tokenResponse.refresh_token,
                ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in)
            };
        }
        catch (Exception ex)
        {
            return new CognitoAuthResult
            {
                IsSuccess = false,
                ErrorMessage = $"Token exchange error: {ex.Message}"
            };
        }
    }

    private async Task StoreTokensAsync(CognitoAuthResult result)
    {
        if (result.AccessToken != null)
            await _secureStorage.SetAsync(AccessTokenKey, result.AccessToken);

        if (result.IdToken != null)
            await _secureStorage.SetAsync(IdTokenKey, result.IdToken);

        if (result.RefreshToken != null)
            await _secureStorage.SetAsync(RefreshTokenKey, result.RefreshToken);

        if (result.ExpiresAt.HasValue)
            await _secureStorage.SetAsync(ExpiresAtKey, result.ExpiresAt.Value.ToString("O"));
    }

    public async Task SignOutAsync()
    {
        _secureStorage.Remove(AccessTokenKey);
        _secureStorage.Remove(IdTokenKey);
        _secureStorage.Remove(RefreshTokenKey);
        _secureStorage.Remove(ExpiresAtKey);

        // Optional: Call Cognito logout endpoint
        var logoutUrl = $"https://{_cognitoDomain}/logout?" +
                       $"client_id={_clientId}&" +
                       $"logout_uri={Uri.EscapeDataString(_redirectUri)}";

        // This will open the browser to complete logout
        await Browser.Default.OpenAsync(logoutUrl, BrowserLaunchMode.SystemPreferred);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var accessToken = await _secureStorage.GetAsync(AccessTokenKey);
        var expiresAtStr = await _secureStorage.GetAsync(ExpiresAtKey);

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(expiresAtStr))
        {
            return null;
        }

        // Check if token is expired
        if (DateTime.TryParse(expiresAtStr, out var expiresAt))
        {
            if (DateTime.UtcNow >= expiresAt.AddMinutes(-5)) // Refresh 5 minutes before expiry
            {
                // Token expired, try to refresh
                var refreshed = await RefreshTokenAsync();
                if (refreshed)
                {
                    return await _secureStorage.GetAsync(AccessTokenKey);
                }
                return null;
            }
        }

        return accessToken;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetAccessTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    private async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _secureStorage.GetAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            var tokenUrl = $"https://{_cognitoDomain}/oauth2/token";

            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", _clientId },
                { "refresh_token", refreshToken }
            };

            var response = await _httpClient.PostAsync(
                tokenUrl,
                new FormUrlEncodedContent(requestBody));

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json);

            if (tokenResponse == null)
            {
                return false;
            }

            var result = new CognitoAuthResult
            {
                IsSuccess = true,
                AccessToken = tokenResponse.access_token,
                IdToken = tokenResponse.id_token,
                RefreshToken = refreshToken, // Refresh token doesn't change
                ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in)
            };

            await StoreTokensAsync(result);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private class TokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string id_token { get; set; } = string.Empty;
        public string? refresh_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; } = string.Empty;
    }
}
