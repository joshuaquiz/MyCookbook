using System;
using System.Collections.Generic;

namespace MyCookbook.Common.ApiModels;

public record ShareCookbookRequest(
    string ShareName,
    List<Guid> RecipeIds,
    int? ExpiresInDays = null
);

