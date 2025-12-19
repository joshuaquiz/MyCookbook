using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
public sealed class CookbookController(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory)
    : ControllerBase
{
    [HttpPost("Share")]
    public async ValueTask<ActionResult<ShareCookbookResponse>> ShareCookbook(
        [FromBody] ShareCookbookRequest request,
        CancellationToken cancellationToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        if (request.RecipeIds == null || request.RecipeIds.Count == 0)
        {
            return BadRequest("At least one recipe must be included in the cookbook share");
        }

        // Verify all recipes exist
        var recipes = await db.Recipes
            .Where(r => request.RecipeIds.Contains(r.RecipeId))
            .ToListAsync(cancellationToken);

        if (recipes.Count != request.RecipeIds.Count)
        {
            return BadRequest("One or more recipes not found");
        }

        // TODO: Get actual user ID from authentication context
        // For now, using the first recipe's author
        var authorId = recipes.First().AuthorId;

        // Generate unique share token
        var shareToken = GenerateShareToken();

        // Calculate expiration
        DateTime? expiresAt = request.ExpiresInDays.HasValue
            ? DateTime.UtcNow.AddDays(request.ExpiresInDays.Value)
            : null;

        // Create cookbook share record
        var cookbookShare = new CookbookShare
        {
            AuthorId = authorId,
            ShareToken = shareToken,
            ShareName = request.ShareName,
            ExpiresAt = expiresAt
        };

        await db.CookbookShares.AddAsync(cookbookShare, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // Add recipes to the share
        foreach (var recipeId in request.RecipeIds)
        {
            await db.CookbookShareRecipes.AddAsync(new CookbookShareRecipe
            {
                CookbookShareId = cookbookShare.ShareId,
                RecipeId = recipeId
            }, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        // Build share URL
        var shareUrl = $"{Request.Scheme}://{Request.Host}/api/Cookbook/Shared/{shareToken}";

        return Ok(new ShareCookbookResponse(
            ShareToken: shareToken,
            ShareUrl: shareUrl,
            ShareName: cookbookShare.ShareName,
            RecipeCount: request.RecipeIds.Count,
            CreatedAt: cookbookShare.CreatedAt,
            ExpiresAt: cookbookShare.ExpiresAt
        ));
    }

    [HttpGet("Shared/{shareToken}")]
    public async ValueTask<ActionResult<SharedCookbookViewModel>> GetSharedCookbook(
        string shareToken,
        CancellationToken cancellationToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);

        // Find the share record
        var cookbookShare = await db.CookbookShares
            .Include(cs => cs.Author)
            .Include(cs => cs.CookbookShareRecipes)
                .ThenInclude(csr => csr.Recipe)
                    .ThenInclude(r => r.EntityImages)
                        .ThenInclude(ei => ei.Image)
            .Include(cs => cs.CookbookShareRecipes)
                .ThenInclude(csr => csr.Recipe)
                    .ThenInclude(r => r.Author)
            .FirstOrDefaultAsync(cs => cs.ShareToken == shareToken, cancellationToken);

        if (cookbookShare == null)
        {
            return NotFound("Share link not found");
        }

        // Check if share is still active
        if (!cookbookShare.IsActive)
        {
            return BadRequest("This share link has been deactivated");
        }

        // Check if share has expired
        if (cookbookShare.ExpiresAt.HasValue && cookbookShare.ExpiresAt.Value < DateTime.UtcNow)
        {
            return BadRequest("This share link has expired");
        }

        // Increment access count
        cookbookShare.AccessCount++;
        await db.SaveChangesAsync(cancellationToken);

        // Build recipe summaries
        var recipeSummaries = cookbookShare.CookbookShareRecipes
            .Select(csr =>
            {
                var imageUrls = csr.Recipe.EntityImages
                    .Where(ei => ei.Image.ImageType == ImageType.Main || ei.Image.ImageType == ImageType.Background)
                    .Select(ei => ei.Image.Url.AbsoluteUri)
                    .ToList();

                var authorImageUri = csr.Recipe.Author.EntityImages
                    .Where(ei => ei.Image.ImageType == ImageType.Main)
                    .Select(ei => ei.Image.Url)
                    .FirstOrDefault();

                var prepMinutes = csr.Recipe.PrepTimeMinutes ?? 0;
                var cookMinutes = csr.Recipe.CookTimeMinutes ?? 0;
                var totalMinutes = prepMinutes + cookMinutes;

                return new RecipeSummaryViewModel
                {
                    Guid = csr.Recipe.RecipeId,
                    ImageUrlsRaw = imageUrls.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(imageUrls) : null,
                    Name = csr.Recipe.Title,
                    AuthorImageUrlRaw = authorImageUri?.AbsoluteUri,
                    AuthorName = csr.Recipe.Author.Name,
                    PrepMinutes = prepMinutes,
                    TotalMinutes = totalMinutes,
                    Rating = csr.Recipe.Rating
                };
            })
            .ToList();

        return Ok(new SharedCookbookViewModel(
            ShareName: cookbookShare.ShareName,
            SharedByAuthorName: cookbookShare.Author.Name,
            CreatedAt: cookbookShare.CreatedAt,
            Recipes: recipeSummaries
        ));
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

