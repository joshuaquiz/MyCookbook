using System;

namespace MyCookbook.Common;

public sealed record PopularItem(
    Guid Guid,
    Uri? ImageUrl,
    string Name,
    Uri? AuthorImageUrl,
    string AuthorName,
    TimeSpan TotalTime,
    Uri ItemUrl);