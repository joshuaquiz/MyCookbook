using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Implementations;
using MyCookbook.API.Implementations.SiteParsers;
using MyCookbook.Common.Database;
using Xunit;

namespace MyCookbook.UnitTests;

public static class ProcessTests
{
    [Fact]
    public static async Task UrlProcessorTest()
    {
        var loggerFactory = LoggerFactory.Create(x => x.AddConsole());
        var ldJsonSectionJsonObjectExtractor = new LdJsonSectionJsonObjectExtractor();
        var jsonNodeGraphExploder = new JsonNodeGraphExploder();
        var urlProcessor = new UrlLdJsonDataNormalizer(
            ldJsonSectionJsonObjectExtractor,
            jsonNodeGraphExploder,
            new SiteNormalizerFactory(),
            loggerFactory.CreateLogger<UrlLdJsonDataNormalizer>());
        await urlProcessor.NormalizeParsedLdJsonData(
            new MyCookbookContext(new DbContextOptions<MyCookbookContext>()),
            new RawDataSource
            {
                Url = new Uri(
                    "https://www.foodnetwork.com/recipes/ree-drummond/chicken-fried-steak-with-gravy-recipe-1925056",
                    UriKind.Absolute)
            },
            CancellationToken.None);
    }
}