using System;
using MyCookbook.Common.Enums;

namespace MyCookbook.Common.ApiModels;

public readonly record struct RecipeIngredientModel(
    Guid Guid,
    IngredientModel? Ingredient,
    string? Quantity,
    Measurement Measurement,
    string? Notes);