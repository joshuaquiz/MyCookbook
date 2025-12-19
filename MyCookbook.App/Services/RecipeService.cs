using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.App.Implementations;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Services;

/// <summary>
/// Implementation of recipe-related API operations
/// </summary>
public class RecipeService : IRecipeService
{
    private readonly CookbookHttpClient _httpClient;

    public RecipeService(CookbookHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RecipeModel> GetRecipeAsync(Guid recipeGuid, CancellationToken cancellationToken = default)
    {
        return await _httpClient.Get<RecipeModel>(
            $"/api/Recipe/{recipeGuid}",
            cancellationToken);
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
        var result = await _httpClient.Get<List<RecipeSummaryViewModel>?>(
            $"/api/Home/Popular?take={take}&skip={skip}",
            cancellationToken);
        
        return result ?? [];
    }

    public async Task<List<RecipeModel>> GetPersonalCookbookAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<RecipeModel>>(
            "/api/Personal/Cookbook");

        return result ?? [];
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

