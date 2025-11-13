using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;
using Schema.NET;

namespace MyCookbook.API.Implementations;

public sealed class UrlQueuerFromJsonObjectMap
    : IUrlQueuerFromJsonObjectMap
{
    public IEnumerable<Uri> QueueUrlsFromJsonObjectMap(
        IReadOnlyDictionary<string, List<JsonObject>> jsonObjects)
    {
        if (jsonObjects.TryGetValue(
                nameof(ItemList),
                out var listItems))
        {
            foreach (var listItem in listItems)
            {
                var itemListElement = JsonSerializer.Deserialize<ItemList>(
                        listItem.ToString())
                    ?.ItemListElement;
                foreach (var uri in (itemListElement
                             ?.Where(x => x is ListItem)
                             .Cast<ListItem>()
                             .SelectMany(x => x.Url) ?? [])
                         .Concat(
                             itemListElement
                                 ?.Where(x => x is SiteNavigationElement)
                                 .Cast<SiteNavigationElement>()
                                 .SelectMany(x => x.Url) ?? []))
                {
                    yield return uri;
                }
            }
        }

        if (jsonObjects.TryGetValue(
                nameof(BreadcrumbList),
                out var breadcrumbLists))
        {
            foreach (var uri in breadcrumbLists
                         .SelectMany(
                             b =>
                                 b.Deserialize<CustomBreadcrumbList>()
                                     ?.ItemListElement
                                     .Skip(1)
                                     .Cast<CustomItemListElement>()
                                     .Select(x => x.Item?.Id)
                                     .Where(x => x != null)
                                     .Select(x => new Uri(x!, UriKind.Absolute))
                                 ?? []))
            {
                yield return uri;
            }
        }
    }
}