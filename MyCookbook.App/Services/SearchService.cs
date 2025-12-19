using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.App.Implementations;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Services;

/// <summary>
/// Implementation of search-related API operations
/// </summary>
public class SearchService : ISearchService
{
    private readonly CookbookHttpClient _httpClient;

    public SearchService(CookbookHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<RecipeSummaryViewModel>> GlobalSearchAsync(
        string searchTerm,
        string category,
        string includeIngredients,
        string excludeIngredients,
        int take,
        int skip,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.Get<List<RecipeSummaryViewModel>?>(
            $"/api/Search/Global?term={searchTerm}&category={category}&ingredient={includeIngredients}&exclude={excludeIngredients}&take={take}&skip={skip}",
            cancellationToken);
        
        return result ?? [];
    }
}

