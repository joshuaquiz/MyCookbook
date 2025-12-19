using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using MyCookbook.App.Database;
using MyCookbook.App.Interfaces;
using MyCookbook.App.ViewModels;
using MyCookbook.Common.ApiModels;
using SQLite;

namespace MyCookbook.App.Implementations;

/// <summary>
/// SQLite-based offline cache service for better performance than file-based caching
/// </summary>
public class SqliteCacheService : ISqliteCacheService, IDisposable
{
    private readonly ILogger<SqliteCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private SQLiteAsyncConnection? _database;
    private bool _initialized;
    private bool _disposed;

    public SqliteCacheService(ILogger<SqliteCacheService> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        try
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "offline_cache.db");
            _database = new SQLiteAsyncConnection(dbPath);

            await _database.CreateTableAsync<CachedRecipe>();
            await _database.CreateTableAsync<CachedRecipeSummary>();

            _initialized = true;
            _logger.LogInformation("SQLite cache database initialized at {Path}", dbPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SQLite cache database");
            throw;
        }
    }

    public async Task CacheRecipeAsync(RecipeModel recipe)
    {
        await EnsureInitializedAsync();

        try
        {
            var json = JsonSerializer.Serialize(recipe, _jsonOptions);
            var cached = new CachedRecipe
            {
                RecipeGuid = recipe.Guid,
                RecipeJson = json,
                CachedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };

            await _database!.InsertOrReplaceAsync(cached);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache recipe {RecipeGuid}", recipe.Guid);
        }
    }

    public async Task<RecipeModel?> GetCachedRecipeAsync(Guid recipeGuid)
    {
        await EnsureInitializedAsync();

        try
        {
            var cached = await _database!.Table<CachedRecipe>()
                .Where(r => r.RecipeGuid == recipeGuid)
                .FirstOrDefaultAsync();

            if (cached == null)
            {
                return null;
            }

            // Update last accessed time
            cached.LastAccessedAt = DateTime.UtcNow;
            await _database.UpdateAsync(cached);

            return JsonSerializer.Deserialize<RecipeModel>(cached.RecipeJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cached recipe {RecipeGuid}", recipeGuid);
            return null;
        }
    }

    public async Task CacheRecipeSummariesAsync(List<RecipeSummaryViewModel> recipes, string cacheKey)
    {
        await EnsureInitializedAsync();

        try
        {
            var json = JsonSerializer.Serialize(recipes, _jsonOptions);
            
            // Delete existing entry for this cache key
            await _database!.Table<CachedRecipeSummary>()
                .Where(s => s.CacheKey == cacheKey)
                .DeleteAsync();

            var cached = new CachedRecipeSummary
            {
                CacheKey = cacheKey,
                SummariesJson = json,
                CachedAt = DateTime.UtcNow
            };

            await _database.InsertAsync(cached);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache recipe summaries for key {CacheKey}", cacheKey);
        }
    }

    public async Task<List<RecipeSummaryViewModel>?> GetCachedRecipeSummariesAsync(string cacheKey)
    {
        await EnsureInitializedAsync();

        try
        {
            var cached = await _database!.Table<CachedRecipeSummary>()
                .Where(s => s.CacheKey == cacheKey)
                .FirstOrDefaultAsync();

            if (cached == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<List<RecipeSummaryViewModel>>(cached.SummariesJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cached recipe summaries for key {CacheKey}", cacheKey);
            return null;
        }
    }

    public async Task<bool> IsRecipeCachedAsync(Guid recipeGuid)
    {
        await EnsureInitializedAsync();

        try
        {
            var count = await _database!.Table<CachedRecipe>()
                .Where(r => r.RecipeGuid == recipeGuid)
                .CountAsync();
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<DateTime?> GetCacheTimeAsync(string cacheKey)
    {
        await EnsureInitializedAsync();

        try
        {
            var cached = await _database!.Table<CachedRecipeSummary>()
                .Where(s => s.CacheKey == cacheKey)
                .FirstOrDefaultAsync();
            return cached?.CachedAt;
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearAllCacheAsync()
    {
        await EnsureInitializedAsync();

        try
        {
            await _database!.DeleteAllAsync<CachedRecipe>();
            await _database.DeleteAllAsync<CachedRecipeSummary>();
            _logger.LogInformation("All cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear all cache");
        }
    }

    public async Task ClearExpiredCacheAsync(TimeSpan maxAge)
    {
        await EnsureInitializedAsync();

        try
        {
            var cutoffTime = DateTime.UtcNow - maxAge;

            // Clear expired recipes
            var expiredRecipes = await _database!.Table<CachedRecipe>()
                .Where(r => r.CachedAt < cutoffTime)
                .ToListAsync();
            foreach (var recipe in expiredRecipes)
            {
                await _database.DeleteAsync(recipe);
            }

            // Clear expired summaries
            var expiredSummaries = await _database.Table<CachedRecipeSummary>()
                .Where(s => s.CachedAt < cutoffTime)
                .ToListAsync();
            foreach (var summary in expiredSummaries)
            {
                await _database.DeleteAsync(summary);
            }

            _logger.LogInformation("Expired cache cleared (older than {MaxAge})", maxAge);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear expired cache");
        }
    }

    public async Task<List<RecipeModel>> GetAllCachedRecipesAsync()
    {
        await EnsureInitializedAsync();

        try
        {
            var cachedRecipes = await _database!.Table<CachedRecipe>().ToListAsync();
            var recipes = new List<RecipeModel>();

            foreach (var cached in cachedRecipes)
            {
                try
                {
                    var recipe = JsonSerializer.Deserialize<RecipeModel>(cached.RecipeJson, _jsonOptions);
                    if (recipe != default)
                    {
                        recipes.Add(recipe);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cached recipe {RecipeGuid}", cached.RecipeGuid);
                }
            }

            return recipes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all cached recipes");
            return new List<RecipeModel>();
        }
    }

    public async Task<long> GetCacheSizeAsync()
    {
        await EnsureInitializedAsync();

        try
        {
            var recipeCount = await _database!.Table<CachedRecipe>().CountAsync();
            var summaryCount = await _database.Table<CachedRecipeSummary>().CountAsync();

            // Rough estimate: count * average size
            // For more accurate size, we'd need to query the SQLite database file size
            return (recipeCount * 10000) + (summaryCount * 5000); // Rough estimates
        }
        catch
        {
            return 0;
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (!_initialized)
        {
            await InitializeAsync();
        }
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
            _database?.CloseAsync().Wait();
        }

        _disposed = true;
    }
}
