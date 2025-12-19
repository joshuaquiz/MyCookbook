using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MyCookbook.App.Interfaces;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Implementations;

public sealed class CookbookStorage(
    ISecureStorage secureStorage,
    IPreferences preferences,
    HttpClient httpClient)
    : ICookbookStorage
{
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private Task<bool>? _ongoingRefresh;
    public ValueTask<AppTheme> GetCurrentAppTheme(
        Application application)
    {
        var value = preferences.Get(
            nameof(AppTheme),
            (int) application.UserAppTheme);
        return ValueTask.FromResult(
            (AppTheme) value);
    }

    public Task SetAppTheme(
        AppTheme appTheme,
        Application application)
    {
        preferences.Set(
            nameof(AppTheme),
            (int) appTheme);
        application.UserAppTheme = appTheme;
        return Task.CompletedTask;
    }

    public Task Empty()
    {
        preferences.Clear();
        secureStorage.RemoveAll();
        return Task.CompletedTask;
    }

    public async Task SetUser(
        UserProfileModel user) =>
        await secureStorage.SetAsync(
            "UserProfile",
            JsonSerializer.Serialize(
                user));

    public async ValueTask<UserProfileModel?> GetUser()
    {
        var profileAsString = secureStorage.GetAsync(
                "UserProfile")
            .GetAwaiter()
            .GetResult();
        return profileAsString == null
            ? null
            : JsonSerializer.Deserialize<UserProfileModel>(
                profileAsString);
    }

    public async Task SetAccessToken(string accessToken, int expiresIn)
    {
        await secureStorage.SetAsync("JWT_AccessToken", accessToken);
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);
        await secureStorage.SetAsync("JWT_ExpiresAt", expiresAt.ToString("O"));
    }

    public async ValueTask<string?> GetAccessToken()
    {
        var accessToken = await secureStorage.GetAsync("JWT_AccessToken");
        var expiresAtStr = await secureStorage.GetAsync("JWT_ExpiresAt");

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(expiresAtStr))
        {
            return null;
        }

        // Check if token is expired or will expire in the next 5 minutes
        if (DateTime.TryParse(expiresAtStr, out var expiresAt))
        {
            // Refresh 5 minutes before expiry
            if (DateTime.UtcNow >= expiresAt.AddMinutes(-5))
            {
                // Try to refresh the token
                var refreshed = await RefreshAccessTokenAsync();
                if (refreshed)
                {
                    // Return the new access token
                    return await secureStorage.GetAsync("JWT_AccessToken");
                }

                // Refresh failed, return null to trigger logout
                return null;
            }
        }

        return accessToken;
    }

    public async Task SetRefreshToken(string refreshToken)
    {
        await secureStorage.SetAsync("JWT_RefreshToken", refreshToken);
    }

    public async ValueTask<string?> GetRefreshToken()
    {
        return await secureStorage.GetAsync("JWT_RefreshToken");
    }

    private async Task<bool> RefreshAccessTokenAsync()
    {
        // Prevent concurrent refresh attempts
        if (_ongoingRefresh != null)
        {
            return await _ongoingRefresh;
        }

        await _refreshLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_ongoingRefresh != null)
            {
                return await _ongoingRefresh;
            }

            _ongoingRefresh = RefreshAccessTokenWithRetryAsync();
            return await _ongoingRefresh;
        }
        finally
        {
            _ongoingRefresh = null;
            _refreshLock.Release();
        }
    }

    private async Task<bool> RefreshAccessTokenWithRetryAsync()
    {
        const int maxRetries = 3;
        var delay = TimeSpan.FromSeconds(1);

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var refreshToken = await GetRefreshToken();
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return false;
                }

                var apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://api-development-mycookbook.g3software.net";
                var refreshRequest = new RefreshTokenRequest(refreshToken);

                var response = await httpClient.PostAsJsonAsync($"{apiBaseUrl}/api/Account/RefreshToken", refreshRequest);

                if (!response.IsSuccessStatusCode)
                {
                    // Don't retry on 401 (invalid refresh token)
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return false;
                    }

                    // Retry on other errors
                    if (attempt < maxRetries - 1)
                    {
                        await Task.Delay(delay);
                        delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2); // Exponential backoff
                        continue;
                    }

                    return false;
                }

                var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
                if (refreshResponse == null)
                {
                    return false;
                }

                // Store the new tokens
                await SetAccessToken(refreshResponse.AccessToken, refreshResponse.ExpiresIn);
                await SetRefreshToken(refreshResponse.RefreshToken);

                return true;
            }
            catch (Exception)
            {
                // Retry on network errors
                if (attempt < maxRetries - 1)
                {
                    await Task.Delay(delay);
                    delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2); // Exponential backoff
                    continue;
                }

                return false;
            }
        }

        return false;
    }
}