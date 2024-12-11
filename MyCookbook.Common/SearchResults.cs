using System.Collections.Generic;

namespace MyCookbook.Common;

public sealed record SearchResults(
    List<string> IngredientNames,
    List<string> RecipeNames);