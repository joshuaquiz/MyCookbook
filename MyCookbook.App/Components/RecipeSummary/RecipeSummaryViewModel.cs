using System;

namespace MyCookbook.App.Components.RecipeSummary;

public partial class RecipeSummaryViewModel
{
    public Guid Guid { get; init; }

    public string? ImageUrlRaw { get; init; }
    public Uri? ImageUrl => ImageUrlRaw is null ? null : new Uri(ImageUrlRaw);

    public string? Name { get; init; }

    public string? AuthorImageUrlRaw { get; init; }
    public Uri? AuthorImageUrl => AuthorImageUrlRaw is null ? null : new Uri(AuthorImageUrlRaw);

    public string? AuthorName { get; init; }

    public long TotalMinutes { get; init; }
    public TimeSpan TotalTime => TimeSpan.FromMinutes(TotalMinutes);

    public string? ItemUrlRaw { get; init; }
    public Uri? ItemUrl => ItemUrlRaw is null ? null : new Uri(ItemUrlRaw);

    public int? Servings { get; init; }
}