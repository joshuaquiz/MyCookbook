using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MyCookbook.App.Components.RecipeSummary;

public partial class RecipeSummaryViewModel
{
    public Guid Guid { get; init; }

    private string? _imageUrlsRawJson;
    public string? ImageUrlsRaw
    {
        get => _imageUrlsRawJson;
        init => _imageUrlsRawJson = value;
    }

    public List<Uri> ImageUrls
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_imageUrlsRawJson))
            {
                return [];
            }

            try
            {
                var urls = JsonSerializer.Deserialize<List<string>>(_imageUrlsRawJson);
                return urls?.Select(url => new Uri(url)).ToList() ?? [];
            }
            catch
            {
                return [];
            }
        }
    }

    public string? Name { get; init; }

    public string? AuthorImageUrlRaw { get; init; }
    public Uri? AuthorImageUrl => AuthorImageUrlRaw is null ? null : new Uri(AuthorImageUrlRaw);

    public string? AuthorName { get; init; }

    public long TotalMinutes { get; init; }
    public TimeSpan TotalTime => TimeSpan.FromMinutes(TotalMinutes);

    public long PrepMinutes { get; init; }
    public TimeSpan PrepTime => TimeSpan.FromMinutes(PrepMinutes);

    public string? ItemUrlRaw { get; init; }
    public Uri? ItemUrl => ItemUrlRaw is null ? null : new Uri(ItemUrlRaw);

    public int? Servings { get; init; }

    public string Difficulty { get; init; } = "Medium";

    public string? Category { get; init; }

    public int? Calories { get; init; }

    public int Hearts { get; init; }

    public decimal? Rating { get; init; }

    public string? Tags { get; init; }
}