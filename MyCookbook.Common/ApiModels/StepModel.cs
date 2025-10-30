using System;
using System.Collections.Generic;

namespace MyCookbook.Common.ApiModels;

public readonly record struct StepModel(
    Guid Guid,
    int StepNumber,
    Uri? ImageUri,
    string? Instructions,
    IReadOnlyList<RecipeIngredientModel> Ingredients);