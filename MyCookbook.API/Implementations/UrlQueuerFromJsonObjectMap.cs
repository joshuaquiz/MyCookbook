using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;
using Schema.NET;

namespace MyCookbook.API.Implementations;

public sealed class UrlQueuerFromJsonObjectMap
    : IUrlQueuerFromJsonObjectMap
{
    public IReadOnlyCollection<Uri> QueueUrlsFromJsonObjectMap(
        IReadOnlyDictionary<string, JsonObject> jsonObjects)
    {
        var urls = new List<Uri>();
        if (jsonObjects.TryGetValue(
                nameof(ItemList),
                out var listItem))
        {
            var itemListElement = JsonSerializer.Deserialize<ItemList>(
                    listItem.ToString())
                ?.ItemListElement;
            urls.AddRange(
                itemListElement
                    ?.Where(x => x is ListItem)
                    .Cast<ListItem>()
                    .SelectMany(x => x.Url)
                    .ToArray()
                ?? []);
            urls.AddRange(
                itemListElement
                    ?.Where(x => x is SiteNavigationElement)
                    .Cast<SiteNavigationElement>()
                    .SelectMany(x => x.Url)
                    .ToArray()
                ?? []);
        }

        if (jsonObjects.TryGetValue(
                nameof(BreadcrumbList),
                out var breadcrumbList))
        {
            urls.AddRange(
                breadcrumbList.Deserialize<CustomBreadcrumbList>()
                    ?.ItemListElement
                    .Skip(1)
                    .Cast<CustomItemListElement>()
                    .Select(x => x.Item?.Id)
                    .Where(x => x != null)
                    .Select(x => new Uri(x!, UriKind.Absolute))
                    .ToArray()
                ?? []);
        }

        return urls;
    }
}