using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCookbook.Common.ApiModels;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class CalendarController(
    IDbContextFactory<MyCookbookContext> dbContextFactory)
    : ControllerBase
{
    [HttpGet("Entries")]
    public async ValueTask<ActionResult<List<UserCalendarEntryModel>>> GetCalendarEntries(
        [FromQuery] Guid authorId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entries = await db.UserCalendars
            .Include(x => x.Recipe)
                .ThenInclude(x => x.EntityImages)
                .ThenInclude(x => x.Image)
            .Where(x => x.AuthorId == authorId && x.Date >= startDate && x.Date <= endDate)
            .Select(x => new UserCalendarEntryModel(
                x.Id,
                x.AuthorId,
                x.RecipeId,
                x.Recipe.Title,
                x.Recipe.EntityImages.Any(y => y.Image.ImageType == ImageType.Main)
                    ? x.Recipe.EntityImages.First(y => y.Image.ImageType == ImageType.Main).Image.Url
                    : null,
                x.Date,
                (int)x.MealType,
                x.MealType.ToString(),
                x.ServingsMultiplier))
            .ToListAsync(cancellationToken);

        return Ok(entries);
    }

    [HttpDelete("Entry/{id}")]
    public async ValueTask<ActionResult> DeleteCalendarEntry(
        Guid id,
        CancellationToken cancellationToken)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var entry = await db.UserCalendars.FindAsync([id], cancellationToken);
        if (entry == null)
        {
            return NotFound();
        }

        db.UserCalendars.Remove(entry);
        await db.SaveChangesAsync(cancellationToken);

        return Ok();
    }
}

