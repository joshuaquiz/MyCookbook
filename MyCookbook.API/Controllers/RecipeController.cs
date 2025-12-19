using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Database;
using MyCookbook.Common.Enums;

namespace MyCookbook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class RecipeController(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory)
    : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async ValueTask<ActionResult<RecipeModel>> GetRecipe(
        Guid id,
        CancellationToken cancellationToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        var recipe = await db.Recipes
            .Include(x => x.RawDataSource)
            .Include(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.Author).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.Steps).ThenInclude(x => x.StepIngredients).ThenInclude(x => x.Ingredient).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.Steps).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
            .Include(x => x.RecipeTags).ThenInclude(x => x.Tag)
            .Include(x => x.RecipeCategories).ThenInclude(x => x.Category)
            .Include(x => x.RecipeHearts)
            .FirstOrDefaultAsync(x => x.RecipeId == id, cancellationToken);

        if (recipe == null)
        {
            return NotFound();
        }

        var recipeModel = new RecipeModel(
            Guid: recipe.RecipeId,
            Url: recipe.RawDataSource.Url,
            ImageUrls: recipe.EntityImages
                .Where(ei => ei.Image.ImageType == ImageType.Main || ei.Image.ImageType == ImageType.Background)
                .Select(ei => ei.Image.Url)
                .ToList(),
            Name: recipe.Title,
            PrepTime: recipe.PrepTimeMinutes.HasValue ? TimeSpan.FromMinutes(recipe.PrepTimeMinutes.Value) : null,
            CookTime: recipe.CookTimeMinutes.HasValue ? TimeSpan.FromMinutes(recipe.CookTimeMinutes.Value) : null,
            Servings: int.TryParse(recipe.Servings, out var servings) ? servings : 1,
            Description: recipe.Description,
            RecipeHearts: recipe.RecipeHearts.Count,
            Rating: recipe.Rating,
            Tags: recipe.RecipeTags.Select(rt => rt.Tag.TagName).ToList(),
            Categories: recipe.RecipeCategories.Select(rc => rc.Category.CategoryName).ToList(),
            UserProfile: new UserProfileModel(
                Guid: recipe.Author.AuthorId,
                BackgroundImageUri: recipe.Author.EntityImages
                    .Where(ei => ei.Image.ImageType == ImageType.Background)
                    .Select(ei => ei.Image.Url)
                    .FirstOrDefault(),
                ProfileImageUri: recipe.Author.EntityImages
                    .Where(ei => ei.Image.ImageType == ImageType.Main)
                    .Select(ei => ei.Image.Url)
                    .FirstOrDefault(),
                FirstName: recipe.Author.Name,
                LastName: string.Empty,
                Country: string.Empty,
                City: string.Empty,
                Age: 0,
                RecipesAdded: await db.Recipes.CountAsync(r => r.AuthorId == recipe.AuthorId, cancellationToken),
                Description: recipe.Author.Bio,
                IsPremium: false,
                IsFollowed: false,
                RecentRecipes: []
            ),
            PrepSteps: recipe.Steps
                .Where(s => s.RecipeStepType == RecipeStepType.PrepStep)
                .OrderBy(s => s.StepNumber)
                .Select(s => new StepModel(
                    Guid: s.StepId,
                    StepNumber: s.StepNumber,
                    ImageUri: s.EntityImages
                        .Where(ei => ei.Image.ImageType == ImageType.Main)
                        .Select(ei => ei.Image.Url)
                        .FirstOrDefault(),
                    Instructions: s.Instructions,
                    Ingredients: s.StepIngredients.Select(si => new RecipeIngredientModel(
                        Guid: si.StepIngredientId,
                        Ingredient: new IngredientModel(
                            Guid: si.Ingredient.IngredientId,
                            ImageUri: si.Ingredient.EntityImages
                                .Where(ei => ei.Image.ImageType == ImageType.Main)
                                .Select(ei => ei.Image.Url)
                                .FirstOrDefault(),
                            Name: si.Ingredient.Name
                        ),
                        QuantityType: si.QuantityType,
                        MinValue: si.MinValue,
                        MaxValue: si.MaxValue,
                        NumberValue: si.NumberValue,
                        MeasurementUnit: si.Unit,
                        Notes: si.Notes
                    )).ToList()
                ))
                .ToList(),
            CookingSteps: recipe.Steps
                .Where(s => s.RecipeStepType == RecipeStepType.CookingStep)
                .OrderBy(s => s.StepNumber)
                .Select(s => new StepModel(
                    Guid: s.StepId,
                    StepNumber: s.StepNumber,
                    ImageUri: s.EntityImages
                        .Where(ei => ei.Image.ImageType == ImageType.Main)
                        .Select(ei => ei.Image.Url)
                        .FirstOrDefault(),
                    Instructions: s.Instructions,
                    Ingredients: s.StepIngredients.Select(si => new RecipeIngredientModel(
                        Guid: si.StepIngredientId,
                        Ingredient: new IngredientModel(
                            Guid: si.Ingredient.IngredientId,
                            ImageUri: si.Ingredient.EntityImages
                                .Where(ei => ei.Image.ImageType == ImageType.Main)
                                .Select(ei => ei.Image.Url)
                                .FirstOrDefault(),
                            Name: si.Ingredient.Name
                        ),
                        QuantityType: si.QuantityType,
                        MinValue: si.MinValue,
                        MaxValue: si.MaxValue,
                        NumberValue: si.NumberValue,
                        MeasurementUnit: si.Unit,
                        Notes: si.Notes
                    )).ToList()
                ))
                .ToList()
        );

        return Ok(recipeModel);
    }

    [HttpPost("{id:guid}/Share")]
    public async ValueTask<ActionResult<ShareRecipeResponse>> ShareRecipe(
        Guid id,
        [FromBody] ShareRecipeRequest request,
        CancellationToken cancellationToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        // Verify recipe exists
        var recipe = await db.Recipes
            .Include(r => r.RawDataSource)
            .FirstOrDefaultAsync(r => r.RecipeId == id, cancellationToken);
        if (recipe == null)
        {
            return NotFound("Recipe not found");
        }

        // TODO: Get actual user ID from authentication context
        // For now, using the recipe's author
        var sharedByAuthorId = recipe.AuthorId;

        // If sharing to a specific user, verify they exist
        if (request.SharedToAuthorId.HasValue)
        {
            var targetAuthor = await db.Authors.FindAsync([request.SharedToAuthorId.Value], cancellationToken);
            if (targetAuthor == null)
            {
                return NotFound("Target user not found");
            }
        }

        // Generate unique share token
        var shareToken = GenerateShareToken();

        // Create share record
        var recipeShare = new RecipeShare
        {
            RecipeId = id,
            SharedByAuthorId = sharedByAuthorId,
            ShareToken = shareToken,
            SharedToAuthorId = request.SharedToAuthorId
        };

        await db.RecipeShares.AddAsync(recipeShare, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Determine what to return based on share type
        string shareValue;
        if (request.SharedToAuthorId.HasValue)
        {
            // Sharing to a user - return the share token
            shareValue = shareToken;
        }
        else
        {
            // Sharing URL - return the original recipe URL
            shareValue = recipe.RawDataSource?.Url?.ToString() ?? $"{Request.Scheme}://{Request.Host}/api/Recipe/Shared/{shareToken}";
        }

        return Ok(new ShareRecipeResponse(
            ShareToken: shareToken,
            ShareUrl: shareValue,
            CreatedAt: recipeShare.CreatedAt,
            ExpiresAt: null
        ));
    }

    [HttpGet("ShareableAuthors")]
    public async ValueTask<ActionResult<List<ShareableAuthorViewModel>>> GetShareableAuthors(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int take = 8,
        CancellationToken cancellationToken = default)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        // TODO: Get actual user ID from authentication context
        // For now, using the first author (test user)
        var currentAuthorId = await db.Authors
            .Where(a => a.Email == "testuser@mycookbook.com")
            .Select(a => a.AuthorId)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentAuthorId == Guid.Empty)
        {
            return Ok(new List<ShareableAuthorViewModel>());
        }

        // Get share counts for all authors this user has shared with
        var shareCounts = await db.RecipeShares
            .Where(rs => rs.SharedByAuthorId == currentAuthorId && rs.SharedToAuthorId != null)
            .GroupBy(rs => rs.SharedToAuthorId!.Value)
            .Select(g => new { AuthorId = g.Key, ShareCount = g.Count() })
            .ToDictionaryAsync(x => x.AuthorId, x => x.ShareCount, cancellationToken);

        // Build query for authors
        var authorsQuery = db.Authors
            .Include(a => a.EntityImages).ThenInclude(ei => ei.Image)
            .Where(a => a.AuthorId != currentAuthorId) // Exclude current user
            .Where(a => a.AuthorType != AuthorType.ImportedProfile) // Exclude imported profiles
            .Where(a => a.Email != null && a.Email != string.Empty); // Only authors with email

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            authorsQuery = authorsQuery.Where(a =>
                a.Email.ToLower().Contains(lowerSearchTerm) ||
                a.Name.ToLower().Contains(lowerSearchTerm));
        }

        // Get all matching authors
        var authors = await authorsQuery.ToListAsync(cancellationToken);

        // Sort by share count (descending), then by email (ascending)
        var sortedAuthors = authors
            .Select(a => new
            {
                Author = a,
                ShareCount = shareCounts.GetValueOrDefault(a.AuthorId, 0)
            })
            .OrderByDescending(x => x.ShareCount)
            .ThenBy(x => x.Author.Email)
            .Take(take)
            .Select(x => new ShareableAuthorViewModel(
                AuthorId: x.Author.AuthorId,
                Name: x.Author.Name,
                Email: x.Author.Email,
                ImageUrl: x.Author.EntityImages
                    .Where(ei => ei.Image.ImageType == ImageType.Main)
                    .Select(ei => ei.Image.Url)
                    .FirstOrDefault(),
                ShareCount: x.ShareCount
            ))
            .ToList();

        return Ok(sortedAuthors);
    }

    [HttpGet("Shared/{shareToken}")]
    public async ValueTask<ActionResult<RecipeModel>> GetSharedRecipe(
        string shareToken,
        CancellationToken cancellationToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        // Find the share record
        var recipeShare = await db.RecipeShares
            .Include(rs => rs.Recipe)
            .FirstOrDefaultAsync(rs => rs.ShareToken == shareToken, cancellationToken);

        if (recipeShare == null)
        {
            return NotFound("Share link not found");
        }

        // Check if share is still active
        if (!recipeShare.IsActive)
        {
            return BadRequest("This share link has been deactivated");
        }

        // Increment access count
        recipeShare.AccessCount++;
        await db.SaveChangesAsync(cancellationToken);

        // Return the recipe using the existing GetRecipe logic
        return await GetRecipe(recipeShare.RecipeId, cancellationToken);
    }

    [HttpPost("{id:guid}/View")]
    public async ValueTask<ActionResult> TrackRecipeView(
        Guid id,
        CancellationToken cancellationToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        // Verify recipe exists
        var recipeExists = await db.Recipes.AnyAsync(r => r.RecipeId == id, cancellationToken);
        if (!recipeExists)
        {
            return NotFound("Recipe not found");
        }

        // Add new popularity record (add-only, no lookup)
        var popularity = new Popularity
        {
            EntityType = PopularityType.Recipe,
            EntityId = id,
            MetricType = MetricType.Views,
            Count = 1,
            CreatedAt = DateTime.UtcNow
        };

        await db.Popularities.AddAsync(popularity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    [HttpPost("{id:guid}/Heart")]
    public async ValueTask<ActionResult> TrackRecipeHeart(
        Guid id,
        CancellationToken cancellationToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        // Verify recipe exists
        var recipeExists = await db.Recipes.AnyAsync(r => r.RecipeId == id, cancellationToken);
        if (!recipeExists)
        {
            return NotFound("Recipe not found");
        }

        // Add new popularity record (add-only, no lookup)
        var popularity = new Popularity
        {
            EntityType = PopularityType.Recipe,
            EntityId = id,
            MetricType = MetricType.Hearts,
            Count = 1,
            CreatedAt = DateTime.UtcNow
        };

        await db.Popularities.AddAsync(popularity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Ok();
    }

    private static string GenerateShareToken()
    {
        // Generate a URL-safe random token
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}

