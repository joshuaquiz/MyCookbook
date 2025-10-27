using Schema.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;
using MyCookbook.Common.Database;
using Recipe = MyCookbook.Common.Database.Recipe;

namespace MyCookbook.API.Implementations;

public sealed class UrlProcessor(
    ILdJsonExtractor ldJsonExtractor,
    ILdJsonSectionJsonObjectExtractor ldJsonSectionJsonObjectExtractor,
    IJsonNodeGraphExploder jsonNodeGraphExploder,
    IUrlQueuerFromJsonObjectMap urlQueuerFromJsonObjectMap,
    IRecipeWebSiteWrapperProcessor recipeWebSiteWrapperProcessor,
    IJobQueuer jobQueuer,
    ILogger<UrlProcessor> logger)
    : IUrlProcessor
{
    public async ValueTask<Recipe?> ProcessUrl(
        MyCookbookContext db,
        RecipeUrl recipeUrl,
        bool isReprocessing,
        CancellationToken cancellationToken)
    {
            var results = await ldJsonExtractor.ExtractLdJsonItems(
                recipeUrl.Uri,
                recipeUrl.Html,
                isReprocessing,
                cancellationToken);
            recipeUrl.Html = results.RawHtml;
            recipeUrl.StatusCode = results.HttpStatus;
            var jsonSections = JsonSerializer.Serialize(
                results.Data);
            logger.LogInformation(
                $"{recipeUrl.Uri} - extracted {results.Data.Count} ld+json sections {jsonSections}");
            recipeUrl.LdJson = jsonSections;
            return await ProcessLdJsonObjects(
                db,
                recipeUrl,
                results.ImageUrl,
                isReprocessing,
                cancellationToken);
    }

    private async ValueTask<Recipe?> ProcessLdJsonObjects(
        MyCookbookContext db,
        RecipeUrl recipeUrl,
        string? imageUrl,
        bool isReprocessing,
        CancellationToken cancellationToken)
    {
        var jsonObjects = JsonSerializer.Deserialize<IReadOnlyList<string>>(
                recipeUrl.LdJson ?? string.Empty)
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
                x => x.First().Value)
            ?? new Dictionary<string, JsonObject>();
        logger.LogInformation(
            $"{recipeUrl.Uri} - extracted {jsonObjects.Count} ld+json objects");
        logger.LogDebug(
            JsonSerializer.Serialize(
                jsonObjects));
        if (!isReprocessing)
        {
            foreach (var url in urlQueuerFromJsonObjectMap
                         .QueueUrlsFromJsonObjectMap(
                             jsonObjects))
            {
                await jobQueuer.QueueUrlProcessingJob(
                    db,
                    url);
            }
        }

        if (!jsonObjects.ContainsKey(nameof(Schema.NET.Recipe)))
        {
            recipeUrl.ProcessingStatus = RecipeUrlStatus.FinishedSuccess;
            return null;
        }

        var recipeWebSiteWrapper = new RecipeWebSiteWrapper(recipeUrl.Uri)
        {
            Organization = jsonObjects.TryGetValue(nameof(Organization), out var organization)
                ? JsonSerializer.Deserialize<Organization>(organization.ToString())
                : null,
            WebSite = jsonObjects.TryGetValue(nameof(WebSite), out var webSite)
                ? JsonSerializer.Deserialize<WebSite>(webSite.ToString())
                : null,
            WebPage = jsonObjects.TryGetValue(nameof(WebPage), out var webPage)
                ? JsonSerializer.Deserialize<WebPage>(webPage.ToString())
                : null,
            ImageObject = jsonObjects.TryGetValue(nameof(ImageObject), out var imageObject)
                ? JsonSerializer.Deserialize<ImageObject>(imageObject.ToString())
                : null,
            Person = jsonObjects.TryGetValue(nameof(Person), out var person)
                ? JsonSerializer.Deserialize<Person>(person.ToString())
                : null,
            Article = jsonObjects.TryGetValue(nameof(Article), out var article)
                ? JsonSerializer.Deserialize<Article>(article.ToString())
                : null,
            Recipe = jsonObjects.TryGetValue(nameof(Schema.NET.Recipe), out var recipe)
                ? JsonSerializer.Deserialize<Schema.NET.Recipe>(recipe.ToString())
                : null,
            ImageUrl = imageUrl
        };
        var handledNames = new List<string>
        {
            nameof(Organization),
            nameof(WebSite),
            nameof(WebPage),
            nameof(ImageObject),
            nameof(BreadcrumbList),
            nameof(Person),
            nameof(Article),
            nameof(Schema.NET.Recipe),
            nameof(ItemList)
        };
        var unhandledItems = jsonObjects
            .Where(x => !handledNames.Contains(x.Key))
            .ToList();
        if (unhandledItems.Any())
        {
            throw new Exception("unknown items");
        }

        return await recipeWebSiteWrapperProcessor.ProcessWrapper(
            db,
            recipeUrl,
            recipeWebSiteWrapper,
            isReprocessing,
            cancellationToken);
    }
}