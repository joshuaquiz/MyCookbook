using System.Text.Json.Nodes;

namespace MyCookbook.API.Exceptions;

public sealed class JsonObjectGraphProcessingException : MyCookBookException
{
    public JsonObjectGraphProcessingException(
        JsonObject jsonObject)
        : base(
            "Unable to process the JsonObject")
    {
        JsonObject = jsonObject;
    }

    public JsonObject JsonObject { get; }
}