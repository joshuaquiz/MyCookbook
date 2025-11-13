using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;
using MyCookbook.Common.Database;
using Schema.NET;

namespace MyCookbook.API.Implementations;

public sealed class UrlLdJsonDataNormalizer(
    ILdJsonSectionJsonObjectExtractor ldJsonSectionJsonObjectExtractor,
    IJsonNodeGraphExploder jsonNodeGraphExploder,
    ISiteNormalizerFactory siteNormalizerFactory,
    ILogger<UrlLdJsonDataNormalizer> logger)
    : IUrlLdJsonDataNormalizer
{
    public async ValueTask<SiteWrapper> NormalizeParsedLdJsonData(
        MyCookbookContext db,
        RawDataSource dataSource,
        CancellationToken cancellationToken)
    {
        var jsonStrings = dataSource.LdJsonData?.Replace("\\u0022", "\\\"") ?? string.Empty;
        var jsonObjects = JsonSerializer.Deserialize<IReadOnlyList<string>>(
                                  jsonStrings)
                              ?.SelectMany(ldJsonSectionJsonObjectExtractor.GetJsonObjectsFromLdJsonSection)
                              .SelectMany(jsonNodeGraphExploder.ExplodeIfGraph)
                              .Select(
                                  x =>
                                      new KeyValuePair<string, JsonObject>(
                                          x["@type"]?.ToString() ?? Guid.NewGuid().ToString(),
                                          x))
                              .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                              .GroupBy(
                                  x =>
                                      x.Key)
                              .ToDictionary(
                                  x => x.Key,
                                  x => x.Select(y => y.Value).Distinct().ToList())
                          ?? new Dictionary<string, List<JsonObject>>();
        logger.LogInformation(
            $"{dataSource.Url} - extracted {jsonObjects.Count} ld+json objects");
        logger.LogDebug(
            JsonSerializer.Serialize(
                jsonObjects));
        var doc = new HtmlDocument();
        doc.LoadHtml(
            dataSource.RawHtml!);
        var wrapper = new SiteWrapper(
            dataSource.Url,
            dataSource.RawHtml!,
            doc,
            jsonObjects,
            TryGetType<Organization>(jsonObjects),
            TryGetType<ProfilePage>(jsonObjects),
            TryGetType<WebSite>(jsonObjects),
            TryGetType<WebPage>(jsonObjects),
            TryGetType<ImageObject>(jsonObjects),
            TryGetType<VideoObject>(jsonObjects),
            TryGetType<Person>(jsonObjects),
            TryGetType<Schema.NET.Recipe>(jsonObjects),
            TryGetType<ItemList>(jsonObjects),
            TryGetType<TVSeries>(jsonObjects));
        var handledOrIgnoredNames = new List<string>
        {
            nameof(Organization),
            nameof(ProfilePage),
            nameof(WebSite),
            nameof(WebPage),
            nameof(Article),
            nameof(ImageObject),
            nameof(VideoObject),
            nameof(BreadcrumbList),
            nameof(Person),
            nameof(Schema.NET.Recipe),
            nameof(ItemList),
            nameof(TVSeries),
            nameof(Product)
        };
        var unhandledItems = jsonObjects
            .Where(x => !handledOrIgnoredNames.Contains(x.Key))
            .Select(x => x.Key)
            .ToList();
        if (unhandledItems.Count > 0)
        {
            throw new Exception($"unknown items: {string.Join(", ", unhandledItems)}");
        }

        var siteParser = siteNormalizerFactory.GetSiteNormalizer(
            dataSource.UrlHost);
        return await siteParser.NormalizeSite(
            wrapper);
    }

    private static ReadOnlyCollection<T>? TryGetType<T>(
        Dictionary<string, List<JsonObject>> dictionary) =>
        dictionary.TryGetValue(typeof(T).Name, out var jsonObjects)
            ? new ReadOnlyCollection<T>(
                jsonObjects
                    .Select(x => JsonSerializer.Deserialize<T>(x.ToString()))
                    .Where(x => x != null)
                    .ToList()!)
            : null;
}