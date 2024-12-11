using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace MyCookbook.API.Interfaces;

public interface IJsonNodeGraphExploder
{
    public IReadOnlyList<JsonObject> ExplodeIfGraph(
        JsonObject jsonObject);
}