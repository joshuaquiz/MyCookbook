using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using HtmlAgilityPack;
using Schema.NET;

namespace MyCookbook.API.Models;

public readonly record struct SiteWrapper(
    Uri Url,
    string RawHtml,
    HtmlDocument HtmlDocument,
    Dictionary<string, List<JsonObject>> LdJsonObjects,
    IReadOnlyList<Organization>? Organizations,
    IReadOnlyList<ProfilePage>? ProfilePages,
    IReadOnlyList<WebSite>? WebSites,
    IReadOnlyList<WebPage>? WebPages,
    IReadOnlyList<ImageObject>? ImageObjects,
    IReadOnlyList<VideoObject>? VideoObjects,
    IReadOnlyList<Person>? Persons,
    IReadOnlyList<Recipe>? Recipes,
    IReadOnlyList<ItemList>? ItemLists,
    IReadOnlyList<TVSeries>? TvSeries);