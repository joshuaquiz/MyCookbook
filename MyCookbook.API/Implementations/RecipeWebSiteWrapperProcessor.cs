using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using MyCookbook.API.Exceptions;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;
using MyCookbook.Common.Database;
using MyCookbook.Common.Enums;
using Schema.NET;
using Recipe = MyCookbook.Common.Database.Recipe;

namespace MyCookbook.API.Implementations;

public sealed partial class RecipeWebSiteWrapperProcessor(
    IUrlQueuerFromJsonObjectMap urlQueuerFromJsonObjectMap,
    ISiteNormalizerFactory siteNormalizerFactory,
    IJobQueuer jobQueuer)
    : IRecipeWebSiteWrapperProcessor
{
    [GeneratedRegex(@"^(?:(?<Number>\d+)|((?<Range>(?<R1>\d+)\s+(?:to|or|\-)\s+(?<R2>\d+))))(?: (?:servings|people))$")]
    private static partial Regex Servings();

    public async ValueTask ProcessWrapper(
        MyCookbookContext db,
        RawDataSource rawDataSource,
        SiteWrapper wrapper,
        CancellationToken cancellationToken)
    {
        var firstUrl = wrapper.Recipes?.FirstOrDefault()?.Url.FirstOrDefault();
        if (firstUrl != null
            && firstUrl != rawDataSource.Url)
        {
            var sameAs = await db.RawDataSources.FirstOrDefaultAsync(
                x => x.Url == firstUrl,
                cancellationToken);
            if (sameAs != null)
            {
                rawDataSource.SameAs = sameAs.SourceId;
                return;
            }
        }

        var siteParser = siteNormalizerFactory.GetSiteNormalizer(
            rawDataSource.UrlHost);
        foreach (var url in urlQueuerFromJsonObjectMap
                     .QueueUrlsFromJsonObjectMap(
                         wrapper.LdJsonObjects)
                     .Concat(
                         siteParser.GetUrlsToQueue(
                             wrapper)))
        {
            await jobQueuer.QueueUrlProcessingJob(
                db,
                url);
        }

        if (wrapper.Recipes is { Count: > 0 })
        {
            if (wrapper.Recipes is { Count: > 1 })
            {
                throw new MultipleLdTypesFoundException(nameof(wrapper.Recipes));
            }

            await ProcessRecipe(
                db,
                rawDataSource,
                wrapper,
                siteParser,
                cancellationToken);
        }
        else if (wrapper.ProfilePages is { Count: > 0 })
        {
            if (wrapper.ProfilePages is { Count: > 1 })
            {
                throw new MultipleLdTypesFoundException(nameof(wrapper.ProfilePages));
            }

            await ProcessProfilePage(
                db,
                rawDataSource,
                wrapper,
                siteParser,
                cancellationToken);
        }
        else
        {
            rawDataSource.PageType = PageType.Breadcrumb;
        }

        await db.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task ProcessProfilePage(
        MyCookbookContext db,
        RawDataSource rawDataSource,
        SiteWrapper wrapper,
        ISiteNormalizer siteNormalizer,
        CancellationToken cancellationToken)
    {
        rawDataSource.PageType = PageType.Author;
        await GenerateAuthor(
            db,
            wrapper.ProfilePages![0].MainEntity.FirstOrDefault() as Person
            ?? wrapper.Persons?.FirstOrDefault()
            ?? wrapper.ProfilePages![0].Publisher,
            siteNormalizer,
            cancellationToken);
    }

    private static async Task ProcessRecipe(
        MyCookbookContext db,
        RawDataSource rawDataSource,
        SiteWrapper wrapper,
        ISiteNormalizer siteNormalizer,
        CancellationToken cancellationToken)
    {
        rawDataSource.PageType = PageType.Recipe;
        var author = await GenerateAuthor(
            db,
            wrapper.Recipes![0].Author,
            siteNormalizer,
            cancellationToken);
        var wrapperRecipe = wrapper.Recipes[0];
        var recipeName = string.Join(' ', wrapperRecipe.Name.ToArray());
        var recipe = await db.Recipes
            .Include(x => x.Steps)
            .Include(x => x.EntityImages)
            .Include(x => x.RecipeTags)
            .Include(x => x.RecipeCategories)
            .FirstOrDefaultAsync(
                x => x.Title == recipeName,
                cancellationToken);
        if (recipe == null)
        {
            var fromDb = await db.Recipes.AddAsync(
                new Recipe
                {
                    Title = recipeName,
                    RawDataSource = rawDataSource,
                    EntityImages = [],
                    Steps = [],
                    RecipeTags = [],
                    RecipeCategories = []
                },
                cancellationToken);
            recipe = fromDb.Entity;
        }

        recipe.Description = string.Join(' ', wrapperRecipe.Description.ToArray());
        recipe.Author = author;
        recipe.PrepTimeMinutes = wrapperRecipe.PrepTime.Count > 0
            ? (int)wrapperRecipe.PrepTime.First()!.Value.TotalMinutes
            : null;
        recipe.CookTimeMinutes = wrapperRecipe.CookTime.Count > 0
            ? (int)wrapperRecipe.CookTime.First()!.Value.TotalMinutes
            : null;
        var stepNumber = 1;
        foreach (var instruction in wrapperRecipe.RecipeInstructions)
        {
            var instructions = string.Join(' ', (instruction as IHowToStep)?.Text.ToArray() ?? []);
            var stepInDb = await db.RecipeSteps
                .Include(recipeStep => recipeStep.StepIngredients)
                .FirstOrDefaultAsync(
                    x =>
                        x.RecipeId == recipe.RecipeId
                        && x.StepNumber == stepNumber,
                    cancellationToken);
            var fistStepInMultiStep = stepNumber == 1
                                      && wrapperRecipe.RecipeInstructions is { HasValue: true, Count: > 1 };
            if (stepInDb == null)
            {
                var fromDb = await db.RecipeSteps.AddAsync(
                    new RecipeStep
                    {
                        Instructions = instructions,
                        RecipeStepType = fistStepInMultiStep
                            ? RecipeStepType.PrepStep
                            : RecipeStepType.CookingStep,
                        RecipeId = recipe.RecipeId,
                        StepNumber = stepNumber,
                        StepIngredients = []
                    },
                    cancellationToken);
                stepInDb = fromDb.Entity;
                recipe.Steps.Add(stepInDb);
            }
            else
            {
                stepInDb.Instructions = instructions;
                stepInDb.RecipeStepType = fistStepInMultiStep
                    ? RecipeStepType.PrepStep
                    : RecipeStepType.CookingStep;
            }

            if (stepNumber == 1)
            {
                stepInDb.StepIngredients = await GenerateStepIngredients(
                    wrapper,
                    db,
                    recipe,
                    stepInDb,
                    cancellationToken);
            }
            else if (stepInDb.StepIngredients.Count > 0)
            {
                db.StepIngredients.RemoveRange(stepInDb.StepIngredients);
                stepInDb.StepIngredients.Clear();
            }

            stepNumber++;
        }

        var imagesFromWrapper = GetImageUrls(
            siteNormalizer,
            wrapperRecipe.Image,
            new OneOrMany<Values<IImageObject, Uri>>(wrapper.ImageObjects ?? []));
        var imagesInDb = await db.GetImages(
                recipe.RecipeId,
                ImageEntityType.Recipe);
        if (imagesFromWrapper.Count > 0)
        {
            var hasExistingMainImage = imagesInDb
                .Any(x =>
                    x.Url == imagesFromWrapper[0]
                    && x.ImageType == ImageType.Main);
            if (!hasExistingMainImage)
            {
                var fromDb = await db.EntityImages.AddAsync(
                    new EntityImage
                    {
                        EntityId = recipe.RecipeId,
                        ImageEntityType = ImageEntityType.Recipe,
                        Image = new Image
                        {
                            ImageType = ImageType.Main,
                            Url = imagesFromWrapper[0]
                        }
                    },
                    cancellationToken);
                recipe.EntityImages.Add(
                    fromDb.Entity);
            }
        }

        if (imagesFromWrapper.Count > 1)
        {
            var hasExistingMainImage = imagesInDb
                .Any(x =>
                    x.Url == imagesFromWrapper[1]
                    && x.ImageType == ImageType.Background);
            if (!hasExistingMainImage)
            {
                var fromDb = await db.EntityImages.AddAsync(
                    new EntityImage
                    {
                        EntityId = recipe.RecipeId,
                        ImageEntityType = ImageEntityType.Recipe,
                        Image = new Image
                        {
                            ImageType = ImageType.Background,
                            Url = imagesFromWrapper[1]
                        }
                    },
                    cancellationToken);
                recipe.EntityImages.Add(
                    fromDb.Entity);
            }
        }

        var servingsStr = wrapperRecipe.RecipeYield.HasValue1
            ? wrapperRecipe.RecipeYield.Value1.First().Value.Value2.FirstOrDefault().ToString()
            : wrapperRecipe.RecipeYield.HasValue2
                ? wrapperRecipe.RecipeYield.Value2.First()
                : string.Empty;
        var servingsMatch = servingsStr != null
            ? Servings().Match(servingsStr)
            : null;
        if (servingsMatch != null)
        {
            if (servingsMatch.Groups["Number"].Success)
            {
                recipe.Servings = servingsMatch.Groups["Number"].Value;
            }
            else if (servingsMatch.Groups["Range"].Success)
            {
                var avgServings = (int)Math.Ceiling(
                    new []
                    {
                        int.Parse(servingsMatch.Groups["R1"].Value),
                        int.Parse(servingsMatch.Groups["R2"].Value)
                    }.Average());
                recipe.Servings = avgServings.ToString();
            }
        }

        if (wrapperRecipe.AggregateRating != default)
        {
            var aggregateRating = wrapperRecipe.AggregateRating.ToArray().FirstOrDefault();
            if (aggregateRating is { RatingValue.HasValue: true })
            {
                var ratingValue = aggregateRating.RatingValue;
                if (ratingValue.HasValue1)
                {
                    recipe.Rating = (decimal?)ratingValue.Value1.First();
                }
                else if (ratingValue.HasValue2)
                {
                    if (decimal.TryParse(ratingValue.Value2.First(), out var parsedRating))
                    {
                        recipe.Rating = parsedRating;
                    }
                }
            }
        }

        if (wrapperRecipe.Keywords != default)
        {
            var keywords = new List<string>();
            if (wrapperRecipe.Keywords.HasValue1)
            {
                keywords.AddRange(
                    wrapperRecipe.Keywords
                        .Value1
                        .ToArray()
                        .SelectMany(
                            x =>
                                x.Split(
                                    ',',
                                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)));
            }

            foreach (var keyword in keywords)
            {
                var tag = await db.Tags.FirstOrDefaultAsync(t => t.TagName == keyword, cancellationToken);
                if (tag == null)
                {
                    tag = new Tag { TagName = keyword };
                    await db.Tags.AddAsync(tag, cancellationToken);
                }

                if (recipe.RecipeTags.All(rt => rt.TagId != tag.TagId))
                {
                    recipe.RecipeTags.Add(new RecipeTag { RecipeId = recipe.RecipeId, Tag = tag });
                }
            }
        }

        if (wrapperRecipe.RecipeCategory != default)
        {
            var categories = wrapperRecipe.RecipeCategory.ToArray();
            foreach (var categoryName in categories)
            {
                var casedCategoryName = categoryName.Transform(To.SentenceCase);
                var category = await db.Categories.FirstOrDefaultAsync(c => c.CategoryName == casedCategoryName, cancellationToken);
                if (category == null)
                {
                    category = new Category { CategoryName = casedCategoryName };
                    await db.Categories.AddAsync(category, cancellationToken);
                }

                if (recipe.RecipeCategories.All(rc => rc.CategoryId != category.CategoryId))
                {
                    recipe.RecipeCategories.Add(new RecipeCategory { RecipeId = recipe.RecipeId, Category = category });
                }
            }
        }

        await db.SaveChangesAsync(
            cancellationToken);
    }

    private static IReadOnlyList<Uri> GetImageUrls(
        ISiteNormalizer siteNormalizer,
        params OneOrMany<Values<IImageObject, Uri>>?[] images)
    {
        var imagesWithValue = images.Where(x => x.HasValue).SelectMany(x => x!.Value).ToList();
        return imagesWithValue
            .Where(x => x.HasValue1)
            .SelectMany(x =>
            {
                var imageArray = x.Value1.ToArray();
                return imageArray
                    .Concat(imageArray.SelectMany(y => y.Thumbnail));
            })
            .SelectMany(x => x.Url)
            .Concat(imagesWithValue
                .Where(x => x.HasValue2)
                .SelectMany(x => x.Value2))
            .Select(x => siteNormalizer.NormalizeImageUrl(x.AbsoluteUri)!)
            .DistinctBy(x => x.AbsoluteUri)
            .ToList();
    }

    private static async Task<Author> GenerateAuthor(
        MyCookbookContext db,
        Values<IOrganization, IPerson> authorData,
        ISiteNormalizer siteNormalizer,
        CancellationToken cancellationToken)
    {
        string? name;
        string? bio;
        string? location;
        Uri? mainImageUrl;
        Uri? backgroundImageUrl;
        if (authorData.HasValue1)
        {
            var org = authorData.Value1.First();
            name = org.Name.FirstOrDefault()
                          ?? string.Empty;
            bio = org.Description;
            location = org.Location.ToString();
            mainImageUrl = siteNormalizer.NormalizeImageUrl(org.Logo.ToString());
            backgroundImageUrl = siteNormalizer.NormalizeImageUrl(org.Image.ToString());
        }
        else if (authorData.HasValue2)
        {
            var person = authorData.Value2.First();
            name = string.Join(' ', person.Name.ToArray());
            name = name
                .Split(
                    ':',
                    '|',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .First();
            bio = person.Description.HasValue1
                ? string.Join(' ', person.Description.Value1.ToArray())
                : null;
            location = person.Address.HasValue1
                ? person.Address.Value1.First().AddressCountry
                : person.Address.HasValue2
                    ? string.Join(' ', person.Address.Value2.ToArray())
                    : null;
            mainImageUrl = GetImageUrls(
                    siteNormalizer,
                    person.Image)
                .FirstOrDefault();
            backgroundImageUrl = GetImageUrls(
                    siteNormalizer,
                    person.Image)
                .ElementAtOrDefault(1);
        }
        else
        {
            throw new Exception("Unable to parse an author");
        }

        var authorInDb = await db.Authors
            .FirstOrDefaultAsync(
                     x => x.Name == name,
                     cancellationToken);
        if (authorInDb != null)
        {
            authorInDb.Name = name;
            authorInDb.Bio = bio ?? authorInDb.Bio;
            authorInDb.IsVisible = true;
        }
        else
        {
            var fromDb = await db.Authors.AddAsync(
                new Author
                {
                    Name = name,
                    Bio = bio ?? string.Empty,
                    IsVisible = true,
                    AuthorType = AuthorType.ImportedProfile,
                    Location = location
                },
                cancellationToken);
            authorInDb = fromDb.Entity;
        }

        var authorImages = await db.GetImages(
            authorInDb.AuthorId,
            ImageEntityType.Author);
        if (mainImageUrl != null
            && (authorImages.Count == 0
                || !authorImages.All(x => x.Url == mainImageUrl && x.ImageType == ImageType.Main)))
        {
            var imageFromDb = await db.Images.FirstOrDefaultAsync(
                x => x.Url == mainImageUrl,
                cancellationToken)
                ?? new Image
                {
                    ImageType = ImageType.Main,
                    Url = mainImageUrl
                };
            var fromDb = await db.EntityImages
                .AddAsync(
                    new EntityImage
                    {
                        EntityId = authorInDb.AuthorId,
                        ImageEntityType = ImageEntityType.Author,
                        Image = imageFromDb
                    },
                    cancellationToken);
            authorInDb.EntityImages.Add(fromDb.Entity);
        }

        if (backgroundImageUrl != null
            && (authorImages.Count == 0
                || !authorImages.All(x => x.Url == backgroundImageUrl && x.ImageType == ImageType.Background)))
        {
            var imageFromDb = await db.Images.FirstOrDefaultAsync(
                                  x => x.Url == mainImageUrl,
                                  cancellationToken)
                              ?? new Image
                              {
                                  ImageType = ImageType.Background,
                                  Url = backgroundImageUrl
                              };
            var fromDb = await db.EntityImages
                .AddAsync(
                    new EntityImage
                    {
                        EntityId = authorInDb.AuthorId,
                        ImageEntityType = ImageEntityType.Author,
                        Image = imageFromDb
                    },
                    cancellationToken);
            authorInDb.EntityImages.Add(fromDb.Entity);
        }

        await db.SaveChangesAsync(
            cancellationToken);
        return authorInDb;
    }

    private static async Task<List<RecipeStepIngredient>> GenerateStepIngredients(
        SiteWrapper wrapper,
        MyCookbookContext db,
        Recipe recipe,
        RecipeStep recipeStep,
        CancellationToken cancellationToken)
    {
        var ingredientsAlreadyMatchedAdded = new List<Ingredient>();
        var recipeStepIngredients = new List<RecipeStepIngredient>();
        foreach (var recipeIngredient in wrapper.Recipes?[0].RecipeIngredient ?? [])
        {
            await foreach (var recipeStepIngredient in RecipeIngredientParser.ParseRecipeStepIngredients(
                               db,
                               recipe.RecipeId,
                               recipeStep.StepId,
                               recipeIngredient,
                               ingredientsAlreadyMatchedAdded,
                               cancellationToken))
            {
                recipeStepIngredients.Add(recipeStepIngredient);
            }
        }

        return recipeStepIngredients;
    }
}