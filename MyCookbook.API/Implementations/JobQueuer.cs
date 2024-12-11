using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;

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

        if (await db.RecipeUrls.AnyAsync(
                x => x.Uri == url))
        {
            logger.LogInformation(
                $"{url} already processed or in the queue, not queueing again");
            return;
        }

        logger.LogInformation(
            $"Queueing {url}");
        await db.RecipeUrls.AddAsync(
            new RecipeUrl
            {
                Guid = Guid.NewGuid(),
                ProcessingStatus = RecipeUrlStatus.NotStarted,
                Uri = url,
                Host = url.Host
            });
        await db.SaveChangesAsync();
    }
}