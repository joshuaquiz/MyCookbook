using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using MyCookbook.App.Interfaces;

namespace MyCookbook.App.Implementations;

/// <summary>
/// Image caching service with 1-day expiration and 100MB size limit
/// </summary>
public class ImageCacheService : IImageCacheService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageCacheService> _logger;
    private readonly string _cacheDirectory;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromDays(1);
    private readonly long _maxCacheSizeBytes = 100 * 1024 * 1024; // 100MB
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private bool _disposed;

    public ImageCacheService(ILogger<ImageCacheService> logger)
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30) // 30 second timeout for image downloads
        };
        _logger = logger;
        _cacheDirectory = Path.Combine(FileSystem.CacheDirectory, "image_cache");
        Directory.CreateDirectory(_cacheDirectory);
    }

    public async Task<string?> GetCachedImagePathAsync(Uri imageUrl, CancellationToken cancellationToken = default)
    {
        if (imageUrl == null)
        {
            return null;
        }

        try
        {
            var fileName = GetCacheFileName(imageUrl);
            var filePath = Path.Combine(_cacheDirectory, fileName);

            // Check if file exists and is not expired
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if (DateTime.UtcNow - fileInfo.LastWriteTimeUtc < _defaultExpiration)
                {
                    _logger.LogDebug("Image cache hit: {Url}", imageUrl);
                    return filePath;
                }

                // File is expired, delete it
                File.Delete(filePath);
            }

            // Download and cache the image
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                // Double-check after acquiring lock
                if (File.Exists(filePath))
                {
                    return filePath;
                }

                _logger.LogDebug("Downloading image: {Url}", imageUrl);
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl, cancellationToken);
                await File.WriteAllBytesAsync(filePath, imageBytes, cancellationToken);

                // Enforce cache size limit
                await EnforceCacheSizeLimitAsync(_maxCacheSizeBytes);

                return filePath;
            }
            finally
            {
                _cacheLock.Release();
            }
        }
        catch (TaskCanceledException)
        {
            // Image download was cancelled, don't log as error
            _logger.LogDebug("Image download cancelled: {Url}", imageUrl);
            return null;
        }
        catch (OperationCanceledException)
        {
            // Image download was cancelled, don't log as error
            _logger.LogDebug("Image download cancelled: {Url}", imageUrl);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache image: {Url}", imageUrl);
            return null;
        }
    }

    public Task ClearCacheAsync()
    {
        try
        {
            if (Directory.Exists(_cacheDirectory))
            {
                Directory.Delete(_cacheDirectory, true);
                Directory.CreateDirectory(_cacheDirectory);
            }
            _logger.LogInformation("Image cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear image cache");
        }
        return Task.CompletedTask;
    }

    public Task ClearExpiredImagesAsync(TimeSpan maxAge)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow - maxAge;
            var files = Directory.GetFiles(_cacheDirectory);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTimeUtc < cutoffTime)
                {
                    File.Delete(file);
                }
            }
            _logger.LogInformation("Expired images cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear expired images");
        }
        return Task.CompletedTask;
    }

    public Task<long> GetCacheSizeAsync()
    {
        try
        {
            var files = Directory.GetFiles(_cacheDirectory);
            return Task.FromResult(files.Sum(f => new FileInfo(f).Length));
        }
        catch
        {
            return Task.FromResult(0L);
        }
    }

    public Task EnforceCacheSizeLimitAsync(long maxSizeBytes)
    {
        try
        {
            var files = Directory.GetFiles(_cacheDirectory)
                .Select(f => new FileInfo(f))
                .OrderBy(f => f.LastAccessTimeUtc)
                .ToList();

            var totalSize = files.Sum(f => f.Length);

            // Remove oldest files until we're under the limit
            while (totalSize > maxSizeBytes && files.Count > 0)
            {
                var oldestFile = files[0];
                totalSize -= oldestFile.Length;
                oldestFile.Delete();
                files.RemoveAt(0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enforce cache size limit");
        }
        return Task.CompletedTask;
    }

    private static string GetCacheFileName(Uri url)
    {
        // Create a hash of the URL to use as filename
        var urlBytes = Encoding.UTF8.GetBytes(url.AbsoluteUri);
        var hashBytes = SHA256.HashData(urlBytes);
        var hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

        // Try to preserve file extension
        var extension = Path.GetExtension(url.LocalPath);
        if (string.IsNullOrEmpty(extension) || extension.Length > 5)
        {
            extension = ".jpg"; // Default extension
        }

        return $"{hash}{extension}";
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _httpClient?.Dispose();
            _cacheLock?.Dispose();
        }

        _disposed = true;
    }
}

