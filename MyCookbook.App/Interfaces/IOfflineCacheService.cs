using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Interfaces;

/// <summary>
/// Service for caching data offline using SQLite
/// </summary>
public interface IOfflineCacheService
{
    /// <summary>
    /// Cache a recipe for offline access
    /// </summary>
    Task CacheRecipeAsync(RecipeModel recipe);
    
    /// <summary>
    /// Get a cached recipe by GUID
    /// </summary>
    Task<RecipeModel?> GetCachedRecipeAsync(Guid recipeGuid);
    
    /// <summary>
    /// Cache a list of recipe summaries
    /// </summary>
    Task CacheRecipeSummariesAsync(List<RecipeSummaryViewModel> recipes, string cacheKey);
    
    /// <summary>
    /// Get cached recipe summaries by cache key
    /// </summary>
    Task<List<RecipeSummaryViewModel>?> GetCachedRecipeSummariesAsync(string cacheKey);
    
    /// <summary>
    /// Check if a recipe is cached
    /// </summary>
    Task<bool> IsRecipeCachedAsync(Guid recipeGuid);
    
    /// <summary>
    /// Get the last cache time for a specific cache key
    /// </summary>
    Task<DateTime?> GetCacheTimeAsync(string cacheKey);
    
    /// <summary>
    /// Clear all cached data
    /// </summary>
    Task ClearAllCacheAsync();
    
    /// <summary>
    /// Clear expired cache entries (older than specified duration)
    /// </summary>
    Task ClearExpiredCacheAsync(TimeSpan maxAge);
    
    /// <summary>
    /// Get all cached recipes for offline browsing
    /// </summary>
    Task<List<RecipeModel>> GetAllCachedRecipesAsync();
}

