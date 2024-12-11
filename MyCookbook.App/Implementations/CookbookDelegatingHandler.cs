using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using G3.Maui.Core.Models;
using MyCookbook.App.Components.RecipeSummary;
using MyCookbook.App.Implementations.Models;
using MyCookbook.Common;
using SQLite;

namespace MyCookbook.App.Implementations;

[Table("Recipes")]
public class Recipe1
{
    private readonly TimeSpan? _totalTime;

    public List<RecipeIngredient1> RecipeIngredients =>
        PrepSteps
            ?.SelectMany(x => x.Ingredients ?? [])
            .Concat(
                CookingSteps
                    ?.SelectMany(x => x.Ingredients ?? [])
                ?? [])
            .ToList()
        ?? [];

    public bool HasPrep =>
        PrepSteps
            ?.Any() == true;

    public string? Guid { get; init; }

    public string? Image { get; init; }

    public string? TotalTime
    {
        get => _totalTime?.ToString("g");
        init => _totalTime = value == null ? null : TimeSpan.Parse(value);
    }

    public TimeSpan? TotalTimeSpan => _totalTime;

    public string? Name { get; init; }

    public TimeSpan PrepTime { get; init; }

    public TimeSpan CookTime { get; init; }

    public int Servings { get; init; }

    public string? Description { get; init; }

    public UserProfile? UserProfile { get; init; }

    public List<Step1>? PrepSteps { get; init; }

    public List<Step1>? CookingSteps { get; init; }
}

[Table("Steps")]
public class Step1
{
    public Guid Guid { get; init; }

    public int StepNumber { get; init; }

    public Uri? ImageUri { get; init; }

    public string? Description { get; init; }

    public List<RecipeIngredient1>? Ingredients { get; init; }
}

[Table("RecipeIngredients")]
public class RecipeIngredient1
{
    public Uri? ImageUri =>
        Ingredient?.ImageUri;

    public Guid Guid { get; init; }

    public required string IngredientGuid { get; init; }

    public required string RecipeGuid { get; init; }

    public Ingredient1? Ingredient { get; init; }

    public string? Quantity { get; init; }

    public Measurement1 Measurement { get; init; }

    public string? Notes { get; init; }
}

public enum Measurement1
{
    Unit,
    Piece,
    Slice,
    Clove,
    Bunch,
    Cup,
    TableSpoon,
    TeaSpoon,
    Ounce,
    Fillet,
    Inch,
    Can
}

[Table("Ingredients")]
public class Ingredient1
{
    public Guid Guid { get; init; }
    public Uri? ImageUri { get; init; }
    public required string Name { get; init; }
}

public sealed partial class CookbookDelegatingHandler : BaseDelegatingHandler
{
    [GeneratedRegex("^/Account/LogIn$")]
    private static partial Regex AccountLogInRegex();

    [GeneratedRegex("^/api/Home/Popular.*$")]
    private static partial Regex HomePopularRegex();

    [GeneratedRegex("^/api/Account/{[a-zA-Z0-0\\-]+}/Cookbook$")]
    private static partial Regex PersonalCookbookRegex();

    [GeneratedRegex("^/api/Recipe/.*$")]
    private static partial Regex RecipeGetRegex();

    [GeneratedRegex("^/api/Search/Ingredients$")]
    private static partial Regex SearchIngredients();

    [GeneratedRegex("^/api/Search/Global.*$")]
    private static partial Regex GlobalSearch();

