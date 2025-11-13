using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using MyCookbook.API.Exceptions;
using MyCookbook.API.Interfaces;

namespace MyCookbook.API.Implementations;

public sealed class JsonNodeGraphExploder : IJsonNodeGraphExploder
{
    public IReadOnlyList<JsonObject> ExplodeIfGraph(
        JsonObject jsonObject)
    {
        if (jsonObject.TryGetPropertyValue(
                "@graph",
                out var graph)
            && graph != null)
        {
            return graph
                .AsArray()
                .Where(x => x != null)
                .Cast<JsonNode>()
                .Select(x => x.AsObject())
                .ToList();
        }

        if (jsonObject.TryGetPropertyValue(
                "@type",
                out _))
        {
            return new List<JsonObject>(1)
            {
                jsonObject
            };
        }

        throw new JsonObjectGraphProcessingException(
            jsonObject);
    }
}