using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCookbook.API.Interfaces;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Implementations;

public sealed class IngredientsCache
    : IIngredientsCache
{
    private static readonly SemaphoreSlim LockObj = new(1);

    private static readonly ConcurrentBag<Ingredient> Ingredients = [];

    private static readonly IEnumerable<string> IngredientNames = Ingredients
        .Select(
            x =>
                x.Name);

    public IReadOnlyCollection<Ingredient> GetRecipeStepIngredientWhereIngredientDoExist(
        IEnumerable<RecipeStepIngredient> recipeStepIngredients) =>
        recipeStepIngredients
            .Where(
                x =>
                    IngredientNames.Contains(
                        x.Ingredient.Name))
            .Select(
                x =>
                    x.Ingredient)
            .ToList();

    public IReadOnlyCollection<Ingredient> GetRecipeStepIngredientWhereIngredientDoesNotExist(
        IEnumerable<RecipeStepIngredient> recipeStepIngredients) =>
        recipeStepIngredients
            .Where(
                x =>
                    !IngredientNames.Contains(
                        x.Ingredient.Name))
            .Select(
                x =>
                    x.Ingredient)
            .ToList();

    public void AddData(
        IEnumerable<Ingredient> ingredients)
    {
        foreach (var ingredient in ingredients)
        {
            if (Ingredients.All(
                    x =>
                        x.Guid != ingredient.Guid))
            {
                Ingredients.Add(
                    ingredient);
            }
        }
    }

    public async ValueTask LoadData(
        MyCookbookContext myCookbookContext,
        CancellationToken cancellationToken)
    {
        if (Ingredients.Count > 0)
        {
            return;
        }

        await LockObj.WaitAsync(
            cancellationToken);
        if (Ingredients.Count > 0)
        {
            LockObj.Release();
            return;
        }

        foreach (var ingredient in await myCookbookContext.Ingredients.ToListAsync(cancellationToken))
        {
            Ingredients.Add(
                ingredient);
        }

        LockObj.Release();
    }
}
