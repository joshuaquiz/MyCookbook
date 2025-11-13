using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;
using Schema.NET;

namespace MyCookbook.API.Implementations.SiteParsers;

public sealed partial class BudgetBytesNormalizer
    : ISiteNormalizer
{
    [GeneratedRegex(@"\?s=\d+&d=\w+&r=\w+")]
    private static partial Regex ResizeUrlData();

    public IEnumerable<Uri> GetUrlsToQueue(
        SiteWrapper wrapper)
    {
        foreach (var uri in wrapper.Persons?.SelectMany(x => x?.Url ?? []) ?? [])
        {
            yield return uri;
        }
    }

    public Uri? NormalizeImageUrl(
        string? uri)
    {
        if (uri == null)
        {
            return null;
        }

        try
        {
            return new Uri(
                ResizeUrlData()
                    .Replace(
                        uri,
                        string.Empty));
        }
        catch
        {
            return null;
        }
    }

    public async ValueTask<SiteWrapper> NormalizeSite(
        SiteWrapper wrapper)
    {
        if (wrapper.Recipes != null)
        {
            return await NormalizeRecipe(
                wrapper);
        }

        if (wrapper.ProfilePages != null)
        {
            return await NormalizeProfilePage(
                wrapper);
        }

        return wrapper;
    }

    private ValueTask<SiteWrapper> NormalizeRecipe(
        SiteWrapper wrapper)
    {
        var author = wrapper.Recipes![0].Author.Value2.First() as Person;
        wrapper.Recipes[0].Author = wrapper.Persons!.First(x => x.Id!.Fragment == author!.Id!.Fragment);
        return ValueTask.FromResult(wrapper);
    }

    private ValueTask<SiteWrapper> NormalizeProfilePage(
        SiteWrapper wrapper)
    {
        return ValueTask.FromResult(wrapper);
    }
}