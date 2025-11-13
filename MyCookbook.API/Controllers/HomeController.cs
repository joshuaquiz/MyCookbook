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
                .Include(x => x.EntityImages).ThenInclude(x => x.Image)
                .Include(x => x.Author).ThenInclude(x => x.EntityImages).ThenInclude(x => x.Image)
                .Select(x =>
                    new PopularItem(
                        x.RecipeId,
                        x.EntityImages.Any(y => y.Image.ImageType == ImageType.Main)
                            ? x.EntityImages.First(y => y.Image.ImageType == ImageType.Main).Image.Url
                            : null,
                        x.Title,
                        x.Author.EntityImages.Any(y => y.Image.ImageType == ImageType.Main)
                            ? x.Author.EntityImages.First(y => y.Image.ImageType == ImageType.Main).Image.Url
                            : null,
                        x.Author.Name,
                        TimeSpan.FromMinutes(x.PrepTimeMinutes ?? 0 + x.CookTimeMinutes ?? 0), 
                        x.RawDataSource.Url))
                .ToListAsync(
                    cancellationToken));
    }
}