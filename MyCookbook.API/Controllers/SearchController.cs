using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public sealed class SearchController(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory)
    : ControllerBase
{
    [HttpGet("Ingredients")]
    public async ValueTask<ActionResult<List<SearchCategoryItem>>> GetIngredients(
        CancellationToken cancellationToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        var ingredients = await db.Ingredients
            .Where(i => !i.Name.Contains(" "))
            .Select(i => new SearchCategoryItem
            {
                Name = i.Name,
                ColorHex = "#" + System.Guid.NewGuid().ToString("N").Substring(0, 6),
                ImageUrl = i.EntityImages
                    .Where(ei => ei.Image.ImageType == ImageType.Main)
                    .Select(ei => ei.Image.Url)
                    .FirstOrDefault()
            })
            .Take(100)
            .ToListAsync(cancellationToken);

        return Ok(ingredients);
    }

    [HttpGet("Global")]
    public async ValueTask<ActionResult<List<RecipeSummaryViewModel>>> GlobalSearch(
        [FromQuery] string? term = null,
        [FromQuery] string? category = null,
        [FromQuery] string? ingredient = null,
        [FromQuery] string? exclude = null,
        [FromQuery] int take = 20,
        [FromQuery] int skip = 0,
        CancellationToken cancellationToken = default)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        var query = db.Recipes
            .Include(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.Author).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.RawDataSource)
            .Include(x => x.RecipeTags).ThenInclude(x => x.Tag)
            .Include(x => x.RecipeCategories).ThenInclude(x => x.Category)
            .Include(x => x.RecipeHearts)
            .Include(x => x.Steps).ThenInclude(x => x.StepIngredients).ThenInclude(x => x.Ingredient)
            .AsQueryable();

        // Apply search term filter
        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(r =>
                r.Title.Contains(term) ||
                r.Author.Name.Contains(term) ||
                r.Steps.Any(s => s.StepIngredients.Any(si => si.Ingredient.Name.Contains(term))));
        }

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(r => r.RecipeCategories.Any(rc => rc.Category.CategoryName.Contains(category)));
        }

        // Apply ingredient filter (must include)
        if (!string.IsNullOrWhiteSpace(ingredient))
        {
            query = query.Where(r => r.Steps.Any(s => s.StepIngredients.Any(si => si.Ingredient.Name == ingredient)));
        }

        // Apply exclude filter
        if (!string.IsNullOrWhiteSpace(exclude))
        {
            query = query.Where(r => !r.Steps.Any(s => s.StepIngredients.Any(si => si.Ingredient.Name.Contains(exclude))));
        }

        var recipes = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        var result = recipes.Select(x => new RecipeSummaryViewModel
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

        return Ok(result);
    }
}