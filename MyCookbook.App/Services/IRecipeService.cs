using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Services;

/// <summary>
/// Service for recipe-related API operations
/// </summary>
public interface IRecipeService
{
    /// <summary>
    /// Get a recipe by its GUID
    /// </summary>
    Task<RecipeModel> GetRecipeAsync(Guid recipeGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Track a recipe view (fire-and-forget)
    /// </summary>
    Task TrackRecipeViewAsync(Guid recipeGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Heart a recipe
    /// </summary>
    Task HeartRecipeAsync(Guid recipeGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unheart a recipe
    /// </summary>
    Task UnheartRecipeAsync(Guid recipeGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get shareable authors for a recipe
    /// </summary>
    Task<List<ShareableAuthorViewModel>> GetShareableAuthorsAsync(string searchTerm, int take = 8, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get popular recipes with pagination
    /// </summary>
    Task<List<RecipeSummaryViewModel>> GetPopularRecipesAsync(int take, int skip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's personal cookbook recipes
    /// </summary>
    Task<List<RecipeModel>> GetPersonalCookbookAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Share a recipe with another author
    /// </summary>
    Task<ShareRecipeResponse> ShareRecipeAsync(Guid recipeGuid, Guid? targetAuthorId, CancellationToken cancellationToken = default);
}

