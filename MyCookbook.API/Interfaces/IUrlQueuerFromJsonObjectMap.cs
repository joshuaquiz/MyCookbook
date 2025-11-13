using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace MyCookbook.API.Interfaces;

public interface IUrlQueuerFromJsonObjectMap
{
    public IEnumerable<Uri> QueueUrlsFromJsonObjectMap(
        IReadOnlyDictionary<string, List<JsonObject>> jsonObjects);
}