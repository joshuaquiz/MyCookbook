using System;

namespace MyCookbook.Common.ApiModels;

public readonly record struct UserCalendarEntryModel(
    Guid Id,
    Guid AuthorId,
    Guid RecipeId,
    string RecipeName,
    Uri? RecipeImageUrl,
    DateTime Date,
    int MealType,
    string MealTypeName,
    decimal ServingsMultiplier);

