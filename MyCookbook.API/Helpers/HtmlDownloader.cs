using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace MyCookbook.API.Helpers;

public static class HtmlDownloader
{
    public static async ValueTask<(HtmlDocument? Html, HttpStatusCode? StatusCode, Exception? Exception)> DownloadHtmlDocument(
        Uri url,
        CancellationToken cancellationToken)
    {
        try
        {
            var web = new HtmlWeb();
            var htmlDocument = await web.LoadFromWebAsync(
                url,
                Encoding.Default,
                new NetworkCredential(),
                cancellationToken);
            return (Html: htmlDocument, StatusCode: HttpStatusCode.OK, Exception: null);
        }
        catch (HttpRequestException e)
        {
            return (Html: null, e.StatusCode, Exception: e);
        }
        catch (Exception e)
        {
            return (Html: null, null, Exception: e);
        }
    }
}