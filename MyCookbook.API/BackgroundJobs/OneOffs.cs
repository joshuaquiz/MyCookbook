using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyCookbook.Common.Database;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.API.Interfaces;

namespace MyCookbook.API.BackgroundJobs;

public sealed partial class OneOffs(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory,
    ILogger<UrlDownloaderJob> logger,
    IUrlLdJsonDataNormalizer urlLdJsonDataNormalizer)
    : BackgroundService
{
    [GeneratedRegex("[ \t\r\n]{2,}")]
    private static partial Regex CollapseWhitespaceRegex();

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await using var db = await myCookbookContextFactory.CreateDbContextAsync(
                stoppingToken);
            var dataSources = await db.RawDataSources
                .Where(
                    x =>
                        !string.IsNullOrWhiteSpace(
                            x.LdJsonData))
                .ToListAsync(
                    stoppingToken);
            var tempFilePath = Path.GetTempFileName();
            foreach (var dataSource in dataSources)
            {
                var siteWrapper = await urlLdJsonDataNormalizer.NormalizeParsedLdJsonData(
                    db,
                    dataSource,
                    stoppingToken);
                var recipe = siteWrapper.Recipes?.ElementAtOrDefault(0);
                if (recipe != null)
                {
                    await File.WriteAllLinesAsync(
                        tempFilePath,
                        recipe.RecipeIngredient.ToArray(),
                        stoppingToken);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}