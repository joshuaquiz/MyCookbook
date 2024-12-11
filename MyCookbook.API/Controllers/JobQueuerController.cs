using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCookbook.API.BackgroundJobs;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Models;

namespace MyCookbook.API.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public sealed class JobQueuerController : ControllerBase
{
    private readonly IDbContextFactory<MyCookbookContext> _dbContextFactory;
    private readonly IJobQueuer _jobQueuer;

    public JobQueuerController(
        IDbContextFactory<MyCookbookContext> dbContextFactory,
        IJobQueuer jobQueuer)
    {
        _dbContextFactory = dbContextFactory;
        _jobQueuer = jobQueuer;
    }

    [HttpGet("QueueUrl")]
    public async Task<OkResult> QueueUrl(
        Uri uri)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        await _jobQueuer.QueueUrlProcessingJob(
            db,
            uri);
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("GetOverallStatus")]
    public async Task<ActionResult> GetOverallStatus()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var data = await db.RecipeUrls
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
    public async Task<IDictionary<string, HostStatus>> GetHostStatuses()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return JobRunner.HostStatuses;
    }

    [HttpGet("GetDomainBreakdown")]
    public async Task<List<UrlSegment>> GetDomainBreakdown()
    {
        return await GetUrlSegments();
    }

    private async Task<List<UrlSegment>> GetUrlSegments()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var urls = db.RecipeUrls.Select(x => x.Uri).ToList();

        var root = new UrlSegment { Segment = "/" };

        foreach (var uri in urls)
        {
            var segments = new List<string> { $"{uri.Scheme}://{uri.Host}/" };
            segments.AddRange(uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(segment => $"/{segment}"));

            AddUrlSegments(root, segments);
        }

        return [root];
    }

    private void AddUrlSegments(UrlSegment parent, List<string> segments)
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

    public class RecipeUrl
    {
        public int Id { get; set; }
        public string Uri { get; set; }
    }
}