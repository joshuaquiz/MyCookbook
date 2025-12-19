using System;
using SQLite;

namespace MyCookbook.App.Database;

/// <summary>
/// SQLite table for cached recipes
/// </summary>
[Table("CachedRecipes")]
public class CachedRecipe
{
    [PrimaryKey]
    public Guid RecipeGuid { get; set; }

    public string RecipeJson { get; set; } = string.Empty;

    public DateTime CachedAt { get; set; }

    public DateTime LastAccessedAt { get; set; }
}

