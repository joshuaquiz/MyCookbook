using MyCookbook.API.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyCookbook.API.Interfaces;

public interface ILdJsonExtractor
{
    public ValueTask<LdJsonAndRawPageData> ExtractLdJsonItems(
        Uri url,
        string? html,
        bool isReprocessing,
        CancellationToken cancellationToken);
}