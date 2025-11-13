using System;
using System.Collections.Generic;
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
using MyCookbook.Common.Database;

namespace MyCookbook.API.BackgroundJobs;

public sealed partial class OneOffs(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory,
    ILogger<UrlDownloaderJob> logger)
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
            var recipeUrls = await db.RawDataSources
                .Where(
                    x =>
                        !string.IsNullOrWhiteSpace(
                            x.RawHtml))
                .ToListAsync(
                    stoppingToken);
            foreach (var recipeUrl in recipeUrls)
            {
                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(
                        recipeUrl.RawHtml!);
                    foreach (var node in doc.DocumentNode.DescendantsAndSelf().Where(n => n.NodeType == HtmlNodeType.Comment)
                                 .Concat(
                                     doc.DocumentNode.SelectNodes("//script[@type='text/x-config']")?.Nodes() ?? [])
                                 .Concat(
                                     doc.DocumentNode.SelectNodes("//noscript")?.Nodes() ?? [])
                                 .Concat(
                                     doc.DocumentNode.SelectNodes("//iframe")?.Nodes() ?? [])
                                 .Concat(
                                     doc.DocumentNode.SelectNodes("//form")?.Nodes() ?? [])
                                 .Concat(
                                     doc.DocumentNode.SelectNodes("//style")?.Nodes() ?? [])
                                 .Concat(
                                     doc.DocumentNode.SelectNodes("//ins")?.Nodes() ?? [])
                                 .Concat(
                                     doc.DocumentNode.SelectNodes("//amp-ad")?.Nodes() ?? [])
                                 .Concat(
                                     doc.DocumentNode.SelectNodes("//script[@type='text/javascript']")?.Nodes() ?? [])
                                 .ToList())
                    {
                        node.Remove();
                    }

                    recipeUrl.RawHtml = CollapseWhitespaceRegex().Replace(doc.DocumentNode.OuterHtml, " ");
                    recipeUrl.Error = null;
                    await db.SaveChangesAsync(
                        stoppingToken);
                }
                catch (Exception e)
                {
                    recipeUrl.Error = $"{e.Message}{Environment.NewLine}{e.StackTrace}";
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