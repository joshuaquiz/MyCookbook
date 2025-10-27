using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyCookbook.Common.Enums;

namespace MyCookbook.Common.Database;

[Table("RecipeSteps")]
public class RecipeStep
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public int StepNumber { get; set; }

    public RecipeStepType RecipeStepType { get; set; }

    public string? Instructions { get; set; }

    public virtual List<RecipeStepIngredient> RecipeStepIngredients { get; set; }

    public Guid RecipeGuid { get; set; }

    [ForeignKey(nameof(RecipeGuid))]
    public virtual Recipe Recipe { get; set; }
}