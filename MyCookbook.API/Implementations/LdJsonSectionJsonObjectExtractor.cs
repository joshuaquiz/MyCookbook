using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using MyCookbook.API.Exceptions;
using MyCookbook.API.Interfaces;

namespace MyCookbook.API.Implementations;

public sealed class LdJsonSectionJsonObjectExtractor : ILdJsonSectionJsonObjectExtractor
{
    public IReadOnlyList<JsonObject> GetJsonObjectsFromLdJsonSection(
        string json)
    {
        JsonException? exception = null;
        try
        {
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(
                json);
            if (jsonObject != null)
            {
                return new List<JsonObject>(1)
                {
                    jsonObject
                };
            }
        }
        catch (JsonException e)
        {
            exception = e;
        }

        try
        {
            var jsonArray = JsonSerializer.Deserialize<JsonArray>(
                json);
            if (jsonArray != null)
            {
                return jsonArray
                    .Where(x => x != null)
                    .Cast<JsonNode>()
                    .Select(x => x.AsObject())
                    .ToList();
            }
        }
        catch (JsonException e)
        {
            exception = e;
        }

        throw new LdJsonProcessingException(
            json,
            exception);
    }
}