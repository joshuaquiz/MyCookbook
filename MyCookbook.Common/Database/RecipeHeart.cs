using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(UserId), nameof(RecipeId))]
[Table("RecipeHearts")]
public class RecipeHeart
{
    [Column("user_id")]
    public Guid UserId { get; set; }

    public virtual User User { get; set; }

    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; }

    [Column("hearted_at")]
    public DateTime HeartedAt { get; set; } = DateTime.UtcNow;
}