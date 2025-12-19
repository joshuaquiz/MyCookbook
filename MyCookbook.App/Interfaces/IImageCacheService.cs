using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyCookbook.App.Interfaces;

/// <summary>
/// Service for caching images locally with size limits and expiration
/// </summary>
public interface IImageCacheService
{
    /// <summary>
    /// Get cached image path or download and cache if not available
    /// </summary>
    Task<string?> GetCachedImagePathAsync(Uri imageUrl, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clear all cached images
    /// </summary>
    Task ClearCacheAsync();
    
    /// <summary>
    /// Clear expired images (older than specified duration)
    /// </summary>
    Task ClearExpiredImagesAsync(TimeSpan maxAge);
    
    /// <summary>
    /// Get current cache size in bytes
    /// </summary>
    Task<long> GetCacheSizeAsync();
    
    /// <summary>
    /// Enforce cache size limit by removing oldest images
    /// </summary>
    Task EnforceCacheSizeLimitAsync(long maxSizeBytes);
}

