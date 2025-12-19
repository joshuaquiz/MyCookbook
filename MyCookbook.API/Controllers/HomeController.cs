using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // Fetch ALL recipes with their related data
        var allRecipes = await db.Recipes
            .Include(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.Author).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.RawDataSource)
            .Include(x => x.RecipeTags).ThenInclude(x => x.Tag)
            .Include(x => x.RecipeCategories).ThenInclude(x => x.Category)
            .Include(x => x.RecipeHearts)
            .ToListAsync(cancellationToken);

        // Sort recipes: first by popularity score (descending), then by creation date (descending) for recipes with no popularity
        var sortedRecipes = allRecipes
            .OrderByDescending(r => popularityScores.GetValueOrDefault(r.RecipeId, 0))
            .ThenByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToList();

        return Ok(MapToRecipeSummaryViewModels(sortedRecipes));
    }

    private static List<RecipeSummaryViewModel> MapToRecipeSummaryViewModels(List<Recipe> recipes)
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
            ItemUrlRaw = x.RawDataSource.Url.ToString(),
            Tags = string.Join(" ", x.RecipeTags.Select(rt => "#" + rt.Tag.TagName)),
            Category = string.Join(", ", x.RecipeCategories.Select(rc => rc.Category.CategoryName)),
            Hearts = x.RecipeHearts.Count,
            Difficulty = "Medium"
        }).ToList();
    }
}