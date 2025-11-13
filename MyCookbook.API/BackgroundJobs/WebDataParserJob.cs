using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MyCookbook.API.Interfaces;
using MyCookbook.Common.Database;

namespace MyCookbook.API.BackgroundJobs;

public sealed class WebDataParserJob(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory,
    IUrlLdJsonDataNormalizer urlLdJsonDataNormalizer,
    IRecipeWebSiteWrapperProcessor recipeWebSiteWrapperProcessor)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var db = await myCookbookContextFactory.CreateDbContextAsync(
                    stoppingToken);
                var rawDataSources = await db.RawDataSources
                    .Where(
                        x =>
                            x.ProcessingStatus == RecipeUrlStatus.DownloadSucceeded
                            || x.ProcessingStatus == RecipeUrlStatus.Parsing)
                    .Take(100)
                    .ToListAsync(
                        stoppingToken);
                foreach (var dataSource in rawDataSources)
                {
                    try
                    {
                        var siteWrapper = await urlLdJsonDataNormalizer.NormalizeParsedLdJsonData(
                            db,
                            dataSource,
                            stoppingToken);
                        await recipeWebSiteWrapperProcessor.ProcessWrapper(
                            db,
                            dataSource,
                            siteWrapper,
                            stoppingToken);
                        dataSource.ProcessingStatus = RecipeUrlStatus.FinishedSuccess;
                    }
                    catch (Exception e)
                    {
                        dataSource.Error = e.ToString();
                        dataSource.ProcessingStatus = RecipeUrlStatus.FinishedError;
                    }
                    finally
                    {
                        await db.SaveChangesAsync(
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
}