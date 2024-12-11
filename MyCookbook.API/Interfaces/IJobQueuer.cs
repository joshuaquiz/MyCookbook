using System;
using System.Threading.Tasks;

namespace MyCookbook.API.Interfaces;

public interface IJobQueuer
{
    public Task QueueUrlProcessingJob(
        MyCookbookContext db,
        Uri url);
}