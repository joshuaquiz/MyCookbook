using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCookbook.Common.Database;

[Table("Authors")]
public class Author
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public string Name { get; init; }

    public string? Image { get; set; }

    [NotMapped]
    public Uri? ImageUri
    {
        get => Image == null ? null : new Uri(Image);
        set => Image = value?.AbsoluteUri;
    }

    public string? BackgroundImage { get; set; }

    [NotMapped]
    public Uri? BackgroundImageUri
    {
        get => BackgroundImage == null ? null : new Uri(BackgroundImage);
        set => BackgroundImage = value?.AbsoluteUri;
    }

    public Guid? UserGuid { get; set; }

    [ForeignKey(nameof(UserGuid))]
    public virtual User? User { get; set; }
}