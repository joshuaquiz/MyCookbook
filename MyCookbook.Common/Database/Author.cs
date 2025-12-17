using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(AuthorId))]
[Table("Authors")]
public class Author
{
    [Column("author_id")]
    public Guid AuthorId { get; set; } = Guid.NewGuid();

    [Column("name")]
    public string Name { get; set; }

    [Column("bio")]
    public string Bio { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("is_visible")]
    public bool IsVisible { get; set; }

    [Column("author_type")]
    public AuthorType AuthorType { get; set; }

    // User authentication fields (merged from User table)
    [Column("email")]
    public string? Email { get; set; }

    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("email_verified")]
    public bool EmailVerified { get; set; }

    [Column("auth_provider")]
    public string? AuthProvider { get; set; }

    [Column("provider_user_id")]
    public string? ProviderUserId { get; set; }

    [Column("cognito_sub")]
    public string? CognitoSub { get; set; }

    [Column("last_login_at")]
    public DateTime? LastLoginAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation for Relationships
    public virtual ICollection<AuthorLink> Links { get; set; }

    public virtual ICollection<Recipe> Recipes { get; set; }

    public virtual ICollection<EntityImage> EntityImages { get; set; } // Link to images

    public virtual ICollection<RecipeHeart> Hearts { get; set; }

    public virtual ICollection<UserCalendar> UserCalendars { get; set; }

    public virtual ICollection<ShoppingListItem> ShoppingListItems { get; set; }

    public virtual ICollection<UserCookbookRecipe> UserCookbookRecipes { get; set; }
}