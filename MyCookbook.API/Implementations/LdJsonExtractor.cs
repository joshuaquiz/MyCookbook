using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;

namespace MyCookbook.API.Implementations;

public sealed class LdJsonExtractor(
    ILogger<LdJsonExtractor> logger)
    : ILdJsonExtractor
{
    public async ValueTask<LdJsonAndRawPageData> ExtractLdJsonItems(
        Uri url,
        string? html,
        bool isReprocessing,
        CancellationToken cancellationToken)
    {
        try
        {
            HtmlDocument? doc;
            if (isReprocessing
                && !string.IsNullOrWhiteSpace(
                    html))
            {
                doc = new HtmlDocument();
                doc.LoadHtml(
                    html);
            }
            else
            {
                var web = new HtmlWeb();
                doc = await web.LoadFromWebAsync(
                    url,
                    Encoding.Default,
                    new NetworkCredential(),
                    cancellationToken);
                if (isReprocessing)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(10),
                        cancellationToken);
                }
            }

            logger.LogDebug(
                $"{url} {doc.Text}");
            var jsonSections = doc.DocumentNode
                .SelectNodes("//script[@type='' or @type='application/ld+json']")
                .Select(n => n.InnerText)
                .ToList();
            string? imageSrc = null;
            if (url.ToString().Contains("foodnetwork")
                || url.ToString().Contains("cookingchanneltv"))
            {
                imageSrc = "https:" + doc.DocumentNode
                    .SelectNodes("//div[@class='recipeLead']//img")
                    ?.FirstOrDefault()
                    ?.Attributes
                    .FirstOrDefault(
                        x =>
                            x.Name == "src")
                    ?.Value;
            }

            return new LdJsonAndRawPageData(
                HttpStatusCode.OK,
                Regex.Replace(
                    doc.Text,
                    "\\s{2,}",
                    " "),
                jsonSections,
                imageSrc,
                null);
        }
        catch (HttpRequestException e)
        {
            logger.LogError(
                e,
                e.Message);
            return new LdJsonAndRawPageData(
                e.StatusCode,
                string.Empty,
                new List<string>(),
                null,
                null);
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                e.Message);
            return new LdJsonAndRawPageData(
                null,
                string.Empty,
                new List<string>(),
                null,
                null);
        }
    }
}