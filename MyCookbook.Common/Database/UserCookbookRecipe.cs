using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(AuthorId), nameof(RecipeId))]
[Table("UserCookbookRecipes")]
public class UserCookbookRecipe
{
    [Column("author_id")]
    public Guid AuthorId { get; set; }

    public virtual Author Author { get; set; }

    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; }

    [Column("saved_at")]
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}

