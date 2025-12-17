using System;

namespace MyCookbook.Common.ApiModels;

public record ShareableAuthorViewModel(
    Guid AuthorId,
    string Name,
    string Email,
    Uri? ImageUrl,
    int ShareCount  // Number of times current user has shared with this author
);

