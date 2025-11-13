using System.Collections.Generic;
using System.Collections.ObjectModel;
using MyCookbook.API.Interfaces;

namespace MyCookbook.API.Implementations.SiteParsers;

public sealed class SiteNormalizerFactory
    : ISiteNormalizerFactory
{
    private static readonly ReadOnlyDictionary<string, ISiteNormalizer> SiteParsers =
        new(
            new Dictionary<string, ISiteNormalizer>
            {
                { "www.foodnetwork.com", new FoodNetworkNormalizer() },
                { "www.cookingchanneltv.com", new FoodNetworkNormalizer() },
                { "www.budgetbytes.com", new BudgetBytesNormalizer() }
            });

    public ISiteNormalizer GetSiteNormalizer(
        string host) =>
        SiteParsers.TryGetValue(
            host,
            out var siteNormalizer)
            ? siteNormalizer
            : new DefaultSiteNormalizer();
}