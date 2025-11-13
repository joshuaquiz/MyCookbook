using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(ImageId))]
[Table("Images")]
public class Image
{
    [Column("image_id")]
    public Guid ImageId { get; set; } = Guid.NewGuid();

    [Column("url")]
    public Uri Url { get; set; }

    [Column("image_type")]
    public ImageType ImageType { get; set; }

    // Navigation
    public virtual ICollection<EntityImage> EntityImages { get; set; }
}