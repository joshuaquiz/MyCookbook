using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace MyCookbook.API.Interfaces;

public interface IUrlQueuerFromJsonObjectMap
{
    public IReadOnlyCollection<Uri> QueueUrlsFromJsonObjectMap(
        IReadOnlyDictionary<string, JsonObject> jsonObjects);
}