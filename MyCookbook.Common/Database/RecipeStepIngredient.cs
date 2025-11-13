using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MyCookbook.Common.Enums;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(StepIngredientId))]
[Table("RecipeStepIngredients")]
public class RecipeStepIngredient
{
    [Column("step_ingredient_id")]
    public Guid StepIngredientId { get; set; } = Guid.NewGuid();

    [Column("measurement_type")]
    public MeasurementUnit Unit { get; set; }

    [Column("quantity_type")]
    public QuantityType QuantityType { get; set; }

    [Column("min_value")]
    public decimal? MinValue { get; set; }

    [Column("max_value")]
    public decimal? MaxValue { get; set; }

    [Column("number_value")]
    public decimal? NumberValue { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("raw_text")]
    public string RawText { get; set; }

    // Foreign Keys
    [Column("recipe_step_id")]
    public Guid RecipeStepId { get; set; }

    public virtual RecipeStep RecipeStep { get; set; }

    [Column("ingredient_id")]
    public Guid IngredientId { get; set; }

    public virtual Ingredient Ingredient { get; set; }
}