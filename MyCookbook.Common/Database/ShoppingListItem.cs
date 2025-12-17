using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(ShoppingListItemId))]
[Table("ShoppingListItems")]
public class ShoppingListItem
{
    [Column("shopping_list_item_id")]
    public Guid ShoppingListItemId { get; set; } = Guid.NewGuid();

    [Column("author_id")]
    public Guid AuthorId { get; set; }

    public virtual Author Author { get; set; }

    [Column("ingredient_id")]
    public Guid IngredientId { get; set; }

    public virtual Ingredient Ingredient { get; set; }

    [Column("recipe_step_id")]
    public Guid RecipeStepId { get; set; }

    public virtual RecipeStep RecipeStep { get; set; }

    [Column("multiplier")]
    public decimal Multiplier { get; set; } = 1.0m;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("is_purchased")]
    public bool IsPurchased { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

