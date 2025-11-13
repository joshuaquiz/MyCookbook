using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(RecipeId), nameof(TagId))]
[Table("RecipeTags")]
public class RecipeTag
{
    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; }

    [Column("tag_id")]
    public Guid TagId { get; set; }

    public virtual Tag Tag { get; set; }
}