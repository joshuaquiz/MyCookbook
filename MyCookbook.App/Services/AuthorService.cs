using System;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.App.Implementations;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Services;

/// <summary>
/// Implementation of author-related API operations
/// </summary>
public class AuthorService : IAuthorService
{
    private readonly CookbookHttpClient _httpClient;

    public AuthorService(CookbookHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthorModel> GetAuthorAsync(
        Guid authorGuid,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.Get<AuthorModel>(
            $"/api/Author/{authorGuid}",
            cancellationToken);
    }
}

