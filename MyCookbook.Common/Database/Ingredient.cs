using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(IngredientId))]
[Table("Ingredients")]
public class Ingredient
{
    [Column("ingredient_id")]
    public Guid IngredientId { get; set; } = Guid.NewGuid();

    [Column("name")]
    public string Name { get; set; }

    // Navigation
    public virtual ICollection<RecipeStepIngredient> StepIngredients { get; set; }

    public virtual ICollection<EntityImage> EntityImages { get; set; }
}