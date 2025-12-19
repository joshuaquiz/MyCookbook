using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyCookbook.Common.ApiModels;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Interfaces;

/// <summary>
/// SQLite-based offline cache service interface
/// </summary>
public interface ISqliteCacheService : IOfflineCacheService
{
    /// <summary>
    /// Initialize the SQLite database
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Get current cache size in bytes
    /// </summary>
    Task<long> GetCacheSizeAsync();
}

