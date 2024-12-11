using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Microsoft.EntityFrameworkCore;
using MyCookbook.Common;

namespace MyCookbook.API;

public class MyCookbookContext : DbContext
{
    public MyCookbookContext(
        DbContextOptions<MyCookbookContext> options)
        : base(
            options)
    {

    }

    public DbSet<RecipeUrl> RecipeUrls { get; set; }

    public DbSet<Ingredient> Ingredients { get; set; }

    public DbSet<RecipeStep> RecipeSteps { get; set; }

    public DbSet<RecipeStepIngredient> RecipeStepIngredients { get; set; }

    public DbSet<Recipe> Recipes { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Author> Authors { get; set; }
}

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

public enum ParserVersion
{
    Unknown = 0,

    V1 = 1,

    V2 = 2,

    V3 = 3
}

public enum RecipeUrlStatus
{
    NotStarted = 0,

    Started = 1,

    FinishedError = 2,

    FinishedSuccess = 3
}

public class Ingredient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public string Name { get; set; }

    public Uri? Image { get; set; }
}

public class RecipeStep
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public int StepNumber { get; set; }

    public RecipeStepType RecipeStepType { get; set; }

    public string? Instructions { get; set; }

    public virtual List<RecipeStepIngredient> RecipeIngredients { get; set; }

    public Guid RecipeGuid { get; set; }

    [ForeignKey(nameof(RecipeGuid))]
    public virtual Recipe Recipe { get; set; }
}

public class RecipeStepIngredient
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public string Quantity { get; set; }

    public Measurement Measurement { get; set; }

    public string? Notes { get; set; }

    public Guid IngredientGuid { get; set; }

    [ForeignKey(nameof(IngredientGuid))]
    public virtual Ingredient Ingredient { get; set; }

    public Guid RecipeStepGuid { get; set; }

    [ForeignKey(nameof(RecipeStepGuid))]
    public virtual RecipeStep RecipeStep { get; set; }
}

public class Recipe
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public Guid RecipeUrlGuid { get; set; }

    [ForeignKey(nameof(RecipeUrlGuid))]
    public virtual RecipeUrl RecipeUrl { get; set; }

    public Uri? Image { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public TimeSpan TotalTime { get; set; }

    public Guid AuthorGuid { get; set; }

    [ForeignKey(nameof(AuthorGuid))]
    public virtual Author Author { get; set; }

    public virtual List<RecipeStep> RecipeSteps { get; set; }
}

public class Author
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public string Name { get; set; }

    public Uri? Image { get; set; }

    public Uri? BackgroundImage { get; set; }

    public Guid? UserGuid { get; set; }

    [ForeignKey(nameof(UserGuid))]
    public virtual User? User { get; set; }
}

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Guid { get; set; }

    public string Name { get; set; }

    public Uri? Image { get; set; }
}