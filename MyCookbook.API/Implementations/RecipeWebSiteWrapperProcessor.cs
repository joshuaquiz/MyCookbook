using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;
using MyCookbook.Common;
using Schema.NET;

namespace MyCookbook.API.Implementations;

public sealed class RecipeWebSiteWrapperProcessor(
    ILdJsonExtractor ldJsonExtractor,
    ILdJsonSectionJsonObjectExtractor ldJsonSectionJsonObjectExtractor,
    IJsonNodeGraphExploder jsonNodeGraphExploder,
    IIngredientsCache ingredientsCache)
    : IRecipeWebSiteWrapperProcessor
{
    public async ValueTask<Recipe> ProcessWrapper(
        MyCookbookContext db,
        RecipeUrl recipeUrl,
        RecipeWebSiteWrapper wrapper,
        bool isReprocessing,
        CancellationToken cancellationToken)
    {
        var author = await GenerateAuthor(
            wrapper,
            cancellationToken);
        author = await db.Authors.FirstOrDefaultAsync(
            x => x.Name == author.Name,
            cancellationToken)
            ?? author;
        var generatedIngredients = GenerateIngredients(
            wrapper);
        var ingredientsToAdd = ingredientsCache.GetRecipeStepIngredientWhereIngredientDoesNotExist(
            generatedIngredients);
        var dbIngredientsForRecipe =
            ingredientsToAdd
                .Concat(
                    ingredientsCache.GetRecipeStepIngredientWhereIngredientDoExist(
                        generatedIngredients))
                .GroupBy(x => x.Name)
                .ToDictionary(
                    x => x.Key,
                    x => x.First());
        var stepNumber = 0;
        var firstStep = new RecipeStep
        {
            StepNumber = stepNumber++,
            Instructions = "Preparation",
            RecipeStepType = RecipeStepType.PrepStep
        };
        var recipeIngredients = generatedIngredients
            .Select(x =>
                new RecipeStepIngredient
                {
                    Ingredient = dbIngredientsForRecipe[x.Ingredient.Name],
                    Measurement = x.Measurement,
                    Quantity = x.Quantity,
                    RecipeStep = firstStep
                })
            .ToList();
        firstStep.RecipeIngredients = recipeIngredients;
        var recipeSteps = new List<RecipeStep>
        {
            firstStep
        };
        recipeSteps.AddRange(
            wrapper.Recipe
                ?.RecipeInstructions
                .Select(
                    x =>
                        new RecipeStep
                        {
                            Instructions = (x as ICreativeWork)?.Text,
                            RecipeStepType = RecipeStepType.CookingStep,
                            StepNumber = stepNumber++
                        })
            ?? []);
        Recipe? recipe = null;
        if (isReprocessing)
        {
            recipe = await db.Recipes
                .FirstOrDefaultAsync(
                    x => x.Name == wrapper.Recipe!.Name,
                    cancellationToken);
        }

        recipe ??= new Recipe();
        recipe.RecipeUrl = recipeUrl;
        recipe.Name = wrapper.Recipe!.Name!;
        recipe.Description = wrapper.Recipe!.Description!;
        recipe.Author = author;
        recipe.Image = wrapper.Recipe.Image;
        recipe.TotalTime = wrapper.Recipe.TotalTime.ToArray().FirstOrDefault() ?? TimeSpan.Zero;
        recipe.RecipeSteps = recipeSteps;
        if (recipe.Image != null
            && wrapper.ImageUrl != null)
        {
            recipe.Image = new Uri(
                wrapper.ImageUrl,
                UriKind.Absolute);
        }

        ingredientsCache.AddData(
            recipeIngredients
                .Select(
                    x =>
                        x.Ingredient));
        return recipe;
    }

    private async Task<Author> GenerateAuthor(
        RecipeWebSiteWrapper wrapper,
        CancellationToken cancellationToken)
    {
        var author = new Author();
        var recipeAuthor = wrapper.Recipe?.Author;
        if (!recipeAuthor.HasValue
            || !recipeAuthor.Value.Any())
        {
            recipeAuthor = wrapper.Recipe?.Publisher;
        }

        if (recipeAuthor?.HasValue1 == true)
        {
            var org = recipeAuthor.Value.Value1.FirstOrDefault();
            author.Name = org?.Name.FirstOrDefault()
                          ?? string.Empty;
            author.Image = org?.Logo;
            author.BackgroundImage = org?.Image;
        }
        else if (recipeAuthor?.HasValue2 == true)
        {
            var person = recipeAuthor.Value.Value2.FirstOrDefault();
            author.Name = string.Join(' ', person?.Name ?? Enumerable.Empty<string>());
            author.Name = author.Name
                .Split(
                    ':',
                    '|',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .First();
            if (person?.Image == null
                && person?.Url != null)
            {
                var results = await ldJsonExtractor.ExtractLdJsonItems(
                    person.Url.First(),
                    null,
                    false,
                    cancellationToken);
                if (results.HttpStatus == HttpStatusCode.OK)
                {
                    var jsonObjects = results
                        .Data
                        .SelectMany(ldJsonSectionJsonObjectExtractor.GetJsonObjectsFromLdJsonSection)
                        .SelectMany(jsonNodeGraphExploder.ExplodeIfGraph)
                        .Select(
                            x =>
                                new KeyValuePair<string, JsonObject>(
                                    x["@type"]?.ToString() ?? Guid.NewGuid().ToString(),
                                    x))
                        .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value);
                    var profilePage = jsonObjects["ProfilePage"];

                }
            }
            else
            {
                author.Image = person?.Image;
            }
        }
        else if (wrapper.Person != null)
        {
            author.Name = wrapper.Person.Name.FirstOrDefault()
                          ?? string.Empty;
            author.Image = wrapper.Person.Image;
        }
        else
        {
            throw new Exception("Unable to parse an author");
        }

        return author;
    }

    private static List<RecipeStepIngredient> GenerateIngredients(
        RecipeWebSiteWrapper wrapper) =>
        wrapper.Recipe
            ?.RecipeIngredient
            .SelectMany(RecipeIngredientParser.Parse)
            .ToList()
        ?? [];
}