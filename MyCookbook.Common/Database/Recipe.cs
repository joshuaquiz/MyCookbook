using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCookbook.Common.Database;

[Table("Recipes")]
public class Recipe
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public Guid RecipeUrlGuid { get; set; }

    [ForeignKey(nameof(RecipeUrlGuid))]
    public virtual RecipeUrl RecipeUrl { get; set; }

    public string? Image { get; set; }

    [NotMapped]
    public Uri? ImageUri
    {
        get => Image == null ? null : new Uri(Image);
        set => Image = value?.AbsoluteUri;
    }

    public string Name { get; init; }

    public string? Description { get; set; }

    public string? TotalTime { get; set; }

    [NotMapped]
    public TimeSpan TotalTimeSpan
    {
        get => TotalTime == null ? TimeSpan.Zero : TimeSpan.Parse(TotalTime);
        set => TotalTime = value.ToString(@"hh\:mm\:ss");
    }

    public Guid AuthorGuid { get; set; }

    [ForeignKey(nameof(AuthorGuid))]
    public virtual Author Author { get; set; }

    public virtual List<RecipeStep> RecipeSteps { get; set; }
}