using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(ShareId))]
[Table("CookbookShares")]
public class CookbookShare
{
    [Column("share_id")]
    public Guid ShareId { get; set; } = Guid.NewGuid();

    [Column("author_id")]
    public Guid AuthorId { get; set; }

    public virtual Author Author { get; set; }

    [Column("share_token")]
    public string ShareToken { get; set; }

    [Column("share_name")]
    public string ShareName { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [Column("access_count")]
    public int AccessCount { get; set; } = 0;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    public virtual ICollection<CookbookShareRecipe> CookbookShareRecipes { get; set; }
}

