using System.Threading;
using System.Threading.Tasks;
using MyCookbook.API.Models;

namespace MyCookbook.API.Interfaces;

public interface IRecipeWebSiteWrapperProcessor
{
    public ValueTask<Recipe> ProcessWrapper(
        MyCookbookContext db,
        RecipeUrl recipeUrl,
        RecipeWebSiteWrapper wrapper,
        bool isReprocessing,
        CancellationToken cancellationToken);
}