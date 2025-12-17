using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(PopularityId))]
[Table("Popularity")]
public class Popularity
{
    [Column("popularity_id")]
    public Guid PopularityId { get; set; } = Guid.NewGuid();

    [Column("entity_type")]
    public PopularityType EntityType { get; set; }

    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Column("metric_type")]
    public MetricType MetricType { get; set; }

    [Column("count")]
    public int Count { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}