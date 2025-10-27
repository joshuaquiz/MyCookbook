using System;

namespace MyCookbook.Common.ApiModels;

public readonly record struct IngredientModel(
    Guid Guid,
    Uri? ImageUri,
    string Name );