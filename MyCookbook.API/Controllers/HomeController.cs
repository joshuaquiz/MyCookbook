using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Database;
using System.Text.Json;

namespace MyCookbook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class HomeController(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory)
    : ControllerBase
{
    [HttpGet("Popular")]
    [OutputCache(PolicyName = "PopularRecipes")]
    public async ValueTask<ActionResult<List<RecipeSummaryViewModel>>> GetPopular(
        [FromQuery] int take = 20,
        [FromQuery] int skip = 0,
        CancellationToken cancellationToken = default)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        var now = DateTime.UtcNow;
        var oneHourAgo = now.AddHours(-1);
        var oneDayAgo = now.AddDays(-1);
        var oneWeekAgo = now.AddDays(-7);
        var oneMonthAgo = now.AddMonths(-1);

        // Get all popularity metrics for recipes
        var allPopularity = await db.Popularities
            .AsNoTracking()
            .Where(p => p.EntityType == PopularityType.Recipe)
            .ToListAsync(cancellationToken);

        // Calculate weighted popularity scores for each recipe
        // Time-based weighting: last hour (weight 8) > last day (weight 4) > last week (weight 2) > last month (weight 1)
        // Metric weighting: hearts are worth 5x views
        var popularityScores = allPopularity
            .GroupBy(p => p.EntityId)
            .Select(g =>
            {
                var lastHourViews = g.Where(p => p.MetricType == MetricType.Views && p.CreatedAt >= oneHourAgo).Sum(p => p.Count);
                var lastDayViews = g.Where(p => p.MetricType == MetricType.Views && p.CreatedAt >= oneDayAgo && p.CreatedAt < oneHourAgo).Sum(p => p.Count);
                var lastWeekViews = g.Where(p => p.MetricType == MetricType.Views && p.CreatedAt >= oneWeekAgo && p.CreatedAt < oneDayAgo).Sum(p => p.Count);
                var lastMonthViews = g.Where(p => p.MetricType == MetricType.Views && p.CreatedAt >= oneMonthAgo && p.CreatedAt < oneWeekAgo).Sum(p => p.Count);

                var lastHourHearts = g.Where(p => p.MetricType == MetricType.Hearts && p.CreatedAt >= oneHourAgo).Sum(p => p.Count);
                var lastDayHearts = g.Where(p => p.MetricType == MetricType.Hearts && p.CreatedAt >= oneDayAgo && p.CreatedAt < oneHourAgo).Sum(p => p.Count);
                var lastWeekHearts = g.Where(p => p.MetricType == MetricType.Hearts && p.CreatedAt >= oneWeekAgo && p.CreatedAt < oneDayAgo).Sum(p => p.Count);
                var lastMonthHearts = g.Where(p => p.MetricType == MetricType.Hearts && p.CreatedAt >= oneMonthAgo && p.CreatedAt < oneWeekAgo).Sum(p => p.Count);

                var weightedScore =
                    // Last hour (weight 8)
                    (lastHourViews * 8) + (lastHourHearts * 5 * 8) +
                    // Last day (weight 4)
                    (lastDayViews * 4) + (lastDayHearts * 5 * 4) +
                    // Last week (weight 2)
                    (lastWeekViews * 2) + (lastWeekHearts * 5 * 2) +
                    // Last month (weight 1)
                    (lastMonthViews * 1) + (lastMonthHearts * 5 * 1);

                return new
                {
                    RecipeId = g.Key,
                    PopularityScore = weightedScore
                };
            })
            .Where(x => x.PopularityScore > 0)
            .OrderByDescending(x => x.PopularityScore)
            .ToDictionary(x => x.RecipeId, x => x.PopularityScore);

        // Step 1: Get recipe IDs with their hosts and popularity scores, ordered by popularity
        // This query is lightweight - only fetches IDs and host names
        var recipesWithHosts = await db.Recipes
            .AsNoTracking()
            .Select(r => new
            {
                r.RecipeId,
                r.CreatedAt,
                UrlHost = r.RawDataSource != null ? r.RawDataSource.UrlHost : null,
                PopularityScore = popularityScores.ContainsKey(r.RecipeId) ? popularityScores[r.RecipeId] : 0
            })
            .ToListAsync(cancellationToken);

        // Step 2: Distribute recipes by host to avoid bandwidth concentration
        // Take top recipes while ensuring diversity across hosts
        var selectedRecipeIds = new List<Guid>();
        var hostCounts = new Dictionary<string, int>();
        const int maxPerHost = 3; // Maximum recipes per host in a single page

        foreach (var recipe in recipesWithHosts
            .OrderByDescending(r => r.PopularityScore)
            .ThenByDescending(r => r.CreatedAt))
        {
            if (selectedRecipeIds.Count >= skip + take)
                break;

            var host = recipe.UrlHost ?? "user-created";
            var currentHostCount = hostCounts.GetValueOrDefault(host, 0);

            // Allow recipe if we haven't hit the per-host limit, or if we need to fill the page
            if (currentHostCount < maxPerHost || selectedRecipeIds.Count < skip + take)
            {
                selectedRecipeIds.Add(recipe.RecipeId);
                hostCounts[host] = currentHostCount + 1;
            }
        }

        // Step 3: Apply skip/take and fetch only the selected recipes with full data
        var recipeIdsToFetch = selectedRecipeIds.Skip(skip).Take(take).ToList();

        if (!recipeIdsToFetch.Any())
        {
            return Ok(new List<RecipeSummaryViewModel>());
        }

        // Step 4: Fetch full recipe data ONLY for the selected recipes
        // Fix #1: Remove .Include(x => x.RawDataSource) to avoid loading massive HTML
        // Fix #4: Add .AsNoTracking() for read-only query
        var recipes = await db.Recipes
            .AsNoTracking()
            .Where(r => recipeIdsToFetch.Contains(r.RecipeId))
            .Include(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.Author).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
            // ✅ Fix #1: Don't include RawDataSource - we'll get URL separately
            .Include(x => x.RecipeTags).ThenInclude(x => x.Tag)
            .Include(x => x.RecipeCategories).ThenInclude(x => x.Category)
            .Include(x => x.RecipeHearts)
            .ToListAsync(cancellationToken);

        // Step 5: Get ONLY the URLs for these recipes (not the entire RawDataSource with HTML)
        var recipeUrls = await db.Recipes
            .AsNoTracking()
            .Where(r => recipeIdsToFetch.Contains(r.RecipeId))
            .Select(r => new
            {
                r.RecipeId,
                Url = r.RawDataSource != null ? r.RawDataSource.Url : null
            })
            .ToDictionaryAsync(x => x.RecipeId, x => x.Url, cancellationToken);

        // Step 6: Maintain the original order (by popularity, then creation date)
        var orderedRecipes = recipeIdsToFetch
            .Select(id => recipes.First(r => r.RecipeId == id))
            .ToList();

        return Ok(MapToRecipeSummaryViewModels(orderedRecipes, recipeUrls));
    }

    private static List<RecipeSummaryViewModel> MapToRecipeSummaryViewModels(
        List<Recipe> recipes,
        Dictionary<Guid, Uri?> recipeUrls)
    {
        return recipes.Select(x => new RecipeSummaryViewModel
        {
            Guid = x.RecipeId,
            Name = x.Title,
            TotalMinutes = (x.PrepTimeMinutes ?? 0) + (x.CookTimeMinutes ?? 0),
            PrepMinutes = x.PrepTimeMinutes ?? 0,
            Servings = int.TryParse(x.Servings, out var servings) ? servings : null,
            Rating = x.Rating,
            ImageUrlsRaw = x.EntityImages
                .Where(ei => ei.Image.ImageType == ImageType.Main || ei.Image.ImageType == ImageType.Background)
                .Select(ei => ei.Image.Url.ToString())
                .ToList() is var urls && urls.Any() ? JsonSerializer.Serialize(urls) : null,
            AuthorName = x.Author.Name,
            AuthorImageUrlRaw = x.Author.EntityImages
                .Where(ei => ei.Image.ImageType == ImageType.Main)
                .Select(ei => ei.Image.Url.ToString())
                .FirstOrDefault(),
            ItemUrlRaw = recipeUrls.GetValueOrDefault(x.RecipeId)?.ToString(),
            Tags = string.Join(" ", x.RecipeTags.Select(rt => "#" + rt.Tag.TagName)),
            Category = string.Join(", ", x.RecipeCategories.Select(rc => rc.Category.CategoryName)),
            Hearts = x.RecipeHearts.Count,
            Difficulty = "Medium"
        }).ToList();
    }
}