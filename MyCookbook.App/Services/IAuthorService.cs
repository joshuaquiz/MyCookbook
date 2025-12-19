using System;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.Services;

/// <summary>
/// Service for author-related API operations
/// </summary>
public interface IAuthorService
{
    /// <summary>
    /// Get an author by their GUID
    /// </summary>
    Task<AuthorModel> GetAuthorAsync(Guid authorGuid, CancellationToken cancellationToken = default);
}

