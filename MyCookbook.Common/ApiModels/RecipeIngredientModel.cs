using System;
using MyCookbook.Common.Database;
using MyCookbook.Common.Enums;

namespace MyCookbook.Common.ApiModels;

public readonly record struct RecipeIngredientModel(
    Guid Guid,
    IngredientModel Ingredient,
    QuantityType QuantityType,
    decimal? MinValue,
    decimal? MaxValue,
    decimal? NumberValue,
    MeasurementUnit MeasurementUnit,
    string? Notes);