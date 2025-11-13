using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(LinkId))]
[Table("AuthorLinks")]
public class AuthorLink
{
    [Column("link_id")]
    public Guid LinkId { get; set; } = Guid.NewGuid();

    [Column("url")]
    public Uri Url { get; set; }

    // Foreign Key
    [Column("author_id")]
    public Guid AuthorId { get; set; }

    public virtual Author Author { get; set; } // Navigation property
}