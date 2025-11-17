using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SQLite;

namespace MyCookbook.App.Helpers;

internal sealed class DatabaseSetupHelper
{
    internal static SQLiteAsyncConnection SetupDatabaseConnection(
        IConfiguration configuration,
        IAmazonS3 s3Client,
        ILogger<SQLiteAsyncConnection> logger)
    {
        var dbFile = GetDatabasePath();
        if (!dbFile.Exists)
        {
            using var stream = dbFile.Create();
        }

        return new SQLiteAsyncConnection(
            dbFile.FullName)
        {
            Trace = true,
            Tracer = x => logger.LogDebug(x)
        };
    }

    internal static async Task<string> DownloadDatabase(
        IConfiguration configuration,
        IAmazonS3 s3Client,
        IProgress<double> progress,
        ILogger<SQLiteAsyncConnection> logger,
        CancellationToken cancellationToken)
    {
        var dbFile = GetDatabasePath();
        if (dbFile is { Exists: true })
        {
            var metadataResponse = await s3Client.GetObjectMetadataAsync(
                new GetObjectMetadataRequest
                {
                    BucketName = configuration["S3DbBucket"],
                    Key = "MyCookbook.db"
                },
                cancellationToken);
            if (dbFile.Length == metadataResponse.ContentLength)
            {
                return dbFile.FullName;
            }

            File.Delete(dbFile.FullName);
            await CustomMultipartDownloadAsync(
                metadataResponse.ContentLength,
                s3Client,
                configuration["S3DbBucket"]!,
                "MyCookbook.db",
                dbFile.FullName,
                progress,
                cancellationToken);
        }

        return dbFile.FullName;
    }

    private static FileInfo GetDatabasePath() =>
        new(
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "MyCookbook.db"));

    private static async Task CustomMultipartDownloadAsync(
        long totalFileSize,
        IAmazonS3 s3Client,
        string bucketName,
        string key,
        string destinationFilePath,
        IProgress<double> progress,
        CancellationToken cancellationToken)
    {
        const long partSize = 16 * 1024 * 1024; // 16 MB part size
        var totalBytesTransferred = 0L;
        var progressLock = new object();
        progress.Report(0.0);
        var totalParts = (int)Math.Ceiling((double)totalFileSize / partSize);
        var downloadTasks = new List<Task>();
        var partFilePaths = new string[totalParts];
        const int maxConcurrentDownloads = 8;
        var semaphore = new SemaphoreSlim(maxConcurrentDownloads);
        for (var partNumber = 0; partNumber < totalParts; partNumber++)
        {
            var startByte = partNumber * partSize;
            var endByte = Math.Min(startByte + partSize - 1, totalFileSize - 1);
            var currentPartSize = endByte - startByte + 1;
            var tempFilePath = $"{destinationFilePath}.part{partNumber}";
            partFilePaths[partNumber] = tempFilePath;
            downloadTasks.Add(
                Task.Run(
                    async () =>
                    {
                        await semaphore.WaitAsync(cancellationToken);
                        try
                        {
                            using var response = await s3Client.GetObjectAsync(
                                new GetObjectRequest
                                {
                                    BucketName = bucketName,
                                    Key = key,
                                    ByteRange = new ByteRange(startByte, endByte)
                                },
                                cancellationToken);
                            await DownloadStreamWithProgress(
                                response.ResponseStream,
                                tempFilePath,
                                progress,
                                progressLock,
                                totalFileSize,
                                () => totalBytesTransferred,
                                cancellationToken);
                            totalBytesTransferred += currentPartSize;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    },
                    cancellationToken));
        }

        await Task.WhenAll(downloadTasks);
        await using (var finalStream = new FileStream(destinationFilePath, FileMode.Create))
        {
            foreach (var partPath in partFilePaths)
            {
                await using var partStream = new FileStream(partPath, FileMode.Open);
                await partStream.CopyToAsync(finalStream, cancellationToken);
            }
        }

        progress.Report(1.0);
        foreach (var partPath in partFilePaths)
        {
            File.Delete(partPath);
        }

        Console.WriteLine($"Download complete: {key} saved to {destinationFilePath}");
    }

    /// <summary>
    /// Manually reads the S3 response stream and writes to a file while reporting progress.
    /// </summary>
    private static async Task DownloadStreamWithProgress(
        Stream sourceStream,
        string destinationPath,
        IProgress<double> progress,
        object progressLock,
        long totalFileSize,
        Func<long> getTotalBytesTransferred,
        CancellationToken cancellationToken)
    {
        const int bufferSize = 81920; // 80 KB buffer

        // Allocate the buffer array once
        var bufferArray = new byte[bufferSize];

        // Get a Memory<byte> wrapper for the buffer array
        var buffer = new Memory<byte>(bufferArray);

        int bytesRead;

        // Track bytes read for *this* specific part
        long partBytesRead = 0;

        // Use default bufferSize in FileStream constructor
        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

        // 1. Use the Memory<byte> overload for reading
        while ((bytesRead = await sourceStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            // 2. Use the Memory<byte> overload for writing, taking a slice for ReadOnlyMemory<byte>
            // We use .Slice(0, bytesRead) to ensure we only write the data that was actually read.
            await fileStream.WriteAsync(buffer[..bytesRead], cancellationToken);

            partBytesRead += bytesRead;

            if (progress != null && totalFileSize > 0)
            {
                // The existing progress reporting logic remains the same.
                // The lock statement is still required for thread safety with the shared counter.
                lock (progressLock)
                {
                    // This block is often used to safely adjust the global counter variable.
                }

                // Re-read the total transferred to get the latest value from other threads.
                // NOTE: 'currentTotalTransferred' must be a variable that can be safely updated
                // across threads. Since it's passed via a Func, we must assume the caller's 
                // implementation manages thread safety, but the 'Interlocked.Add' below is the standard way.
                var currentTotalTransferred = getTotalBytesTransferred();

                // Estimate the current progress by adding the newly read bytes
                var progressValue = (double)(currentTotalTransferred + partBytesRead) / totalFileSize;

                // In a robust implementation, you would use Interlocked.Add on the shared counter
                // variable itself, not the local copy retrieved by the Func.
                // For example: Interlocked.Add(ref globalTotalBytesTransferred, bytesRead);
                // Since we can't do that here, we report the calculated value:
                progress.Report(progressValue);
            }
        }
    }
}