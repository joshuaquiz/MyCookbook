using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
public sealed class AccountController(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory)
    : ControllerBase
{
    [HttpPost("LogIn")]
    public async ValueTask<ActionResult<UserProfileModel>> LogIn(
        [FromBody] LoginRequest loginRequest,
        CancellationToken cancellationToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        // Find user by email
        var author = await db.Authors
            .Include(a => a.EntityImages).ThenInclude(ei => ei.Image)
            .FirstOrDefaultAsync(a => a.Email == loginRequest.Username, cancellationToken);

        if (author == null)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }

        // Verify password hash
        var passwordHash = ComputeSha256Hash(loginRequest.Password);
        if (author.PasswordHash != passwordHash)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }

        // Update last login
        author.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        // Get recipe count
        var recipeCount = await db.Recipes.CountAsync(r => r.AuthorId == author.AuthorId, cancellationToken);

        // Build user profile
        var userProfile = new UserProfileModel(
            Guid: author.AuthorId,
            BackgroundImageUri: author.EntityImages
                .Where(ei => ei.Image.ImageType == ImageType.Background)
                .Select(ei => new Uri(ei.Image.Url))
                .FirstOrDefault(),
            ProfileImageUri: author.EntityImages
                .Where(ei => ei.Image.ImageType == ImageType.Main)
                .Select(ei => new Uri(ei.Image.Url))
                .FirstOrDefault(),
            FirstName: author.Name,
            LastName: string.Empty,
            Country: author.Location ?? string.Empty,
            City: string.Empty,
            Age: 0,
            RecipesAdded: recipeCount,
            Description: author.Bio ?? string.Empty,
            IsPremium: false,
            IsFollowed: false,
            RecentRecipes: []
        );

        return Ok(userProfile);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawData);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    [HttpGet("{userId:guid}/Cookbook")]
    public async ValueTask<ActionResult<List<RecipeSummaryViewModel>>> GetUserCookbook(
        Guid userId,
        [FromQuery] int take = 20,
        [FromQuery] int skip = 0,
        CancellationToken cancellationToken = default)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        // Get recipes from the user's cookbook
        var userRecipeIds = await db.UserCookbookRecipes
            .Where(ucr => ucr.AuthorId == userId)
            .Select(ucr => ucr.RecipeId)
            .ToListAsync(cancellationToken);

        if (!userRecipeIds.Any())
        {
            return Ok(new List<RecipeSummaryViewModel>());
        }

        var recipes = await db.Recipes
            .Where(r => userRecipeIds.Contains(r.RecipeId))
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