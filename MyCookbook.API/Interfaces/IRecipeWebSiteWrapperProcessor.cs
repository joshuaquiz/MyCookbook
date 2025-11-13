using System.Threading;
using System.Threading.Tasks;
using MyCookbook.API.Models;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Interfaces;

public interface IRecipeWebSiteWrapperProcessor
{
    public ValueTask ProcessWrapper(
        MyCookbookContext db,
        RawDataSource rawDataSource,
        SiteWrapper wrapper,
        CancellationToken cancellationToken);
}