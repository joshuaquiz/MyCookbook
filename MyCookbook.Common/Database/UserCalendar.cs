using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

[PrimaryKey(nameof(Id))]
[Table("UserCalendars")]
public class UserCalendar
{
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("author_id")]
    public Guid AuthorId { get; set; }

    public virtual Author Author { get; set; }

    [Column("recipe_id")]
    public Guid RecipeId { get; set; }

    public virtual Recipe Recipe { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("meal_type")]
    public MealType MealType { get; set; }

    [Column("servings_multiplier")]
    public decimal ServingsMultiplier { get; set; } = 1.0m;
}

