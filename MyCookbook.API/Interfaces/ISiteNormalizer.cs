using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyCookbook.API.Models;

namespace MyCookbook.API.Interfaces;

public interface ISiteNormalizer
{
    public IEnumerable<Uri> GetUrlsToQueue(
        SiteWrapper wrapper);

    public Uri? NormalizeImageUrl(
        string? url);

    public ValueTask<SiteWrapper> NormalizeSite(
        SiteWrapper wrapper);
}