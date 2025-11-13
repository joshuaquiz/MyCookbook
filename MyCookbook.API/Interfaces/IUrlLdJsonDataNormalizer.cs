using System.Threading;
using System.Threading.Tasks;
using MyCookbook.API.Models;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Interfaces;

public interface IUrlLdJsonDataNormalizer
{
    public ValueTask<SiteWrapper> NormalizeParsedLdJsonData(
        MyCookbookContext db,
        RawDataSource dataSource,
        CancellationToken cancellationToken);
}