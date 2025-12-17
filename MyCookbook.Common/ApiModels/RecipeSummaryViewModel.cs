using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyCookbook.Common.ApiModels;

public class RecipeSummaryViewModel
{
    public Guid Guid { get; init; }

    [JsonPropertyName("ImageUrlsRaw")]
    public string? ImageUrlsRaw { get; init; }

    [JsonIgnore]
    public List<Uri>? ImageUrls
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ImageUrlsRaw))
            {
                return null;
            }

            try
            {
                var urls = JsonSerializer.Deserialize<List<string>>(ImageUrlsRaw);
                return urls?.Select(url => new Uri(url)).ToList();
            }
            catch
            {
                return null;
            }
        }
    }

    public string? Name { get; init; }

    public string? AuthorImageUrlRaw { get; init; }

    [JsonIgnore]
    public Uri? AuthorImageUrl => AuthorImageUrlRaw is null ? null : new Uri(AuthorImageUrlRaw);

    public string? AuthorName { get; init; }

    public long TotalMinutes { get; init; }

    [JsonIgnore]
    public TimeSpan TotalTime => TimeSpan.FromMinutes(TotalMinutes);

    public long PrepMinutes { get; init; }

    [JsonIgnore]
    public TimeSpan PrepTime => TimeSpan.FromMinutes(PrepMinutes);

    public string? ItemUrlRaw { get; init; }

    [JsonIgnore]
    public Uri? ItemUrl => ItemUrlRaw is null ? null : new Uri(ItemUrlRaw);

    public int? Servings { get; init; }

    public string Difficulty { get; init; } = "Medium";

    public string? Category { get; init; }

    public int? Calories { get; init; }

    public int Hearts { get; init; }

    public decimal? Rating { get; init; }

    public string? Tags { get; init; }
}

