using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using MyCookbook.Common.Enums;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(StepId))]
[Table("RecipeSteps")]
public class RecipeStep
{
    [Column("step_id")]
    public Guid StepId { get; set; } = Guid.NewGuid();

    [Column("step_type")]
    public RecipeStepType RecipeStepType { get; set; }

    [Column("step_number")]
    public int StepNumber { get; set; }

    [Column("instructions")]
    public string Instructions { get; set; }

    // Foreign Keys
    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; }

    // Navigation
    public virtual ICollection<RecipeStepIngredient> StepIngredients { get; set; }

    public virtual ICollection<EntityImage> EntityImages { get; set; }
}