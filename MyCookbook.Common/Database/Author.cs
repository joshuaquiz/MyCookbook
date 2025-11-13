using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(AuthorId))]
[Table("Authors")]
public class Author
{
    [Column("author_id")]
    public Guid AuthorId { get; set; } = Guid.NewGuid();

    [Column("name")]
    public string Name { get; set; }

    [Column("bio")]
    public string Bio { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("is_visible")]
    public bool IsVisible { get; set; }

    [Column("author_type")]
    public AuthorType AuthorType { get; set; }

    // Navigation for Relationships
    public virtual User User { get; set; } // One-to-one (optional)

    public virtual ICollection<AuthorLink> Links { get; set; }

    public virtual ICollection<Recipe> Recipes { get; set; }

    public virtual ICollection<EntityImage> EntityImages { get; set; } // Link to images
}