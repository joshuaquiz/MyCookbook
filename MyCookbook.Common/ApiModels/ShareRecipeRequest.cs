using System;

namespace MyCookbook.Common.ApiModels;

public record ShareRecipeRequest(
    Guid? SharedToAuthorId = null  // If null, creates a shareable URL; if set, shares to specific user
);

