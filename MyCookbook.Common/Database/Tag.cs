using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(TagId))]
[Table("Tags")]
public class Tag
{
    [Column("tag_id")]
    public Guid TagId { get; set; } = Guid.NewGuid();

    [Column("tag_name")]
    public string TagName { get; set; }

    // Navigation
    public virtual ICollection<RecipeTag> RecipeTags { get; set; }
}