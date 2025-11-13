using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;
using Schema.NET;

namespace MyCookbook.API.Implementations.SiteParsers;

public sealed partial class FoodNetworkNormalizer
    : ISiteNormalizer
{
    [GeneratedRegex(@"\.rend\.hgtvcom\.\d+.\d+.suffix/\d+\.(?:webp|jpeg|jpg|gif|png)")]
    private static partial Regex ResizeUrlData();

    public IEnumerable<Uri> GetUrlsToQueue(
        SiteWrapper wrapper) =>
        [];

    public Uri? NormalizeImageUrl(
        string? uri)
    {
        if (uri == null)
        {
            return null;
        }

        try
        {
            return new Uri(
                ResizeUrlData()
                    .Replace(
                        uri,
                        string.Empty),
                UriKind.Absolute);
        }
        catch
        {
            return null;
        }
    }

    public async ValueTask<SiteWrapper> NormalizeSite(
        SiteWrapper wrapper)
    {
        if (wrapper.Recipes != null)
        {
            return await NormalizeRecipe(
                wrapper);
        }

        if (wrapper.ProfilePages != null)
        {
            return await NormalizeProfilePage(
                wrapper);
        }

        return wrapper;
    }

    private ValueTask<SiteWrapper> NormalizeRecipe(
        SiteWrapper wrapper)
    {
        if (wrapper.Recipes is { Count: 1 })
        {
            if (!wrapper.Recipes[0].Image.HasValue)
            {
                var extractedUrl = NormalizeImageUrl(
                    "https:" + wrapper.HtmlDocument.DocumentNode
                        .SelectNodes("//div[@class='recipeLead']//img")
                        .FirstOrDefault()
                        ?.Attributes
                        .FirstOrDefault(x =>
                            x.Name == "src")
                        ?.Value);
                if (extractedUrl != null)
                {
                    wrapper.Recipes[0].Image = new Values<IImageObject, Uri>(extractedUrl);
                }
            }

            if (!wrapper.Recipes[0].Video.HasValue)
            {
                var extractedUrl = NormalizeImageUrl(
                    "https:" + wrapper.HtmlDocument.DocumentNode
                        .SelectNodes("//div[@class='recipeLead']//img")
                        .FirstOrDefault()
                        ?.Attributes
                        .FirstOrDefault(x =>
                            x.Name == "src")
                        ?.Value);
                if (extractedUrl != null)
                {
                    wrapper.Recipes[0].Video = new Values<IClip, IVideoObject>(
                        new VideoObject
                        {
                            Url = extractedUrl
                        });
                }
            }
        }

        return ValueTask.FromResult(wrapper);
    }

    private ValueTask<SiteWrapper> NormalizeProfilePage(
        SiteWrapper wrapper)
    {
        if (wrapper.ProfilePages is { Count: 1 })
        {
            if (!wrapper.ProfilePages[0].Image.HasValue)
            {
                var images = new List<Uri>();
                var mainImage = NormalizeImageUrl(
                    wrapper.HtmlDocument.DocumentNode
                        .SelectNodes(
                            "//meta[@property='og:image']")
                        .FirstOrDefault()
                        ?.Attributes
                        .FirstOrDefault(x =>
                            x.Name == "content")
                        ?.Value);
                if (mainImage != null)
                {
                    images.Add(mainImage);
                }

                var backgroundImage = NormalizeImageUrl(
                    wrapper.HtmlDocument.DocumentNode
                        .SelectNodes(
                            "//div[@class='m-Carousel__m-Slide']//img[@class='m-MediaBlock__a-Image']")
                        .FirstOrDefault()
                        ?.Attributes
                        .FirstOrDefault(x =>
                            x.Name == "src")
                        ?.Value);
                if (backgroundImage != null)
                {
                    images.Add(backgroundImage);
                }

                if (images.Count > 0)
                {
                    wrapper.ProfilePages[0].MainEntity.First().Image = new Values<IImageObject, Uri>(images);
                }
            }

            if (wrapper.ProfilePages[0].MainEntity.Count == 1)
            {
                if (string.IsNullOrWhiteSpace(wrapper.ProfilePages[0].MainEntity.First().Description))
                {
                    wrapper.ProfilePages[0].MainEntity.First().Description = WebUtility.HtmlDecode(
                        wrapper.HtmlDocument.DocumentNode
                            .SelectNodes(
                                "//meta[@name='description']")
                            .FirstOrDefault()
                            ?.Attributes
                            .FirstOrDefault(x =>
                                x.Name == "content")
                            ?.Value
                    ) ?? string.Empty;
                }
            }
        }

        return ValueTask.FromResult(wrapper);
    }
}