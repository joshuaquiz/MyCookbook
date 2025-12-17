using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(RecipeId))]
[Table("Recipes")]
public class Recipe
{
    [Column("recipe_id")]
    public Guid RecipeId { get; set; } = Guid.NewGuid();

    [Column("title")]
    public string Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("servings")]
    public string? Servings { get; set; }

    [Column("prep_time")]
    public int? PrepTimeMinutes { get; set; }

    [Column("cook_time")]
    public int? CookTimeMinutes { get; set; }

    [Column("type")]
    public string? RecipeType { get; set; }

    [Column("is_public")]
    public bool IsPublic { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("rating")]
    public decimal? Rating { get; set; }

    // Foreign Keys
    [Column("author_id")]
    public Guid AuthorId { get; set; }

    public virtual Author Author { get; set; }

    [Column("recipe_url_id")]
    public Guid RawDataSourceId { get; set; }

    public virtual RawDataSource RawDataSource { get; set; }

    [Column("original_recipe_id")]
    public Guid? OriginalRecipeId { get; set; }

    public virtual Recipe OriginalRecipe { get; set; }

    public virtual ICollection<Recipe> Copies { get; set; }

    // Navigation for Relationships
    public virtual ICollection<RecipeTag> RecipeTags { get; set; }

    public virtual ICollection<RecipeCategory> RecipeCategories { get; set; }

    public virtual ICollection<RecipeStep> Steps { get; set; }

    public virtual ICollection<RecipeHeart> RecipeHearts { get; set; }

    public virtual ICollection<EntityImage> EntityImages { get; set; }
}