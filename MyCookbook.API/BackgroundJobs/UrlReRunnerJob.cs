using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MyCookbook.Common.Database;

namespace MyCookbook.API.BackgroundJobs;

public sealed class UrlReRunnerJob(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await using var db = await myCookbookContextFactory.CreateDbContextAsync(
                stoppingToken);
            var latestParser = Enum.GetValues<ParserVersion>().Max();
            var recipeUrls = await db.RawDataSources
                .Where(
                    x =>
                        x.ProcessingStatus > RecipeUrlStatus.NotStarted
                        && x.ParserVersion > 0
                        && x.ParserVersion < latestParser)
                .ToListAsync(
                    stoppingToken);
            foreach (var recipeUrl in recipeUrls)
            {
                recipeUrl.ProcessingStatus = RecipeUrlStatus.NotStarted;
            }

            await db.SaveChangesAsync(
                stoppingToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}