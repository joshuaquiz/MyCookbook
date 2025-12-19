using System;
using SQLite;

namespace MyCookbook.App.Database;

/// <summary>
/// SQLite table for cached recipe summaries
/// </summary>
[Table("CachedRecipeSummaries")]
public class CachedRecipeSummary
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string CacheKey { get; set; } = string.Empty;

    public string SummariesJson { get; set; } = string.Empty;

    public DateTime CachedAt { get; set; }
}

