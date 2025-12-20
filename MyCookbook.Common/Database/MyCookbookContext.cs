using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MyCookbook.Common.Database;

public class MyCookbookContext(
    DbContextOptions<MyCookbookContext> options)
    : DbContext(
        options)
{
    public DbSet<Author> Authors { get; set; }

    public DbSet<Recipe> Recipes { get; set; }

    public DbSet<Ingredient> Ingredients { get; set; }

    public DbSet<RecipeStep> RecipeSteps { get; set; }

    public DbSet<RecipeStepIngredient> StepIngredients { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<RecipeTag> RecipeTags { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<RecipeCategory> RecipeCategories { get; set; }

    public DbSet<RecipeHeart> RecipeHearts { get; set; }

    public DbSet<Image> Images { get; set; }

    public DbSet<EntityImage> EntityImages { get; set; }

    public DbSet<RawDataSource> RawDataSources { get; set; }

    public DbSet<Popularity> Popularities { get; set; }

    public DbSet<AuthorLink> AuthorLinks { get; set; }

    public DbSet<UserCalendar> UserCalendars { get; set; }

    public DbSet<ShoppingListItem> ShoppingListItems { get; set; }

    public DbSet<UserCookbookRecipe> UserCookbookRecipes { get; set; }

    public DbSet<RecipeShare> RecipeShares { get; set; }

    public DbSet<CookbookShare> CookbookShares { get; set; }

    public DbSet<CookbookShareRecipe> CookbookShareRecipes { get; set; }

    public async Task<List<Image>> GetImages(
        Guid id,
        ImageEntityType imageEntityType) =>
        await EntityImages
            .Where(ei => ei.EntityId == id && ei.ImageEntityType == imageEntityType)
            .Select(ei => ei.Image)
            .ToListAsync();

    public async Task<List<EntityImage>> GetEntityImages(
        Guid id,
        ImageEntityType imageEntityType) =>
        await EntityImages
            .Include(x => x.Image)
            .Where(ei => ei.EntityId == id && ei.ImageEntityType == imageEntityType)
            .ToListAsync();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure all DateTime properties to use UTC
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableDateTimeConverter);
                }
            }
        }

        // 1. Primary Keys (Defined on models, but confirming GUID text type)
        // EF Core maps C# string PKs to TEXT in SQLite by default.

        // 2. Recipe Constraints
        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.OriginalRecipe)
            .WithMany(r => r.Copies)
            .HasForeignKey(r => r.OriginalRecipeId)
            .IsRequired(false); // Can be NULL for original recipes

        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.RawDataSource)
            .WithMany()
            .HasForeignKey(r => r.RawDataSourceId)
            .IsRequired(false); // Can be NULL for user-created recipes

        modelBuilder.Entity<RecipeStepIngredient>()
            .HasOne(r => r.RecipeStep)
            .WithMany(r => r.StepIngredients)
            .HasForeignKey(r => r.RecipeStepId);

        modelBuilder.Entity<RecipeStep>()
            .HasIndex(s => new { s.RecipeId, s.StepNumber })
            .IsUnique(); // UNIQUE (recipe_id, step_number)

        // 4. Junction Table Configuration

        // RecipeTag (Many-to-Many setup with Composite Key)
        modelBuilder.Entity<RecipeTag>()
            .HasKey(rt => new { rt.RecipeId, rt.TagId });

        // RecipeCategory (Many-to-Many setup with Composite Key)
        modelBuilder.Entity<RecipeCategory>()
            .HasKey(rc => new { rc.RecipeId, rc.CategoryId });

        // RecipeHeart (Many-to-Many setup with Composite Key)
        modelBuilder.Entity<RecipeHeart>()
            .HasKey(rh => new { rh.AuthorId, rh.RecipeId });

        // Popularity (Index for efficient queries - NOT unique to allow multiple time-stamped entries)
        modelBuilder.Entity<Popularity>()
            .HasIndex(p => new { p.EntityType, p.EntityId, p.MetricType, p.CreatedAt });

        // 5. Polymorphic Image Mapping (EntityImage)
        // This is the key structural link for images across different entity types.
        modelBuilder.Entity<EntityImage>()
            .HasOne(ei => ei.Image)
            .WithMany(i => i.EntityImages)
            .HasForeignKey(ei => ei.ImageId);

        // We do NOT define direct FK relationships from EntityImage.EntityId 
        // to Recipe/Author/RecipeStep/Ingredient here, as EF Core does not support
        // polymorphic FKs directly. This link is managed at the application level
        // using the EntityId (GUID) and EntityType fields.

        // 6. RawDataSource (Unique constraint on URL)
        modelBuilder.Entity<RawDataSource>()
            .HasIndex(r => r.Url)
            .IsUnique();

        modelBuilder.Entity<AuthorLink>()
            .HasOne(al => al.Author)
            .WithMany(a => a.Links)
            .HasForeignKey(al => al.AuthorId);

        // --- 1. Author Images ---
        modelBuilder.Entity<Author>()
            .HasMany(a => a.EntityImages) // Author has many EntityImages
            .WithOne()
            .HasForeignKey(ei => ei.EntityId) // EntityImages joins on EntityId
            .HasPrincipalKey(a => a.AuthorId) // Author joins on AuthorId (converted to string)
            .IsRequired(false); // Use 'false' for optional relationships;

        // --- 2. Recipe Images ---
        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.EntityImages) // Recipe has many EntityImages
            .WithOne()
            .HasForeignKey(ei => ei.EntityId)
            .HasPrincipalKey(r => r.RecipeId)
            .IsRequired(false);

        // --- 3. Ingredient Images ---
        modelBuilder.Entity<Ingredient>()
            .HasMany(i => i.EntityImages) // Ingredient has many EntityImages
            .WithOne()
            .HasForeignKey(ei => ei.EntityId)
            .HasPrincipalKey(i => i.IngredientId)
            .IsRequired(false);

        // --- 4. RecipeSteps ---
        modelBuilder.Entity<RecipeStep>()
            .HasMany(i => i.EntityImages) // RecipeStep has many EntityImages
            .WithOne()
            .HasForeignKey(ei => ei.EntityId)
            .HasPrincipalKey(i => i.StepId)
            .IsRequired(false);

        // --- 5. Sharing Tables ---
        // RecipeShare unique index on share_token
        modelBuilder.Entity<RecipeShare>()
            .HasIndex(rs => rs.ShareToken)
            .IsUnique();

        // CookbookShare unique index on share_token
        modelBuilder.Entity<CookbookShare>()
            .HasIndex(cs => cs.ShareToken)
            .IsUnique();

        // CookbookShareRecipe composite key
        modelBuilder.Entity<CookbookShareRecipe>()
            .HasKey(csr => new { csr.CookbookShareId, csr.RecipeId });
    }
}