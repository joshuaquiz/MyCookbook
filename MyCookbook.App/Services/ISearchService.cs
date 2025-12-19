using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Services;

/// <summary>
/// Service for search-related API operations
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Global search for recipes
    /// </summary>
    Task<List<RecipeSummaryViewModel>> GlobalSearchAsync(
        string searchTerm,
        string category,
        string includeIngredients,
        string excludeIngredients,
        int take,
        int skip,
        CancellationToken cancellationToken = default);
}

