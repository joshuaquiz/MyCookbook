using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;
using MyCookbook.Common.Database;
using Schema.NET;

namespace MyCookbook.API.Implementations;

public sealed class Temp(IDbContextFactory<MyCookbookContext> dbContextFactory, IRecipeWebSiteWrapperProcessor recipeWebSiteWrapperProcessor, ILdJsonSectionJsonObjectExtractor ldJsonSectionJsonObjectExtractor, IJsonNodeGraphExploder jsonNodeGraphExploder) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(stoppingToken);
        var recipeIngredients = (await db.RecipeUrls.Select(x => x.LdJson).Where(x => x != null).ToListAsync(stoppingToken))
            .Select(x => JsonSerializer.Deserialize<IReadOnlyList<string>>(x!)
            ?.SelectMany(ldJsonSectionJsonObjectExtractor.GetJsonObjectsFromLdJsonSection)
            .SelectMany(jsonNodeGraphExploder.ExplodeIfGraph)
            .Select(
                y =>
                    new KeyValuePair<string, JsonObject>(
                        y["@type"]?.ToString() ?? Guid.NewGuid().ToString(),
                        y))
            .Where(y => !string.IsNullOrWhiteSpace(y.Key))
            .ToDictionary(
                y => y.Key,
                y => y.Value))
            .Where(x => x != null)
            .Select(x =>
                new RecipeWebSiteWrapper(new Uri("http://fake.lol"))
                {
                    Organization = x!.TryGetValue(nameof(Organization), out var organization)
                        ? JsonSerializer.Deserialize<Organization>(organization.ToString())
                        : null,
                    WebSite = x.TryGetValue(nameof(WebSite), out var webSite)
                        ? JsonSerializer.Deserialize<WebSite>(webSite.ToString())
                        : null,
                    WebPage = x.TryGetValue(nameof(WebPage), out var webPage)
                        ? JsonSerializer.Deserialize<WebPage>(webPage.ToString())
                        : null,
                    ImageObject = x.TryGetValue(nameof(ImageObject), out var imageObject)
                        ? JsonSerializer.Deserialize<ImageObject>(imageObject.ToString())
                        : null,
                    Person = x.TryGetValue(nameof(Person), out var person)
                        ? JsonSerializer.Deserialize<Person>(person.ToString())
                        : null,
                    Article = x.TryGetValue(nameof(Article), out var article)
                        ? JsonSerializer.Deserialize<Article>(article.ToString())
                        : null,
                    Recipe = x.TryGetValue(nameof(Schema.NET.Recipe), out var recipe)
                        ? JsonSerializer.Deserialize<Schema.NET.Recipe>(recipe.ToString())
                        : null
                })
            .SelectMany(x => x.Recipe?.RecipeIngredient.Select(y => y) ?? [])
            .Distinct()
            .ToList();
        await File.WriteAllLinesAsync(
            "./recipeIngredients.txt",
            recipeIngredients,
            stoppingToken);
    }
}