using System;

namespace MyCookbook.API.Models;

public sealed record RecipeWebSiteWrapper(
    Uri Url)
{
    public Schema.NET.Organization? Organization { get; set; }

    public Schema.NET.WebSite? WebSite { get; set; }

    public Schema.NET.WebPage? WebPage { get; set; }

    public Schema.NET.ImageObject? ImageObject { get; set; }

    public Schema.NET.Person? Person { get; set; }

    public Schema.NET.Article? Article { get; set; }

    public Schema.NET.Recipe? Recipe { get; set; }

    public string? ImageUrl { get; set; }
}