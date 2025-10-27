using System;
using System.Threading.Tasks;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Interfaces;

public interface IJobQueuer
{
    public Task QueueUrlProcessingJob(
        MyCookbookContext db,
        Uri url);
}