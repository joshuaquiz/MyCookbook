using System;

namespace MyCookbook.App.Components.RecipeSummary;

public partial class RecipeSummaryViewModel
{
    public Guid Guid { get; init; }

    public Uri? ImageUrl { get; init; }

    public string? Name { get; init; }

    public Uri? AuthorImageUrl { get; init; }

    public string? AuthorName { get; init; }

    public TimeSpan TotalTime { get; init; }

    public Uri? ItemUrl { get; init; }

    public int? Servings { get; init; }
}