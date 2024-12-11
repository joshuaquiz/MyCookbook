using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MyCookbook.API.Interfaces;

public interface IIngredientsCache
{
    public IReadOnlyCollection<Ingredient> GetRecipeStepIngredientWhereIngredientDoExist(
        IEnumerable<RecipeStepIngredient> recipeStepIngredients);

    public IReadOnlyCollection<Ingredient> GetRecipeStepIngredientWhereIngredientDoesNotExist(
        IEnumerable<RecipeStepIngredient> recipeStepIngredients);

    public ValueTask LoadData(
        MyCookbookContext myCookbookContext,
        CancellationToken cancellationToken);

    public void AddData(
        IEnumerable<Ingredient> ingredients);
}