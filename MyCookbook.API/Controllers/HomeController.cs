using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using MyCookbook.Common;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace MyCookbook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public sealed class HomeController : ControllerBase
{
    private readonly IDbContextFactory<MyCookbookContext> _myCookbookContextFactory;

    public HomeController(
        IDbContextFactory<MyCookbookContext> myCookbookContextFactory)
    {
        _myCookbookContextFactory = myCookbookContextFactory;
    }

    [HttpGet("Popular")]
    public async ValueTask<ActionResult<List<PopularItem>>> GetPopular(
        CancellationToken cancellationToken)
    {
        await using var db = await _myCookbookContextFactory.CreateDbContextAsync(
            cancellationToken);
        return new JsonResult(
            await db.Recipes
                .Select(x =>
                    new PopularItem(
                        x.Guid,
                        x.Image,
                        x.Name,
                        x.Author.Image,
                        x.Author.Name,
                        x.TotalTime,
                        x.Image ?? new Uri("http://www.google.com")))
                .ToListAsync(
                    cancellationToken));
    }
}