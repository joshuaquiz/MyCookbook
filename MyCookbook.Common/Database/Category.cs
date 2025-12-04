using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(CategoryId))]
[Table("Categories")]
public class Category
{
    [Column("category_id")]
    public Guid CategoryId { get; set; } = Guid.NewGuid();

    [Column("category_name")]
    public string CategoryName { get; set; }

    // Navigation
    public virtual ICollection<RecipeCategory> RecipeCategories { get; set; }
}

