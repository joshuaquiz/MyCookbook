using System;
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
//[Authorize]
public sealed class HomeController(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory)
    : ControllerBase
{
    [HttpGet("Popular")]
    public async ValueTask<ActionResult<List<RecipeSummaryViewModel>>> GetPopular(
        [FromQuery] int take = 20,
        [FromQuery] int skip = 0,
        [FromQuery] int hoursWindow = 24,
        CancellationToken cancellationToken = default)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        // Calculate the time window for popularity metrics
        var cutoffTime = DateTime.UtcNow.AddHours(-hoursWindow);

        // Get popularity scores for recipes in the time window
        var popularityScores = await db.Popularities
            .Where(p => p.EntityType == PopularityType.Recipe
                        && p.CreatedAt >= cutoffTime)
            .GroupBy(p => p.EntityId)
            .Select(g => new
            {
                RecipeId = g.Key,
                ViewCount = g.Where(p => p.MetricType == MetricType.Views).Sum(p => p.Count),
                HeartCount = g.Where(p => p.MetricType == MetricType.Hearts).Sum(p => p.Count),
                // Weighted score: hearts are worth 5x views
                PopularityScore = g.Where(p => p.MetricType == MetricType.Views).Sum(p => p.Count) +
                                  (g.Where(p => p.MetricType == MetricType.Hearts).Sum(p => p.Count) * 5)
            })
            .OrderByDescending(x => x.PopularityScore)
            .Take(take + skip)
            .ToListAsync(cancellationToken);

        // Get the recipe IDs to fetch
        var recipeIds = popularityScores
            .Skip(skip)
            .Take(take)
            .Select(x => x.RecipeId)
            .ToList();

        if (!recipeIds.Any())
        {
            // If no popularity data, fall back to recent recipes
            var recentRecipes = await db.Recipes
                .Include(x => x.EntityImages).ThenInclude(x => x.Image)
                .Include(x => x.Author).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
                .Include(x => x.RawDataSource)
                .Include(x => x.RecipeTags).ThenInclude(x => x.Tag)
                .Include(x => x.RecipeCategories).ThenInclude(x => x.Category)
                .Include(x => x.RecipeHearts)
                .OrderByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return Ok(MapToRecipeSummaryViewModels(recentRecipes));
        }

        // Fetch the recipes with all their related data
        var recipes = await db.Recipes
            .Where(r => recipeIds.Contains(r.RecipeId))
            .Include(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.Author).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.RawDataSource)
            .Include(x => x.RecipeTags).ThenInclude(x => x.Tag)
            .Include(x => x.RecipeCategories).ThenInclude(x => x.Category)
            .Include(x => x.RecipeHearts)
            .ToListAsync(cancellationToken);

        // Order by popularity score (maintain the order from the popularity query)
        var orderedRecipes = recipeIds
            .Select(id => recipes.FirstOrDefault(r => r.RecipeId == id))
            .Where(r => r != null)
            .ToList();

        return Ok(MapToRecipeSummaryViewModels(orderedRecipes!));
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