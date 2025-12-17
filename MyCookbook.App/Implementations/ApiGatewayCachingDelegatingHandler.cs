using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.App.Services;

namespace MyCookbook.App.Implementations;

/// <summary>
/// Delegating handler that caches API Gateway responses for 3 seconds.
/// This wraps the HTTP client that points to the API Gateway.
/// </summary>
public sealed class ApiGatewayCachingDelegatingHandler : DelegatingHandler
{
    private readonly ICognitoAuthService? _cognitoAuthService;
    private readonly ConcurrentDictionary<string, CachedResponse> _cache = new();
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(3);

    public ApiGatewayCachingDelegatingHandler(ICognitoAuthService? cognitoAuthService = null)
    {
        _cognitoAuthService = cognitoAuthService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Only cache GET requests
        if (request.Method != HttpMethod.Get)
        {
            return await SendWithAuthAsync(request, cancellationToken);
        }

        var cacheKey = GetCacheKey(request);

        // Check if we have a valid cached response
        if (_cache.TryGetValue(cacheKey, out var cachedResponse))
        {
            if (DateTime.UtcNow - cachedResponse.Timestamp < _cacheDuration)
            {
                // Return cached response
                return CloneHttpResponseMessage(cachedResponse.Response);
            }

            // Remove expired cache entry
            _cache.TryRemove(cacheKey, out _);
        }

        // Make the actual request
        var response = await SendWithAuthAsync(request, cancellationToken);

        // Cache successful responses
        if (response.IsSuccessStatusCode)
        {
            var clonedResponse = await CloneHttpResponseMessageAsync(response);
            _cache[cacheKey] = new CachedResponse
            {
                Response = clonedResponse,
                Timestamp = DateTime.UtcNow
            };
        }

        return response;
    }

    private async Task<HttpResponseMessage> SendWithAuthAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Add Cognito access token to Authorization header if available
        if (_cognitoAuthService != null)
        {
            var token = await _cognitoAuthService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private static string GetCacheKey(HttpRequestMessage request)
    {
        // Create cache key from method + URL + query string
        return $"{request.Method}:{request.RequestUri}";
    }

    private static HttpResponseMessage CloneHttpResponseMessage(HttpResponseMessage response)
    {
        var cloned = new HttpResponseMessage(response.StatusCode)
        {
            Content = response.Content,
            ReasonPhrase = response.ReasonPhrase,
            RequestMessage = response.RequestMessage,
            Version = response.Version
        };

        foreach (var header in response.Headers)
        {
            cloned.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return cloned;
    }

    private static async Task<HttpResponseMessage> CloneHttpResponseMessageAsync(HttpResponseMessage response)
    {
        // Read the content as bytes to cache it
        var contentBytes = await response.Content.ReadAsByteArrayAsync();

        var cloned = new HttpResponseMessage(response.StatusCode)
        {
            Content = new ByteArrayContent(contentBytes),
            ReasonPhrase = response.ReasonPhrase,
            Version = response.Version
        };

        // Copy headers
        foreach (var header in response.Headers)
        {
            cloned.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content headers
        foreach (var header in response.Content.Headers)
        {
            cloned.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Reset the original response content so it can still be read
        response.Content = new ByteArrayContent(contentBytes);
        foreach (var header in cloned.Content.Headers)
        {
            response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return cloned;
    }

    private sealed class CachedResponse
    {
        public HttpResponseMessage Response { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}

