using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using G3.Maui.Core.Models;
using Microsoft.EntityFrameworkCore;
using MyCookbook.App.Components.RecipeSummary;
using MyCookbook.App.Implementations.Models;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Database;
using MyCookbook.Common.Enums;
using SQLite;

namespace MyCookbook.App.Implementations;

public sealed partial class CookbookDelegatingHandler : BaseDelegatingHandler
{
    [GeneratedRegex("^/api/Account/LogIn$")]
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
        SQLiteAsyncConnection connection)
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
                            ValueTask.FromResult<HttpContent>(
                                CreateStringContent(
                                    serializeUser)))
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
                            var popularRecipeItems = await connection.QueryAsync<RecipeSummaryViewModel>(
                                """
                                WITH RecipePrimaryImage AS (
                                    -- Finds a single primary image URL for each recipe
                                    SELECT
                                        ei.entity_id AS recipe_id,
                                        MIN(i.url) AS image_url -- Use MIN as a common SQLite trick to pick one arbitrary URL
                                    FROM
                                        EntityImages AS ei
                                        INNER JOIN Images AS i
                                            ON i.image_id = ei.image_id
                                                AND i.image_type = 1
                                    WHERE
                                        ei.entity_type = 1
                                    GROUP BY
                                        ei.entity_id
                                ),
                                RecipeBackgroundImage AS (
                                    -- Finds a single primary image URL for each recipe
                                    SELECT
                                        ei.entity_id AS recipe_id,
                                        MIN(i.url) AS image_url -- Use MIN as a common SQLite trick to pick one arbitrary URL
                                    FROM
                                        EntityImages AS ei
                                        INNER JOIN Images AS i
                                            ON i.image_id = ei.image_id
                                                AND i.image_type = 2
                                    WHERE
                                        ei.entity_type = 1
                                    GROUP BY
                                        ei.entity_id
                                ),
                                AuthorPrimaryImage AS (
                                    -- Finds a single primary image URL for each author
                                    SELECT
                                        ei.entity_id AS author_id,
                                        MIN(i.url) AS image_url -- Use MIN as a common SQLite trick to pick one arbitrary URL
                                    FROM
                                        EntityImages AS ei
                                        INNER JOIN Images AS i
                                            ON i.image_id = ei.image_id
                                                AND i.image_type = 1
                                    WHERE
                                        ei.entity_type = 3
                                    GROUP BY
                                        ei.entity_id
                                )
                                SELECT
                                    r.recipe_id AS Guid,
                                    COALESCE(pri.image_url, bri.image_url) AS ImageUrlRaw,
                                    r.title AS Name,
                                    ai.image_url AS AuthorImageUrlRaw,
                                    a.name AS AuthorName,
                                    CAST(COALESCE(r.prep_time, 0) + COALESCE(r.cook_time, 0) AS BIGINT) AS TotalMinutes,
                                    u.url AS ItemUrlRaw
                                FROM
                                    Recipes AS r
                                    INNER JOIN Authors AS a
                                        ON a.author_id = r.author_id
                                    INNER JOIN RawDataSources AS u
                                        ON u.source_id = r.recipe_url_id
                                    LEFT JOIN RecipePrimaryImage AS pri
                                        ON pri.recipe_id = r.recipe_id
                                    LEFT JOIN RecipeBackgroundImage AS bri
                                        ON bri.recipe_id = r.recipe_id
                                    LEFT JOIN AuthorPrimaryImage AS ai
                                        ON ai.author_id = r.author_id
                                LIMIT
                                    ?
                                OFFSET
                                    ?;
                                """,
                                take,
                                skip);
                            return CreateStringContent(
                                JsonSerializer.Serialize(
                                    popularRecipeItems
                                        .DistinctBy(
                                            x =>
                                                x.Guid)));
                        })
{
                    }]),
            new MockedHttpRequest(
                SearchIngredients(),
                [
                    new MockedHttpRequestMethodActions(
                        HttpMethod.Get,
                        async (_, _) =>
                        {
                            var popularRecipeItems = await connection.QueryAsync<SearchCategoryItem>(
                                "SELECT \"#\" || SUBSTRING(HEX(ROUND(RANDOM() * 10000000)), 0, 7) AS ColorHex, i.name AS Name FROM Ingredients AS i WHERE i.name NOT LIKE '% %'");
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
                                : $"(i.name LIKE '%{term}%' OR r.title LIKE '%{term}%' OR a.name LIKE '%{term}%')";
                            var category = queryParams["category"]?.Trim() ?? string.Empty;
                            var categoryQuery = string.IsNullOrWhiteSpace(category)
                                ? string.Empty
                                : " AND 1 = 1";
                            var ingredient = queryParams["ingredient"]?.Trim() ?? string.Empty;
                            var ingredientQuery = string.IsNullOrEmpty(ingredient)
                                ? string.Empty
                                : $" AND i.name = '{ingredient}'";
                            var popularRecipeItems = await connection.QueryAsync<RecipeSummaryViewModel>(
                                $"""
                                
                                WITH RecipePrimaryImage AS (
                                    -- Finds a single primary image URL for each recipe
                                    SELECT
                                        ei.entity_id AS recipe_id,
                                        MIN(i.url) AS image_url -- Use MIN as a common SQLite trick to pick one arbitrary URL
                                    FROM
                                        EntityImages AS ei
                                        INNER JOIN Images AS i
                                            ON i.image_id = ei.image_id
                                                AND i.image_type = 1
                                    WHERE
                                        ei.entity_type = 1
                                    GROUP BY
                                        ei.entity_id
                                ),
                                RecipeBackgroundImage AS (
                                    -- Finds a single primary image URL for each recipe
                                    SELECT
                                        ei.entity_id AS recipe_id,
                                        MIN(i.url) AS image_url -- Use MIN as a common SQLite trick to pick one arbitrary URL
                                    FROM
                                        EntityImages AS ei
                                        INNER JOIN Images AS i
                                            ON i.image_id = ei.image_id
                                                AND i.image_type = 2
                                    WHERE
                                        ei.entity_type = 1
                                    GROUP BY
                                        ei.entity_id
                                ),
                                AuthorPrimaryImage AS (
                                    -- Finds a single primary image URL for each author
                                    SELECT
                                        ei.entity_id AS author_id,
                                        MIN(i.url) AS image_url -- Use MIN as a common SQLite trick to pick one arbitrary URL
                                    FROM
                                        EntityImages AS ei
                                        INNER JOIN Images AS i
                                            ON i.image_id = ei.image_id
                                                AND i.image_type = 1
                                    WHERE
                                        ei.entity_type = 3
                                    GROUP BY
                                        ei.entity_id
                                )
                                SELECT DISTINCT
                                    r.recipe_id AS Guid,
                                    COALESCE(pri.image_url, bri.image_url) AS ImageUrlRaw,
                                    r.title AS Name,
                                    ai.image_url AS AuthorImageUrlRaw,
                                    a.name AS AuthorName,
                                    CAST(COALESCE(r.prep_time, 0) + COALESCE(r.cook_time, 0) AS BIGINT) AS TotalMinutes,
                                    u.url AS ItemUrlRaw
                                FROM
                                    Ingredients AS i
                                    INNER JOIN RecipeStepIngredients AS rsi
                                        ON rsi.ingredient_id = i.ingredient_id
                                    INNER JOIN RecipeSteps AS rs
                                        ON rs.step_id = rsi.recipe_step_id
                                    INNER JOIN Recipes AS r
                                        ON r.recipe_id = rs.recipe_id
                                    INNER JOIN Authors AS a
                                        ON a.author_id = r.author_id
                                    INNER JOIN RawDataSources AS u
                                        ON u.source_id = r.recipe_url_id
                                    LEFT JOIN RecipePrimaryImage AS pri
                                        ON pri.recipe_id = r.recipe_id
                                    LEFT JOIN RecipeBackgroundImage AS bri
                                        ON bri.recipe_id = r.recipe_id
                                    LEFT JOIN AuthorPrimaryImage AS ai
                                        ON ai.author_id = r.author_id
                                WHERE
                                    {termQuery}{categoryQuery}{ingredientQuery}
                                LIMIT
                                    ?
                                OFFSET
                                    ?;
                                """,
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
                            var popularRecipeItems = new List<RecipeSummaryViewModel>()/*await connection.QueryAsync<RecipeSummaryViewModel>(
                                $"SELECT r.Guid AS Guid, r.Image AS ImageUrl, r.Name AS Name, a.Image AS AuthorImageUrl, a.Name AS AuthorName, r.TotalTime AS TotalTime, u.Uri AS ItemUrl FROM Recipes AS r INNER JOIN Authors AS a ON a.Guid = r.AuthorGuid INNER JOIN RecipeUrls AS u ON u.Guid = r.RecipeUrlGuid WHERE a.Guid = '{authorGuid}' LIMIT ? OFFSET ?",
                                take,
                                skip)*/;
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
                        async (httpRequestMessage, cancellationToken) =>
                        {
                            var guid = Guid.Parse(
                                httpRequestMessage
                                    .RequestUri
                                    !.PathAndQuery
                                    .Replace(
                                        "/api/Recipe/",
                                        string.Empty));
                            var context = CreateNewContext(
                                connection.DatabasePath);
                            var recipe = await context.Recipes
                                .Include(x => x.RawDataSource)
                                .Include(x => x.EntityImages).ThenInclude(x => x.Image)
                                .Include(x => x.Author).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
                                .Include(x => x.Steps).ThenInclude(x => x.StepIngredients).ThenInclude(x => x.Ingredient).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
                                .FirstAsync(x => x.RecipeId == guid, cancellationToken);
                            var recipeItem = new RecipeModel(
                                recipe.RecipeId,
                                recipe.RawDataSource.Url,
                                recipe.EntityImages.FirstOrDefault(x => x.Image.ImageType == ImageType.Main)?.Image.Url,
                                recipe.Title,
                                recipe.PrepTimeMinutes.HasValue
                                    ? TimeSpan.FromMinutes(recipe.PrepTimeMinutes.Value)
                                    : TimeSpan.Zero, 
                                recipe.CookTimeMinutes.HasValue
                                    ? TimeSpan.FromMinutes(recipe.CookTimeMinutes.Value)
                                    : TimeSpan.Zero, 
                                recipe.Servings ?? 4,
                                recipe.Description,
                                new UserProfileModel(
                                    recipe.Author.AuthorId,
                                    recipe.Author.EntityImages.FirstOrDefault(x => x.Image.ImageType == ImageType.Background)?.Image.Url,
                                    recipe.Author.EntityImages.FirstOrDefault(x => x.Image.ImageType == ImageType.Main)?.Image.Url,
                                    recipe.Author.Name,
                                    recipe.Author.Name,
                                    string.Empty,
                                    string.Empty,
                                    5,
                                    0,
                                    string.Empty,
                                    false,
                                    false,
                                    []),
                                recipe.Steps
                                    .Where(
                                        x =>
                                            x.RecipeStepType == RecipeStepType.PrepStep)
                                    .Select(
                                        x =>
                                            new StepModel(
                                                x.StepId,
                                                x.StepNumber,
                                                null,
                                                x.Instructions,
                                                x.StepIngredients
                                                    .Select(
                                                        y =>
                                                            new RecipeIngredientModel(
                                                                y.StepIngredientId,
                                                                new IngredientModel(
                                                                    y.Ingredient.IngredientId,
                                                                    y.Ingredient.EntityImages.FirstOrDefault(z => z.Image.ImageType == ImageType.Main)?.Image.Url,
                                                                    y.Ingredient.Name),
                                                                y.QuantityType,
                                                                y.MinValue,
                                                                y.MaxValue,
                                                                y.NumberValue,
                                                                y.Unit,
                                                                y.Notes))
                                                    .ToList()))
                                    .ToList(),
                                recipe.Steps
                                    .Where(
                                        x =>
                                            x.RecipeStepType == RecipeStepType.CookingStep)
                                    .Select(
                                        x =>
                                            new StepModel(
                                                x.StepId,
                                                x.StepNumber,
                                                null,
                                                x.Instructions,
                                                x.StepIngredients
                                                    .Select(
                                                        y =>
                                                            new RecipeIngredientModel(
                                                                y.StepIngredientId,
                                                                new IngredientModel(
                                                                    y.Ingredient.IngredientId,
                                                                    y.Ingredient.EntityImages.FirstOrDefault(z => z.Image.ImageType == ImageType.Main)?.Image.Url,
                                                                    y.Ingredient.Name),
                                                                y.QuantityType,
                                                                y.MinValue,
                                                                y.MaxValue,
                                                                y.NumberValue,
                                                                y.Unit,
                                                                y.Notes))
                                                    .ToList()))
                                    .ToList());
                            return CreateStringContent(
                                JsonSerializer.Serialize(
                                    recipeItem));
                        })
                ])
        ];
    }

    private static MyCookbookContext CreateNewContext(
        string path)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyCookbookContext>();
        optionsBuilder.UseSqlite(
            $"Data Source={path}");
        return new MyCookbookContext(
            optionsBuilder.Options);
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