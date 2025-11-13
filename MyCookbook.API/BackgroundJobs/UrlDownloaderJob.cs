using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;
using MyCookbook.Common.Database;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MyCookbook.API.BackgroundJobs;

public sealed class UrlDownloaderJob(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory,
    ILdJsonExtractor ldJsonExtractor,
    ILogger<UrlDownloaderJob> logger,
    IUrlLdJsonDataNormalizer urlLdJsonDataNormalizer,
    IRecipeWebSiteWrapperProcessor recipeWebSiteWrapperProcessor)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var latestParserVersion = Enum.GetValues<ParserVersion>().Max();
                foreach (var dataSource in (await db.RawDataSources
                             /*.Where(
                                 x =>
                                     x.ProcessingStatus == RecipeUrlStatus.NotStarted
                                     || x.ProcessingStatus == RecipeUrlStatus.Downloading)*/
                             .Where(x => !x.UrlHost.Contains("food"))
                             .GroupBy(x => x.UrlHost)
                             .ToDictionaryAsync(
                                 x => x.Key,
                                 x => x.Take(5).ToList(),
                                 stoppingToken))
                         .SelectMany(x => x.Value))
                {
                    if (dataSource.UrlHost != "www.budgetbytes.com")
                    {
                        continue;
                    }
                    logger.LogInformation(
                        $"Starting to process {dataSource.Url}");
                    dataSource.ProcessingStatus = RecipeUrlStatus.Downloading;
                    dataSource.ParserVersion = latestParserVersion;
                    await db.SaveChangesAsync(
                        stoppingToken);
                    var delay = string.IsNullOrEmpty(dataSource.RawHtml)
                        ? TimeSpan.FromSeconds(
                            Random.Shared.Next(
                                3,
                                20))
                        : TimeSpan.Zero;
                    try
                    {
                        var results = await ldJsonExtractor.ExtractLdJsonItems(
                            dataSource.Url,
                            dataSource.RawHtml,
                            stoppingToken);
                        dataSource.RawHtml = results.RawHtml;
                        var jsonSections = JsonSerializer.Serialize(
                            results.Data);
                        logger.LogInformation(
                            $"{dataSource.Url} - extracted {results.Data.Count} ld+json sections {jsonSections}");
                        dataSource.LdJsonData = jsonSections;
                        dataSource.ProcessingStatus = RecipeUrlStatus.DownloadSucceeded;
                    }
                    catch (Exception e)
                    {
                        dataSource.Error = e.ToString();
                        dataSource.ProcessingStatus = RecipeUrlStatus.DownloadFailed;
                        delay = TimeSpan.FromMinutes(3);
                    }
                    finally
                    {
                        dataSource.ProcessedDatetime = DateTime.Now;
                    }

                    await db.SaveChangesAsync(
                        stoppingToken);
                    logger.LogInformation(
                        $"Delaying for processor for {dataSource.Url.Host} for {delay:g}");

                    var timer = Stopwatch.StartNew();
                    // From: WebSiteParserJob
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
                    // End: WebSiteParserJob

                    timer.Stop();
                    await Task.Delay(
                        TimeSpan.FromSeconds(Math.Max(0, delay.TotalSeconds - timer.Elapsed.TotalSeconds)),
                        stoppingToken);
                }
            }
            catch
            {
                // Do nothing.
            }
        }
    }
}