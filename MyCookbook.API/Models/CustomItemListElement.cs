using System.Text.Json.Serialization;

namespace MyCookbook.API.Models;

public record CustomItemListElement(
    [property: JsonPropertyName("@type")]
    string? Type,
    [property: JsonPropertyName("item")]
    CustomItem? Item,
    [property: JsonPropertyName("position")]
    int? Position,
    [property: JsonPropertyName("name")]
    string? Name);