using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCookbook.Common.ApiModels;

public readonly record struct RecipeModel(
    Guid Guid,
    Uri Url,
    IReadOnlyList<Uri> ImageUrls,
    string Name,
    TimeSpan? PrepTime,
    TimeSpan? CookTime,
    int Servings,
    string? Description,
    int RecipeHearts,
    decimal? Rating,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Categories,
    UserProfileModel? UserProfile,
    IReadOnlyList<StepModel> PrepSteps,
    IReadOnlyList<StepModel> CookingSteps)
{
    public TimeSpan? TotalTime =>
        !PrepTime.HasValue && !CookTime.HasValue
            ? null
            : (PrepTime ?? TimeSpan.Zero) + (CookTime ?? TimeSpan.Zero);

    public List<RecipeIngredientModel> RecipeIngredients =>
        PrepSteps
            .SelectMany(x => x.Ingredients)
            .Concat(
                CookingSteps.SelectMany(x => x.Ingredients))
            .ToList();

    public bool HasPrep =>
        PrepSteps.Count > 0;
}