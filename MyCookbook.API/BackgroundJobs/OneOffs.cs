using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;
using static MyCookbook.API.Controllers.JobQueuerController;

namespace MyCookbook.API.BackgroundJobs;

public sealed class OneOffs(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory,
    IUrlProcessor urlProcessor,
    ILogger<JobRunner> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await using var db = await myCookbookContextFactory.CreateDbContextAsync(
                stoppingToken);
            var recipeUrls = await db.RecipeUrls
                .Where(
                    x =>
                        string.IsNullOrWhiteSpace(
                            x.Html))
                .ToListAsync(
                    stoppingToken);
            foreach (var recipeUrl in recipeUrls)
            {
                try
                {
                    logger.LogInformation(
                        $"Processing {recipeUrl.Uri}");
                    var web = new HtmlWeb();
                    var doc = await web.LoadFromWebAsync(
                        recipeUrl.Uri,
                        Encoding.Default,
                        new NetworkCredential(),
                        stoppingToken);
                    recipeUrl.Html = Regex.Replace(
                        doc.Text,
                        "\\s{2,}",
                        " ");
                    await db.SaveChangesAsync(
                        stoppingToken);
                    var delay = recipeUrl.StatusCode == HttpStatusCode.OK
                        ? TimeSpan.FromSeconds(
                            Random.Shared.Next(
                                3,
                                20))
                        : TimeSpan.FromMinutes(3);
                    logger.LogInformation(
                        $"Delaying for processor for {delay:g}");
                }
                catch (Exception e)
                {
                    recipeUrl.Exception = $"{e.Message}{Environment.NewLine}{e.StackTrace}";
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