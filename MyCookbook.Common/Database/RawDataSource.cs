using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(SourceId))]
[Table("RawDataSources")]
public class RawDataSource
{
    [Column("source_id")]
    public Guid SourceId { get; set; } = Guid.NewGuid();

    [Column("same_as")]
    public Guid? SameAs { get; set; }

    [Column("url")]
    public Uri Url { get; set; }

    [Column("url_host")]
    public string UrlHost { get; set; }

    [Column("processing_status")]
    public RecipeUrlStatus ProcessingStatus { get; set; }

    [Column("page_type")]
    public PageType PageType { get; set; } = PageType.Unknown;

    [Column("parser_version")]
    public ParserVersion ParserVersion { get; set; }

    [Column("ld_json_data")]
    public string? LdJsonData { get; set; }

    [Column("raw_html")]
    public string? RawHtml { get; set; }

    [Column("processed_datetime")]
    public DateTime? ProcessedDatetime { get; set; }

    [Column("error")]
    public string? Error { get; set; }
}