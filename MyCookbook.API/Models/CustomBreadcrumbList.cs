using System.Collections.Generic;
using System.Text.Json.Serialization;
using Schema.NET;

namespace MyCookbook.API.Models;

public record CustomBreadcrumbList(
    [property: JsonPropertyName("@context")]
    string? Context,
    [property: JsonPropertyName("@type")]
    string? Type,
    [property: JsonPropertyName("@id")]
    string? Id,
    [property: JsonPropertyName("itemListElement")]
    [property: JsonConverter(typeof(ValuesJsonConverter))]
    Values<IReadOnlyList<CustomItemListElement>, CustomItemListElement> ItemListElement);