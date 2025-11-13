using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;

namespace MyCookbook.API.Implementations.SiteParsers;

public sealed class DefaultSiteNormalizer
    : ISiteNormalizer
{
    public IEnumerable<Uri> GetUrlsToQueue(
        SiteWrapper wrapper) =>
        [];

    public Uri? NormalizeImageUrl(
        string? url) =>
        url == null
            ? null
            : new Uri(
                url);

    public ValueTask<SiteWrapper> NormalizeSite(
        SiteWrapper wrapper) =>
        ValueTask.FromResult(
            wrapper);
}