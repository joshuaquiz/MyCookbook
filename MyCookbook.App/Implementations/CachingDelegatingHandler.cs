using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.App.Interfaces;
using MyCookbook.App.Services;

namespace MyCookbook.App.Implementations;

/// <summary>
/// Delegating handler that caches responses for 3 seconds.
/// </summary>
public sealed class CachingDelegatingHandler(
    ICognitoAuthService? cognitoAuthService = null,
    ICookbookStorage? cookbookStorage = null)
    : DelegatingHandler
{
    private readonly ConcurrentDictionary<string, CachedResponse> _cache = new();
    private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(3);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
        {
            return await SendWithAuthAsync(request, cancellationToken);
        }

        var cacheKey = GetCacheKey(request);
        if (_cache.TryGetValue(cacheKey, out var cachedResponse))
        {
            if (DateTime.UtcNow - cachedResponse.Timestamp < _cacheDuration)
            {
                return await CloneHttpResponseMessageAsync(cachedResponse.Response);
            }

            _cache.TryRemove(cacheKey, out _);
        }

        var response = await SendWithAuthAsync(request, cancellationToken);
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
        string? token = null;
        if (cognitoAuthService != null)
        {
            token = await cognitoAuthService.GetAccessTokenAsync();
        }

        if (string.IsNullOrEmpty(token) && cookbookStorage != null)
        {
            token = await cookbookStorage.GetAccessToken();
        }

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && cookbookStorage != null)
        {
            await cookbookStorage.Empty();
        }

        return response;
    }

    private static string GetCacheKey(
        HttpRequestMessage request) =>
        $"{request.Method}:{request.RequestUri}";

    private static async Task<HttpResponseMessage> CloneHttpResponseMessageAsync(
        HttpResponseMessage response)
    {
        var contentBytes = await response.Content.ReadAsByteArrayAsync();
        var cloned = new HttpResponseMessage(response.StatusCode)
        {
            Content = new ByteArrayContent(contentBytes),
            ReasonPhrase = response.ReasonPhrase,
            Version = response.Version
        };

        foreach (var header in response.Headers)
        {
            cloned.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var header in response.Content.Headers)
        {
            cloned.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        response.Content = new ByteArrayContent(contentBytes);
        foreach (var header in cloned.Content.Headers)
        {
            response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return cloned;
    }

    private sealed class CachedResponse
    {
        public required HttpResponseMessage Response { get; init; }

        public DateTime Timestamp { get; init; }
    }
}

