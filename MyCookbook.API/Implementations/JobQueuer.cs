using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Implementations;

public sealed class JobQueuer(
    ILogger<JobQueuer> logger)
    : IJobQueuer
{
    public async Task QueueUrlProcessingJob(
        MyCookbookContext db,
        Uri url)
    {
        if (!url.IsAbsoluteUri)
        {
            url = new Uri(
                "https:" + url,
                UriKind.Absolute);
        }

        if (await db.RawDataSources.AnyAsync(
                x => x.Url == url))
        {
            logger.LogInformation(
                $"{url} already processed or in the queue, not queueing again");
            return;
        }

        logger.LogInformation(
            $"Queueing {url}");
        await db.RawDataSources.AddAsync(
            new RawDataSource
            {
                SourceId = Guid.NewGuid(),
                ProcessingStatus = RecipeUrlStatus.NotStarted,
                Url = url,
                UrlHost = url.Host
            });
        await db.SaveChangesAsync();
    }
}