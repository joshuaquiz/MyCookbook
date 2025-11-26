namespace MyCookbook.Lambda.Interfaces;

public interface IS3Service
{
    Task<string> DownloadFileAsStringAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task<string> DownloadFileToTempAsync(string bucketName, string key, CancellationToken cancellationToken = default);
    Task UploadFileAsync(string bucketName, string key, string filePath, CancellationToken cancellationToken = default);
    string GetPreSignedUrl(string bucketName, string key, TimeSpan expiration);
}

