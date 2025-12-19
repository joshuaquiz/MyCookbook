using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCookbook.API.Interfaces;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public sealed class JobQueuerController(
    IDbContextFactory<MyCookbookContext> dbContextFactory,
    IJobQueuer jobQueuer)
    : ControllerBase
{
    [HttpGet("QueueUrl")]
    public async Task<OkResult> QueueUrl(
        Uri uri)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await jobQueuer.QueueUrlProcessingJob(
            db,
            uri);
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("GetOverallStatus")]
    public async Task<ActionResult> GetOverallStatus()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var data = await db.RawDataSources
            .GroupBy(
                x =>
                    x.ProcessingStatus)
            .ToDictionaryAsync(
                x => x.Key,
                x => x.Count());
        return Ok(
            data);
    }

    [HttpGet("GetHostStatuses")]
    public async Task<ActionResult> GetHostStatuses()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var data = await db.RawDataSources
            .GroupBy(x =>
                x.UrlHost)
            .ToDictionaryAsync(
                x => x.Key,
                x => x
                    .GroupBy(y =>
                        y.ProcessingStatus)
                    .ToDictionary(
                        y => y.Key,
                        y => y.Count()));
        return Ok(
            data);
    }

    [HttpGet("GetDomainBreakdown")]
    public async Task<List<UrlSegment>> GetDomainBreakdown()
    {
        return await GetUrlSegments();
    }

    private async Task<List<UrlSegment>> GetUrlSegments()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var urls = db.RawDataSources.Select(x => x.Url).ToList();
        var root = new UrlSegment
        {
            Segment = "/"
        };
        foreach (var uri in urls)
        {
            var segments = new List<string> { $"{uri.Scheme}://{uri.Host}/" };
            segments.AddRange(uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(segment => $"/{segment}"));

            AddUrlSegments(root, segments);
        }

        return [root];
    }

    private static void AddUrlSegments(UrlSegment parent, List<string> segments)
    {
        if (!segments.Any())
        {
            return;
        }

        var currentSegment = segments[0];
        var child = parent.Children.FirstOrDefault(c => c.Segment == currentSegment);
        if (child == null)
        {
            child = new UrlSegment { Segment = currentSegment };
            parent.Children.Add(child);
        }

        AddUrlSegments(child, segments.Skip(1).ToList());
    }
    public class UrlSegment
    {
        public string Segment { get; set; }

        public int Count => Children.Count;

        public List<UrlSegment> Children { get; set; } = [];
    }
}