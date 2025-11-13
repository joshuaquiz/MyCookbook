using System.Text.Json;

namespace MyCookbook.API.Exceptions;

public sealed class LdJsonProcessingException(
    string ldJsonSection,
    JsonException? jsonException)
    : MyCookBookException(
        "Unable to process the ld+json section",
        jsonException)
{
    public string LdJsonSection { get; } = ldJsonSection;
}
public sealed class MultipleLdTypesFoundException(
    string typeName)
    : MyCookBookException(
        $"Multiple {typeName} sections not supported at this time")
{
    public string LdSectionTypeName { get; } = typeName;
}