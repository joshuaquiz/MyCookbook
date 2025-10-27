using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public sealed class HomeController(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory)
    : ControllerBase
{
    [HttpGet("Popular")]
    public async ValueTask<ActionResult<List<PopularItem>>> GetPopular(
        CancellationToken cancellationToken)
    {
        await using var db = await myCookbookContextFactory.CreateDbContextAsync(
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