using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCookbook.Common;

public sealed record Recipe(
    Guid Guid,
    Uri? ImageUri,
    string Name,
    TimeSpan PrepTime,
    TimeSpan CookTime,
    int Servings,
    string Description,
    UserProfile UserProfile,
    List<Step> PrepSteps,
    List<Step> CookingSteps)
{
    public TimeSpan TotalTime =>
        PrepTime + CookTime;

    public List<RecipeStepIngredient> RecipeStepIngredients =>
        PrepSteps
            .SelectMany(x => x.Ingredients)
            .Concat(
                CookingSteps
                    .SelectMany(x => x.Ingredients))
            .ToList();

    public bool HasPrep =>
        PrepSteps
            .Any();
}

public sealed record Step(
    Guid Guid,
    int StepNumber,
    Uri? ImageUri,
    string Description,
    List<RecipeStepIngredient> Ingredients);

public sealed record RecipeStepIngredient(
    Guid Guid,
    Ingredient Ingredient,
    string Quantity,
    Measurement Measurement,
    string? Notes)
{
    public Uri? ImageUri =>
        Ingredient.ImageUri;
}

public enum RecipeStepType
{
    PrepStep,
    CookingStep
}

public enum Measurement
{
    Unit,
    Piece,
    Slice,
    Clove,
    Bunch,
    Cup,
    TableSpoon,
    TeaSpoon,
    Ounce,
    Fillet,
    Inch,
    Can
}

public sealed record Ingredient(
    Guid Guid,
    Uri? ImageUri,
    string Name);