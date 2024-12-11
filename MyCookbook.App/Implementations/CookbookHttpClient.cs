using System.Net.Http;
using G3.Maui.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Networking;

namespace MyCookbook.App.Implementations;

public sealed class CookbookHttpClient(
    IConnectivity connectivity,
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<CookbookHttpClient> logger)
    : BaseHttpClient(
        connectivity,
        httpClient,
        memoryCache,
        logger);