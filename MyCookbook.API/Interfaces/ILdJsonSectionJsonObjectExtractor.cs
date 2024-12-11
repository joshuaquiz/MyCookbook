using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace MyCookbook.API.Interfaces;

public interface ILdJsonSectionJsonObjectExtractor
{
    public IReadOnlyList<JsonObject> GetJsonObjectsFromLdJsonSection(
        string json);
}