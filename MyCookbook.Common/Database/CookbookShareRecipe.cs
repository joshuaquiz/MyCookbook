using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(CookbookShareId), nameof(RecipeId))]
[Table("CookbookShareRecipes")]
public class CookbookShareRecipe
{
    [Column("cookbook_share_id")]
    public Guid CookbookShareId { get; set; }

    public virtual CookbookShare CookbookShare { get; set; }

    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; }
}

