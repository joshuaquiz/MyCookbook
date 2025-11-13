using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;

namespace MyCookbook.API.Implementations;

public sealed partial class LdJsonExtractor(
    ILogger<LdJsonExtractor> logger)
    : ILdJsonExtractor
{
    [GeneratedRegex("[ \t\r\n]{2,}")]
    private static partial Regex CollapseWhitespaceRegex();

    public async ValueTask<LdJsonAndRawPageData> ExtractLdJsonItems(
        Uri url,
        string? html,
        CancellationToken cancellationToken)
    {
        try
        {
            HtmlDocument? doc;
            if (!string.IsNullOrWhiteSpace(
                    html))
            {
                doc = new HtmlDocument();
                doc.LoadHtml(
                    html);
            }
            else
            {
                var (htmlDoc, statusCode, exception) = await Helpers.HtmlDownloader.DownloadHtmlDocument(
                    url,
                    cancellationToken);
                doc = htmlDoc;
                if (exception != null)
                {
                    logger.LogError(
                        exception,
                        exception.Message);
                    return new LdJsonAndRawPageData(
                        statusCode,
                        string.Empty,
                        new List<string>());
                }

                if (doc == null)
                {
                    return new LdJsonAndRawPageData(
                        null,
                        string.Empty,
                        new List<string>());
                }
            }

            logger.LogDebug(
                $"{url} {doc.Text}");
            var jsonSections = doc.DocumentNode
                .SelectNodes("//script[@type='' or @type='application/ld+json']")
                ?.ToList()
                ?? [];
            var ldTextSections = new List<string>();
            foreach (var section in jsonSections)
            {
                if (section.InnerText.Contains("@context"))
                {
                    ldTextSections.Add(section.InnerText);
                }
                else
                {
                    section.Remove();
                }
            }

            foreach (var node in doc.DocumentNode.DescendantsAndSelf().Where(n => n.NodeType == HtmlNodeType.Comment)
                         .Concat(
                             doc.DocumentNode.SelectNodes("//script[@type='text/javascript']")?.Nodes() ?? [])
                         .Concat(
                             doc.DocumentNode.SelectNodes("//script[@type='text/x-config']")?.Nodes() ?? [])
                         .Concat(
                             doc.DocumentNode.SelectNodes("//template")?.Nodes() ?? [])
                         .Concat(
                             doc.DocumentNode.SelectNodes("//svg")?.Nodes() ?? [])
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
                         .ToList())
            {
                node.Remove();
            }

            return new LdJsonAndRawPageData(
                HttpStatusCode.OK,
                CollapseWhitespaceRegex()
                    .Replace(doc.DocumentNode.OuterHtml, " "),
                ldTextSections);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                e.Message);
            return new LdJsonAndRawPageData(
                null,
                string.Empty,
                new List<string>());
        }
    }
}