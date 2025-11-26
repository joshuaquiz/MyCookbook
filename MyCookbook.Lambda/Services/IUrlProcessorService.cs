namespace MyCookbook.Lambda.Services;

public interface IUrlProcessorService
{
    public Task ProcessUrlAsync(
        Uri url,
        CancellationToken cancellationToken);
}

