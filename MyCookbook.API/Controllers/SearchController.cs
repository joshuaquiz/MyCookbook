using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyCookbook.Common;
using Microsoft.AspNetCore.Mvc;

namespace MyCookbook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public sealed class SearchController : ControllerBase
{
    [HttpGet("Categories")]
    public async ValueTask<List<CategoryItem>> GetCategories(
        CancellationToken cancellationToken)
    {
        return
        [
            new(null, "#ffffff", "American"),
            new(null, "#ffffff", "Mexican"),
            new(null, "#ffffff", "Pizza"),
            new(null, "#ffffff", "BBQ")
        ];
    }

    [HttpGet("GlobalSearch")]
    public async ValueTask<SearchResults> GlobalSearch(
        [FromQuery] string category,
        [FromQuery] string searchTerm,
        CancellationToken cancellationToken)
    {
        return new SearchResults(
            [
                "chicken",
                "chicken liver",
                "chicken feet"
            ],
            [
                "chicken noodle soup",
                "chicken pot pie",
                "chicken fried"
            ]);
    }
}