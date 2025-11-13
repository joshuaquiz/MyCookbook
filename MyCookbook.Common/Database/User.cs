using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(UserId))]
[Table("Users")]
public class User
{
    [Column("user_id")]
    public Guid UserId { get; set; } = Guid.NewGuid();

    [Column("username")]
    public string Username { get; set; }

    [Column("password_hash")]
    public string PasswordHash { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key to Author (Required, Unique)
    [Column("author_id")]
    public Guid AuthorId { get; set; }

    public virtual Author Author { get; set; }

    // Navigation for Relationships
    public virtual ICollection<RecipeHeart> Hearts { get; set; }
}