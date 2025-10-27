using Microsoft.EntityFrameworkCore;

namespace MyCookbook.Common.Database;

public class MyCookbookContext(
    DbContextOptions<MyCookbookContext> options)
    : DbContext(
        options)
{
    public DbSet<RecipeUrl> RecipeUrls { get; set; }

    public DbSet<Ingredient> Ingredients { get; set; }

    public DbSet<RecipeStep> RecipeSteps { get; set; }

    public DbSet<RecipeStepIngredient> RecipeStepIngredients { get; set; }

    public DbSet<Recipe> Recipes { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Author> Authors { get; set; }
}