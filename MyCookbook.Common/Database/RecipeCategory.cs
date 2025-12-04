using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(RecipeId), nameof(CategoryId))]
[Table("RecipeCategories")]
public class RecipeCategory
{
    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; }

    [Column("category_id")]
    public Guid CategoryId { get; set; }

    public virtual Category Category { get; set; }
}

