using System;

namespace MyCookbook.Common.ApiModels;

public record ShareRecipeResponse(
    string ShareToken,
    string ShareUrl,
    DateTime CreatedAt,
    DateTime? ExpiresAt
);

