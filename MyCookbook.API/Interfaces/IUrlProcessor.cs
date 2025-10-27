using System.Threading;
using System.Threading.Tasks;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Interfaces;

public interface IUrlProcessor
{
    public ValueTask<Recipe?> ProcessUrl(
        MyCookbookContext db,
        RecipeUrl recipeUrl,
        bool isReprocessing,
        CancellationToken cancellationToken);
}