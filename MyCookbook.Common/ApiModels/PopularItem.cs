using System;

namespace MyCookbook.Common.ApiModels;

public readonly record struct PopularItem(
    Guid Guid,
    Uri? ImageUrl,
    string Name,
    Uri? AuthorImageUrl,
    string AuthorName,
    TimeSpan TotalTime,
    Uri ItemUrl);