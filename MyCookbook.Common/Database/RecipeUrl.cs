using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace MyCookbook.Common.Database;

public class RecipeUrl
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public ParserVersion ParserVersion { get; set; } = ParserVersion.Unknown;

    public RecipeUrlStatus ProcessingStatus { get; set; }

    public string Host { get; set; }

    public Uri Uri { get; set; }

    public HttpStatusCode? StatusCode { get; set; }

    public string? LdJson { get; set; }

    public string? Html { get; set; }

    public string? Exception { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}