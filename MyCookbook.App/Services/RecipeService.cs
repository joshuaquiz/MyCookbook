using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.App.Implementations;
using MyCookbook.App.Interfaces;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Services;

/// <summary>
/// Implementation of recipe-related API operations with offline caching support
/// </summary>
public class RecipeService : IRecipeService
{
    private readonly CookbookHttpClient _httpClient;
    private readonly IOfflineCacheService _cacheService;

    public RecipeService(
        CookbookHttpClient httpClient,
        IOfflineCacheService cacheService)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
    }

    public async Task<RecipeModel> GetRecipeAsync(Guid recipeGuid, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to fetch from API
            var recipe = await _httpClient.Get<RecipeModel>(
                $"/api/Recipe/{recipeGuid}",
                cancellationToken);

            // Cache the recipe for offline access
            await _cacheService.CacheRecipeAsync(recipe);

            return recipe;
        }
        catch
        {
            // If API call fails, try to get from cache
            var cachedRecipe = await _cacheService.GetCachedRecipeAsync(recipeGuid);
            if (cachedRecipe.HasValue)
            {
                return cachedRecipe.Value;
            }

            // If no cache available, re-throw the exception
            throw;
        }
    }

    public async Task TrackRecipeViewAsync(Guid recipeGuid, CancellationToken cancellationToken = default)
    {
        await _httpClient.Post<object, object>(
            $"/api/Recipe/{recipeGuid}/View",
            new { },
            cancellationToken);
    }

    public async Task HeartRecipeAsync(Guid recipeGuid, CancellationToken cancellationToken = default)
    {
        await _httpClient.Post<object, object>(
            $"/api/Recipe/{recipeGuid}/Heart",
            new { },
            cancellationToken);
    }

    public async Task UnheartRecipeAsync(Guid recipeGuid, CancellationToken cancellationToken = default)
    {
        await _httpClient.Post<object, object>(
            $"/api/Recipe/{recipeGuid}/Unheart",
            new { },
            cancellationToken);
    }

    public async Task<List<ShareableAuthorViewModel>> GetShareableAuthorsAsync(
        string searchTerm,
        int take = 8,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.Get<List<ShareableAuthorViewModel>>(
            $"/api/Recipe/ShareableAuthors?searchTerm={Uri.EscapeDataString(searchTerm)}&take={take}",
            cancellationToken);
    }

    public async Task<List<RecipeSummaryViewModel>> GetPopularRecipesAsync(
        int take,
        int skip,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"popular_{take}_{skip}";

        try
        {
            var result = await _httpClient.Get<List<RecipeSummaryViewModel>?>(
                $"/api/Home/Popular?take={take}&skip={skip}",
                cancellationToken);

            var recipes = result ?? [];

            // Cache the results
            if (recipes.Count > 0)
            {
                await _cacheService.CacheRecipeSummariesAsync(recipes, cacheKey);
            }

            return recipes;
        }
        catch
        {
            // If API call fails, try to get from cache
            var cachedRecipes = await _cacheService.GetCachedRecipeSummariesAsync(cacheKey);
            if (cachedRecipes != null)
            {
                return cachedRecipes;
            }

            // If no cache available, re-throw the exception
            throw;
        }
    }

    public async Task<List<RecipeModel>> GetPersonalCookbookAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "personal_cookbook";

        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<RecipeModel>>(
                "/api/Personal/Cookbook");

            var recipes = result ?? [];

            // Cache each recipe individually for offline access
            foreach (var recipe in recipes)
            {
                await _cacheService.CacheRecipeAsync(recipe);
            }

            return recipes;
        }
        catch
        {
            // If API call fails, return all cached recipes
            var cachedRecipes = await _cacheService.GetAllCachedRecipesAsync();
            if (cachedRecipes.Count > 0)
            {
                return cachedRecipes;
            }

            // If no cache available, re-throw the exception
            throw;
        }
    }

    public async Task<ShareRecipeResponse> ShareRecipeAsync(
        Guid recipeGuid,
        Guid? targetAuthorId,
        CancellationToken cancellationToken = default)
    {
        var request = new ShareRecipeRequest(SharedToAuthorId: targetAuthorId);
        return await _httpClient.Post<ShareRecipeResponse, ShareRecipeRequest>(
            $"/api/Recipe/{recipeGuid}/Share",
            request,
            cancellationToken);
    }
}

