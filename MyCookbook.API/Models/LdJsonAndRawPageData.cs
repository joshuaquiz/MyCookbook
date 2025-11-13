using System.Collections.Generic;
using System.Net;

namespace MyCookbook.API.Models;

public record LdJsonAndRawPageData(
    HttpStatusCode? HttpStatus,
    string RawHtml,
    IReadOnlyList<string> Data);