    private static List<MockedHttpRequest> GenerateFakeRequests(
        SQLiteAsyncConnection context)
    {
        var serializeUser = JsonSerializer.Serialize(
            CookbookFakeData.GetAppUsersProfile());
        return
        [
            new MockedHttpRequest(
                AccountLogInRegex(),
                [
                    new MockedHttpRequestMethodActions(
                        HttpMethod.Post,
                        (_, _) =>
                            ValueTask.FromResult(
                                CreateStringContent(
                                    serializeUser) as HttpContent))
                ]),
            new MockedHttpRequest(
                HomePopularRegex(),
                [
                    new MockedHttpRequestMethodActions(
                        HttpMethod.Get,
                        async (httpRequestMessage, _) =>
                        {
                            var queryParams = httpRequestMessage
                                .RequestUri
                                !.OriginalString
                                .Replace(
                                    "/api/Home/Popular?",
                                    string.Empty)
                                .Split(
                                    '&',
                                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .Select(
                                    x =>
                                        x.Split(
                                            '=',
                                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                                .ToDictionary(
                                    x =>
                                        x[0],
                                    x =>
                                        int.Parse(
                                            x[1]));
                            var take = queryParams["take"];
                            var skip = queryParams["skip"];
                            var popularRecipeItems = await context.QueryAsync<RecipeSummaryViewModel>(
                                "SELECT r.Guid AS Guid, r.Image AS ImageUrl, r.Name AS Name, a.Image AS AuthorImageUrl, a.Name AS AuthorName, r.TotalTime AS TotalTime, u.Uri AS ItemUrl FROM Recipes AS r INNER JOIN Authors AS a ON a.Guid = r.AuthorGuid INNER JOIN RecipeUrls AS u ON u.Guid = r.RecipeUrlGuid LIMIT ? OFFSET ?",
                                take,
                                skip);
                            return CreateStringContent(
                                JsonSerializer.Serialize(
                                    popularRecipeItems
                                        .DistinctBy(
                                            x =>
                                                x.Guid)));
                        })
                ]),
            new MockedHttpRequest(
                SearchIngredients(),
                [
                    new MockedHttpRequestMethodActions(
                        HttpMethod.Get,
                        async (_, _) =>
                        {
                            var popularRecipeItems = await context.QueryAsync<SearchCategoryItem>(
                                "SELECT \"#\" || SUBSTRING(HEX(ROUND(RANDOM() * 10000000)), 0, 7) AS ColorHex, i.Name AS Name FROM Ingredients AS i");
                            return CreateStringContent(
                                JsonSerializer.Serialize(
                                    popularRecipeItems
                                        .DistinctBy(
                                            x =>
                                                x.Name)));
                        })
                ]),
            new MockedHttpRequest(
                GlobalSearch(),
                [
                    new MockedHttpRequestMethodActions(
                        HttpMethod.Get,
                        async (httpRequestMessage, _) =>
                        {
                            var queryParams = httpRequestMessage
                                .RequestUri
                                !.OriginalString
                                .Replace(
                                    "/api/Search/Global?",
                                    string.Empty)
                                .Split(
                                    '&',
                                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .Select(
                                    x =>
                                        x.Split(
                                            '=',
                                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                                .ToDictionary(
                                    x =>
                                        x[0],
                                    x =>
                                        x.ElementAtOrDefault(1));
                            var take = int.Parse(queryParams["take"] ?? "0");
                            var skip = int.Parse(queryParams["skip"] ?? "0");
                            var term = queryParams["term"]?.Trim() ?? string.Empty;
                            var termQuery = string.IsNullOrWhiteSpace(term)
                                ? "1 = 1"
                                : $"(r.Name LIKE '%{term}%' OR a.Name LIKE '%{term}%' OR i.Name LIKE '%{term}%')";
                            var category = queryParams["category"]?.Trim() ?? string.Empty;
                            var categoryQuery = string.IsNullOrWhiteSpace(category)
                                ? string.Empty
                                : " AND 1 = 1";
                            var ingredient = queryParams["ingredient"]?.Trim() ?? string.Empty;
                            var ingredientQuery = string.IsNullOrEmpty(ingredient)
                                ? string.Empty
                                : $" AND i.Name = '{ingredient}'";
                            var popularRecipeItems = await context.QueryAsync<RecipeSummaryViewModel>(
                                $"SELECT r.Guid AS Guid, r.Image AS ImageUrl, r.Name AS Name, a.Image AS AuthorImageUrl, a.Name AS AuthorName, r.TotalTime AS TotalTime, u.Uri AS ItemUrl FROM Recipes AS r INNER JOIN Authors AS a ON a.Guid = r.AuthorGuid INNER JOIN RecipeUrls AS u ON u.Guid = r.RecipeUrlGuid INNER JOIN RecipeIngredients AS ri ON ri.RecipeGuid = r.Guid INNER JOIN Ingredients AS i ON i.Guid = ri.IngredientGuid WHERE {termQuery}{categoryQuery}{ingredientQuery} LIMIT ? OFFSET ?",
                                take,
                                skip);
                            return CreateStringContent(
                                JsonSerializer.Serialize(
                                    popularRecipeItems
                                        .DistinctBy(
                                            x =>
                                                x.Guid)));
                        })
                ]),
            new MockedHttpRequest(
                PersonalCookbookRegex(),
                [
                    new MockedHttpRequestMethodActions(
                        HttpMethod.Get,
                        async (httpRequestMessage, _) =>
                        {
                            var queryParams = httpRequestMessage
                                .RequestUri
                                !.OriginalString
                                .Split(
                                    '?',
                                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .ElementAt(
                                    1)
                                .Split(
                                    '&',
                                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .Select(
                                    x =>
                                        x.Split(
                                            '=',
                                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                                .ToDictionary(
                                    x =>
                                        x[0],
                                    x =>
                                        int.Parse(
                                            x[1]));
                            var take = queryParams["take"];
                            var skip = queryParams["skip"];
                            var authorGuid = httpRequestMessage
                                .RequestUri
                                !.PathAndQuery
                                .Replace(
                                    "/api/Account/",
                                    string.Empty)
                                .Replace(
                                    "/Cookbook",
                                    string.Empty)
                                .ToUpperInvariant();
                            var popularRecipeItems = await context.QueryAsync<RecipeSummaryViewModel>(
                                $"SELECT r.Guid AS Guid, r.Image AS ImageUrl, r.Name AS Name, a.Image AS AuthorImageUrl, a.Name AS AuthorName, r.TotalTime AS TotalTime, u.Uri AS ItemUrl FROM Recipes AS r INNER JOIN Authors AS a ON a.Guid = r.AuthorGuid INNER JOIN RecipeUrls AS u ON u.Guid = r.RecipeUrlGuid WHERE a.Guid = '{authorGuid}' LIMIT ? OFFSET ?",
                                take,
                                skip);
                            return CreateStringContent(
                                JsonSerializer.Serialize(
                                    popularRecipeItems
                                        .DistinctBy(
                                            x =>
                                                x.Guid)));
                        })
                ]),
            new MockedHttpRequest(
                RecipeGetRegex(),
                [
                    new MockedHttpRequestMethodActions(
                        HttpMethod.Get,
                        async (httpRequestMessage, _) =>
                        {
                            var guid = httpRequestMessage
                                .RequestUri
                                !.PathAndQuery
                                .Replace(
                                    "/api/Recipe/",
                                    string.Empty)
                                .ToUpperInvariant();
                            var recipeItem = await context
                                .Table<Recipe1>()
                                .FirstAsync(
                                    x =>
                                        x.Guid == guid);
                            return CreateStringContent(
                                JsonSerializer.Serialize(
                                    recipeItem));
                        })
                ])
        ];
    }

    /// <summary>
    /// Creates a new instance of <see cref="CookbookDelegatingHandler"/>.
    /// </summary>
    public CookbookDelegatingHandler(
        SQLiteAsyncConnection context)
        : base(
            GenerateFakeRequests(
                context))
    {
    }
}