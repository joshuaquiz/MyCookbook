using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using MyCookbook.App.Interfaces;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Implementations;

/// <summary>
/// Simple file-based offline cache service
/// For production, consider using SQLite-net for better performance
/// </summary>
public class OfflineCacheService : IOfflineCacheService
{
    private readonly string _cacheDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public OfflineCacheService()
    {
        _cacheDirectory = Path.Combine(FileSystem.CacheDirectory, "offline_cache");
        Directory.CreateDirectory(_cacheDirectory);
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task CacheRecipeAsync(RecipeModel recipe)
    {
        var filePath = GetRecipeFilePath(recipe.Guid);
        var json = JsonSerializer.Serialize(recipe, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<RecipeModel?> GetCachedRecipeAsync(Guid recipeGuid)
    {
        var filePath = GetRecipeFilePath(recipeGuid);
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<RecipeModel>(json, _jsonOptions);
        }
        catch
        {
            // If deserialization fails, delete the corrupted cache file
            File.Delete(filePath);
            return null;
        }
    }

    public async Task CacheRecipeSummariesAsync(List<RecipeSummaryViewModel> recipes, string cacheKey)
    {
        var filePath = GetSummariesFilePath(cacheKey);
        var cacheData = new CachedSummaries
        {
            CachedAt = DateTime.UtcNow,
            Recipes = recipes
        };
        var json = JsonSerializer.Serialize(cacheData, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<List<RecipeSummaryViewModel>?> GetCachedRecipeSummariesAsync(string cacheKey)
    {
        var filePath = GetSummariesFilePath(cacheKey);
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var cacheData = JsonSerializer.Deserialize<CachedSummaries>(json, _jsonOptions);
            return cacheData?.Recipes;
        }
        catch
        {
            // If deserialization fails, delete the corrupted cache file
            File.Delete(filePath);
            return null;
        }
    }

    public Task<bool> IsRecipeCachedAsync(Guid recipeGuid)
    {
        var filePath = GetRecipeFilePath(recipeGuid);
        return Task.FromResult(File.Exists(filePath));
    }

    public async Task<DateTime?> GetCacheTimeAsync(string cacheKey)
    {
        var filePath = GetSummariesFilePath(cacheKey);
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var cacheData = JsonSerializer.Deserialize<CachedSummaries>(json, _jsonOptions);
            return cacheData?.CachedAt;
        }
        catch
        {
            return null;
        }
    }

    public Task ClearAllCacheAsync()
    {
        if (Directory.Exists(_cacheDirectory))
        {
            Directory.Delete(_cacheDirectory, true);
            Directory.CreateDirectory(_cacheDirectory);
        }
        return Task.CompletedTask;
    }

    public Task ClearExpiredCacheAsync(TimeSpan maxAge)
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
        
        return Task.CompletedTask;
    }

    public async Task<List<RecipeModel>> GetAllCachedRecipesAsync()
    {
        var recipes = new List<RecipeModel>();
        var recipeFiles = Directory.GetFiles(_cacheDirectory, "recipe_*.json");

        foreach (var file in recipeFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var recipe = JsonSerializer.Deserialize<RecipeModel>(json, _jsonOptions);
                if (recipe != null)
                {
                    recipes.Add(recipe);
                }
            }
            catch
            {
                // Skip corrupted files
            }
        }

        return recipes;
    }

    private string GetRecipeFilePath(Guid recipeGuid)
    {
        return Path.Combine(_cacheDirectory, $"recipe_{recipeGuid}.json");
    }

    private string GetSummariesFilePath(string cacheKey)
    {
        // Sanitize cache key for file name
        var sanitized = string.Join("_", cacheKey.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_cacheDirectory, $"summaries_{sanitized}.json");
    }

    private class CachedSummaries
    {
        public DateTime CachedAt { get; set; }
        public List<RecipeSummaryViewModel> Recipes { get; set; } = [];
    }
}

