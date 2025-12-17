using System;
using System.Collections.Generic;

namespace MyCookbook.Common.ApiModels;

public record SharedCookbookViewModel(
    string ShareName,
    string SharedByAuthorName,
    DateTime CreatedAt,
    List<RecipeSummaryViewModel> Recipes
);

