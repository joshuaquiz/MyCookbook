using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyCookbook.Common.Enums;

namespace MyCookbook.Common.Database;

[Table("RecipeStepIngredients")]
public class RecipeStepIngredient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public string Quantity { get; set; }

    public Measurement Measurement { get; set; }

    public string? Notes { get; set; }

    public Guid IngredientGuid { get; set; }

    [ForeignKey(nameof(IngredientGuid))]
    public virtual Ingredient Ingredient { get; set; }

    public Guid RecipeStepGuid { get; set; }

    [ForeignKey(nameof(RecipeStepGuid))]
    public virtual RecipeStep RecipeStep { get; set; }
}