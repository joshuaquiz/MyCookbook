using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(EntityImageId))]
[Table("EntityImages")]
public class EntityImage
{
    [Column("entity_image_id")]
    public Guid EntityImageId { get; set; } = Guid.NewGuid();

    // Foreign Key to Image
    [Column("image_id")]
    public Guid ImageId { get; set; }

    public virtual Image Image { get; set; }

    // Polymorphic Links
    [Column("entity_id")]
    public Guid EntityId { get; set; }

    [Column("entity_type")]
    public ImageEntityType ImageEntityType { get; set; }
}