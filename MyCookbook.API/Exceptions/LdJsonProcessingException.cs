using System.Text.Json;

namespace MyCookbook.API.Exceptions;

public sealed class LdJsonProcessingException : MyCookBookException
{
    public LdJsonProcessingException(
        string ldJsonSection,
        JsonException? jsonException)
        : base(
            "Unable to process the ld+json section",
            jsonException)
    {
        LdJsonSection = ldJsonSection;
    }

    public string LdJsonSection { get; }
}