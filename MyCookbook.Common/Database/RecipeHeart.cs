using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(AuthorId), nameof(RecipeId))]
[Table("RecipeHearts")]
public class RecipeHeart
{
    [Column("author_id")]
    public Guid AuthorId { get; set; }

    public virtual Author Author { get; set; }

    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; }

    [Column("hearted_at")]
    public DateTime HeartedAt { get; set; } = DateTime.UtcNow;
}