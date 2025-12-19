using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using G3.Maui.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using MyCookbook.App.Interfaces;
using MyCookbook.App.Services;
using MyCookbook.App.ViewModels;
using MyCookbook.App.Views;

namespace MyCookbook.App.Implementations;

public sealed class CookbookHttpClient(
    IConnectivity connectivity,
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<CookbookHttpClient> logger)
    : BaseHttpClient(connectivity, httpClient, memoryCache, logger)
{
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// Helper method to create an absolute URI from a relative path
    /// </summary>
    private Uri MakeAbsoluteUri(string relativePath)
    {
        if (_httpClient.BaseAddress == null)
        {
            throw new InvalidOperationException("HttpClient.BaseAddress is not set");
        }

        return new Uri(_httpClient.BaseAddress, relativePath);
    }

    /// <summary>
    /// GET request with string path (converted to absolute URI)
    /// Catches 401 Unauthorized errors and redirects to login
    /// Catches timeout and network errors gracefully
    /// </summary>
    public async ValueTask<T> Get<T>(string path, CancellationToken cancellationToken)
    {
        logger.LogInformation("HTTP GET: {Path} (Type: {Type})", path, typeof(T).Name);

        try
        {
            var result = await base.Get<T>(MakeAbsoluteUri(path), cancellationToken);
            logger.LogInformation("HTTP GET SUCCESS: {Path} (Type: {Type})", path, typeof(T).Name);
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(ex, "HTTP GET UNAUTHORIZED: {Path} - Redirecting to login", path);
            await HandleUnauthorizedAsync();
            // Return default value to prevent crash
            return default!;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "HTTP GET TIMEOUT: {Path} - Request timeout or cancelled", path);
            // Return default value to prevent crash
            return default!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP GET ERROR: {Path} - {Message}", path, ex.Message);
            // Return default value to prevent crash
            return default!;
        }
    }

    /// <summary>
    /// POST request with string path (converted to absolute URI)
    /// Catches 401 Unauthorized errors and redirects to login
    /// Catches timeout and network errors gracefully
    /// </summary>
    public async ValueTask<TResponse> Post<TResponse, TRequest>(string path, TRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("HTTP POST: {Path} (Request: {RequestType}, Response: {ResponseType})",
            path, typeof(TRequest).Name, typeof(TResponse).Name);

        try
        {
            var result = await base.Post<TResponse, TRequest>(MakeAbsoluteUri(path), request, cancellationToken);
            logger.LogInformation("HTTP POST SUCCESS: {Path} (Response: {ResponseType})",
                path, typeof(TResponse).Name);
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(ex, "HTTP POST UNAUTHORIZED: {Path} - Redirecting to login", path);
            await HandleUnauthorizedAsync();
            // Return default value to prevent crash
            return default!;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "HTTP POST TIMEOUT: {Path} - Request timeout or cancelled", path);
            // Return default value to prevent crash
            return default!;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP POST ERROR: {Path} - {Message}", path, ex.Message);
            // Return default value to prevent crash
            return default!;
        }
    }

    /// <summary>
    /// GET request with JSON deserialization (for compatibility with existing code)
    /// Catches 401 Unauthorized errors and redirects to login
    /// Catches timeout and network errors gracefully
    /// </summary>
    public async Task<T?> GetFromJsonAsync<T>(string path)
    {
        logger.LogInformation("HTTP GET (JSON): {Path} (Type: {Type})", path, typeof(T).Name);

        try
        {
            var result = await _httpClient.GetFromJsonAsync<T>(path);
            logger.LogInformation("HTTP GET (JSON) SUCCESS: {Path} (Type: {Type})", path, typeof(T).Name);
            return result;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            logger.LogWarning(ex, "HTTP GET (JSON) UNAUTHORIZED: {Path} - Redirecting to login", path);
            await HandleUnauthorizedAsync();
            // Return default value to prevent crash
            return default;
        }
        catch (TaskCanceledException ex)
        {
            logger.LogWarning(ex, "HTTP GET (JSON) TIMEOUT: {Path} - Request timeout or cancelled", path);
            // Return default value to prevent crash
            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP GET (JSON) ERROR: {Path} - {Message}", path, ex.Message);
            // Return default value to prevent crash
            return default;
        }
    }

    /// <summary>
    /// Handles unauthorized access by redirecting to the login page
    /// </summary>
    private static async Task HandleUnauthorizedAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                // Get the service provider to resolve dependencies
                var serviceProvider = Application.Current?.Handler?.MauiContext?.Services;
                if (serviceProvider != null)
                {
                    var loginViewModel = serviceProvider.GetService(typeof(LoginViewModel)) as LoginViewModel;
                    var cognitoAuthService = serviceProvider.GetService(typeof(ICognitoAuthService)) as ICognitoAuthService;
                    var cookbookStorage = serviceProvider.GetService(typeof(ICookbookStorage)) as ICookbookStorage;

                    if (loginViewModel != null)
                    {
                        if (Application.Current?.Windows.Count > 0)
                        {
                            var loginPage = new Login(loginViewModel);
                            Application.Current.Windows[0].Page = loginPage;

                            await loginPage.DisplayAlertAsync(
                                "Session Expired",
                                "Your session has expired. Please log in again.",
                                "OK");
                        }
                    }
                }
            }
            catch
            {
                // If we can't redirect, the storage has already been cleared by the delegating handler
                // The user will be prompted to login on next navigation
            }
        });
    }
}