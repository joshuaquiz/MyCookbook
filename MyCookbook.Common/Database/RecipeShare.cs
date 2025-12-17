using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(ShareId))]
[Table("RecipeShares")]
public class RecipeShare
{
    [Column("share_id")]
    public Guid ShareId { get; set; } = Guid.NewGuid();

    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; }

    [Column("shared_by_author_id")]
    public Guid SharedByAuthorId { get; set; }

    public virtual Author SharedByAuthor { get; set; }

    [Column("share_token")]
    public string ShareToken { get; set; }

    [Column("shared_to_author_id")]
    public Guid? SharedToAuthorId { get; set; }

    public virtual Author? SharedToAuthor { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("access_count")]
    public int AccessCount { get; set; } = 0;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;
}

