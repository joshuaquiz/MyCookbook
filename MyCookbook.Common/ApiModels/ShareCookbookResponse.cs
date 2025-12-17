using System;

namespace MyCookbook.Common.ApiModels;

public record ShareCookbookResponse(
    string ShareToken,
    string ShareUrl,
    string ShareName,
    int RecipeCount,
    DateTime CreatedAt,
    DateTime? ExpiresAt
);

