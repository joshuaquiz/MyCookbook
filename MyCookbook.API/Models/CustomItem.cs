using System.Text.Json.Serialization;

namespace MyCookbook.API.Models;

public record CustomItem(
    [property: JsonPropertyName("@type")]
    string? Type,
    [property: JsonPropertyName("@id")]
    string? Id,
    [property: JsonPropertyName("name")]
    string? Name);