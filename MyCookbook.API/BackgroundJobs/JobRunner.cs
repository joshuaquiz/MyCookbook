using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;

namespace MyCookbook.API.BackgroundJobs;

public record HostStatus(
    string Host)
{
    public bool IsWaiting { get; set; }

    public string? WaitingUntil { get; set; }

    public string? FullUrl { get; set; }

    public bool InProgress { get; set; }
}

public sealed class JobRunner(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory,
    IUrlProcessor urlProcessor,
    IIngredientsCache ingredientsCache,
    ILogger<JobRunner> logger)
    : BackgroundService
{
    public static IDictionary<string, HostStatus> HostStatuses = new ConcurrentDictionary<string, HostStatus>();

    private readonly TaskFactory _taskFactory = new();
    private readonly IDictionary<string, Task> _tasks = new Dictionary<string, Task>();

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
            stoppingToken);
        await ingredientsCache.LoadData(
            db,
            stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var hosts = await db.RecipeUrls
                    .Where(
                        x =>
                            x.ProcessingStatus == RecipeUrlStatus.NotStarted
                            || x.ProcessingStatus == RecipeUrlStatus.Started)
                    .Select(x => x.Host)
                    .Distinct()
                    .ToListAsync(
                        stoppingToken);
                foreach (var host in hosts
                             .Where(x => !_tasks.ContainsKey(x)))
                {
                    HostStatuses[host] = new HostStatus(
                        host);
                    _tasks.Add(
                        host,
                        _taskFactory.StartNew(
                            async () =>
                                await ProcessHost(
                                    host,
                                    stoppingToken),
                            stoppingToken,
                            TaskCreationOptions.LongRunning,
                            TaskScheduler.Default));
                }

                var delay = TimeSpan.FromMinutes(1);
                logger.LogInformation(
                    $"Delaying for host checking {delay:g}");
                await Task.Delay(
                    delay,
                    stoppingToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    private async Task ProcessHost(
        string host,
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var db = await myCookbookContextFactory.CreateDbContextAsync(
                    stoppingToken);
                var recipeUrl = await db.RecipeUrls.FirstOrDefaultAsync(
                    x =>
                        x.Host == host
                        && (x.ProcessingStatus == RecipeUrlStatus.NotStarted
                            || x.ProcessingStatus == RecipeUrlStatus.Started),
                    stoppingToken);
                if (recipeUrl != null)
                {
                    HostStatuses[host].FullUrl = recipeUrl.Uri.ToString();
                    HostStatuses[host].InProgress = true;
                    logger.LogInformation(
                        $"Starting to process {recipeUrl.Uri}");
                    recipeUrl.ParserVersion = Enum.GetValues(
                            typeof(ParserVersion))
                        .Cast<ParserVersion>()
                        .Max();
                    recipeUrl.ProcessingStatus = RecipeUrlStatus.Started;
                    await db.SaveChangesAsync(
                        stoppingToken);
                    try
                    {
                        var recipe = await urlProcessor.ProcessUrl(
                            db,
                            recipeUrl,
                            false,
                            stoppingToken);
                        if (recipe != null)
                        {
                            await db.Recipes.AddAsync(
                                recipe,
                                stoppingToken);
                            await db.SaveChangesAsync(
                                stoppingToken);
                        }

                        recipeUrl.ProcessingStatus = RecipeUrlStatus.FinishedSuccess;
                    }
                    catch (TaskCanceledException e)
                    {
                        logger.LogError(
                            e.Message,
                            e);
                        recipeUrl.Exception = $"{e.Message}{Environment.NewLine}{e.StackTrace}";
                        recipeUrl.ProcessingStatus = RecipeUrlStatus.FinishedError;
                    }
                    catch (Exception e)
                    {
                        logger.LogError(
                            e.Message,
                            e);
                        recipeUrl.Exception = $"{e.Message}{Environment.NewLine}{e.StackTrace}";
                        recipeUrl.ProcessingStatus = RecipeUrlStatus.FinishedError;
                    }
                    finally
                    {
                        recipeUrl.CompletedAt = DateTimeOffset.UtcNow;
                        await db.SaveChangesAsync(
                            stoppingToken);
                    }

                    HostStatuses[host].InProgress = false;
                }

                var delay = recipeUrl?.StatusCode == HttpStatusCode.OK
                    ? TimeSpan.FromSeconds(
                        Random.Shared.Next(
                            3,
                            20))
                    : TimeSpan.FromMinutes(3);
                logger.LogInformation(
                    $"Delaying for processor for {host} for {delay:g}");
                HostStatuses[host].IsWaiting = true;
                HostStatuses[host].WaitingUntil = DateTime.Now.Add(delay).ToString("s");
                await Task.Delay(
                    delay,
                    stoppingToken);
                HostStatuses[host].IsWaiting = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                HostStatuses[host].IsWaiting = false;
                HostStatuses[host].InProgress = false;
                HostStatuses[host].FullUrl = null;
                HostStatuses[host].WaitingUntil = null;
            }
        }
    }
}