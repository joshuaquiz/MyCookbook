using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using MyCookbook.Lambda.Interfaces;

namespace MyCookbook.Lambda.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;

    public S3Service(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task<string> DownloadFileAsStringAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = key
        };

        using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        using var reader = new StreamReader(response.ResponseStream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public async Task<string> DownloadFileToTempAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        
        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.DownloadAsync(tempFilePath, bucketName, key, cancellationToken);
        
        return tempFilePath;
    }

    public async Task UploadFileAsync(string bucketName, string key, string filePath, CancellationToken cancellationToken = default)
    {
        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(filePath, bucketName, key, cancellationToken);
    }

    public string GetPreSignedUrl(string bucketName, string key, TimeSpan expiration)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiration)
        };

        return _s3Client.GetPreSignedURL(request);
    }
}

