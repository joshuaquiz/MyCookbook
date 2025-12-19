using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class AccountController(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory)
    : ControllerBase
{
    [HttpPost("LogIn")]
    [AllowAnonymous]
    public async ValueTask<ActionResult<LoginResponse>> LogIn(
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
        var backgroundImageUri = author.EntityImages
            .Where(ei => ei.Image.ImageType == ImageType.Background)
            .Select(ei => ei.Image.Url)
            .FirstOrDefault();

        var profileImageUri = author.EntityImages
            .Where(ei => ei.Image.ImageType == ImageType.Main)
            .Select(ei => ei.Image.Url)
            .FirstOrDefault();

        var userProfile = new UserProfileModel(
            Guid: author.AuthorId,
            BackgroundImageUri: backgroundImageUri,
            ProfileImageUri: profileImageUri,
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

        // Generate JWT access token and refresh token
        var accessToken = GenerateJwtToken(author, isRefreshToken: false);
        var refreshToken = GenerateJwtToken(author, isRefreshToken: true);

        var loginResponse = new LoginResponse(
            UserProfile: userProfile,
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            TokenType: "Bearer",
            ExpiresIn: 3600 // 1 hour
        );

        return Ok(loginResponse);
    }

    [HttpPost("RefreshToken")]
    [AllowAnonymous]
    public async ValueTask<ActionResult<RefreshTokenResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate the refresh token
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "MyCookbook-Development-Secret-Key-Change-In-Production-12345678901234567890";
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "MyCookbook.API";
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "MyCookbook.App";

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = securityKey,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(request.RefreshToken, validationParameters, out var validatedToken);

            // Check if this is a refresh token
            var tokenTypeClaim = principal.FindFirst("token_type");
            if (tokenTypeClaim?.Value != "refresh")
            {
                return Unauthorized(new { error = "Invalid refresh token" });
            }

            // Get the author ID from the token
            var authorIdClaim = principal.FindFirst("author_id");
            if (authorIdClaim == null || !Guid.TryParse(authorIdClaim.Value, out var authorId))
            {
                return Unauthorized(new { error = "Invalid token claims" });
            }

            // Get the author from the database
            await using var db = await myCookbookContextFactory.CreateDbContextAsync(cancellationToken);
            var author = await db.Authors.FindAsync(new object[] { authorId }, cancellationToken);

            if (author == null)
            {
                return Unauthorized(new { error = "User not found" });
            }

            // Generate new tokens
            var newAccessToken = GenerateJwtToken(author, isRefreshToken: false);
            var newRefreshToken = GenerateJwtToken(author, isRefreshToken: true);

            var response = new RefreshTokenResponse(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken,
                TokenType: "Bearer",
                ExpiresIn: 3600 // 1 hour
            );

            return Ok(response);
        }
        catch (SecurityTokenException)
        {
            return Unauthorized(new { error = "Invalid or expired refresh token" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while refreshing the token", details = ex.Message });
        }
    }

    private static string GenerateJwtToken(Author author, bool isRefreshToken = false)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "MyCookbook-Development-Secret-Key-Change-In-Production-12345678901234567890";
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "MyCookbook.API";
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "MyCookbook.App";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, author.AuthorId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, author.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Name, author.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("author_id", author.AuthorId.ToString())
        };

        // Add token type claim to distinguish refresh tokens from access tokens
        if (isRefreshToken)
        {
            claims.Add(new Claim("token_type", "refresh"));
        }

        // Refresh tokens expire in 30 days, access tokens in 1 hour
        var expiration = isRefreshToken ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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