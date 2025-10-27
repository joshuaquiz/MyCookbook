using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        var databasePath = DownloadDatabase(
                false,
                configuration,
                s3Client,
                new Progress<double>(),
                logger,
                CancellationToken.None)
            .GetAwaiter()
            .GetResult();
        return new SQLiteAsyncConnection(
            databasePath)
        {
            Trace = true,
            Tracer = x => logger.LogDebug(x)
        };
    }

    internal static async Task<string> DownloadDatabase(
        bool doUpgradeIfNeeded,
        IConfiguration configuration,
        IAmazonS3 s3Client,
        IProgress<double> progress,
        ILogger<SQLiteAsyncConnection> logger,
        CancellationToken cancellationToken)
    {
        var databasePath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "MyCookbook.db");
        var tempPath = databasePath + ".tmp.db";
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }

        var dbFile = new FileInfo(databasePath);
        if (!dbFile.Exists)
        {
            await s3Client.DownloadToFilePathAsync(
                configuration["S3DbBucket"],
                "FreshMyCookbook.db",
                databasePath,
                new Dictionary<string, object>(),
                cancellationToken);
        }

        if (doUpgradeIfNeeded && dbFile is { Exists: true, Length: < 2_000_000_000 })
        {
            var connection = new SQLiteConnection(
                databasePath)
            {
                Trace = true,
                Tracer = x => logger.LogDebug(x)
            };
            await CustomMultipartDownloadAsync(
                s3Client,
                configuration["S3DbBucket"],
                "MyCookbook.db",
                tempPath,
                progress,
                cancellationToken);
            HandleMigrations(
                connection,
                tempPath);
            MergeNewRows(
                connection,
                tempPath);
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }

        return databasePath;
    }

    private static async Task CustomMultipartDownloadAsync(
        IAmazonS3 s3Client,
        string bucketName,
        string key,
        string destinationFilePath,
        IProgress<double> progress,
        CancellationToken cancellationToken)
    {
        // --- Configuration ---
        const long partSize = 16 * 1024 * 1024; // 16 MB part size

        // 1. Get object metadata to find the total size
        var metadataRequest = new GetObjectMetadataRequest
        {
            BucketName = bucketName,
            Key = key
        };
        var metadataResponse = await s3Client.GetObjectMetadataAsync(metadataRequest, cancellationToken);
        var totalFileSize = metadataResponse.ContentLength;

        // --- Progress Tracking Variables ---
        var totalBytesTransferred = 0L;
        // Object used as a lock for thread-safe updates to totalBytesTransferred
        var progressLock = new object();
        progress.Report(0.0); // Report 0% start

        // 2. Calculate the number of parts
        var totalParts = (int)Math.Ceiling((double)totalFileSize / partSize);
        var downloadTasks = new List<Task>();

        // Used to reassemble the file in the correct order
        var partFilePaths = new string[totalParts];

        // Optional: Use a SemaphoreSlim to control concurrency
        const int maxConcurrentDownloads = 8;
        var semaphore = new SemaphoreSlim(maxConcurrentDownloads);

        for (var partNumber = 0; partNumber < totalParts; partNumber++)
        {
            var startByte = partNumber * partSize;
            var endByte = Math.Min(startByte + partSize - 1, totalFileSize - 1);
            var currentPartSize = endByte - startByte + 1; // Actual size of this part

            // Define a temporary path to save the part
            var tempFilePath = $"{destinationFilePath}.part{partNumber}";
            partFilePaths[partNumber] = tempFilePath;

            // Add the download task
            downloadTasks.Add(
                Task.Run(
                    async () =>
                    {
                        await semaphore.WaitAsync(cancellationToken); // Wait for a slot in the semaphore
                        try
                        {
                            // 3. Create a GetObject request with the specific byte range
                            var getObjectRequest = new GetObjectRequest
                            {
                                BucketName = bucketName,
                                Key = key,
                                ByteRange = new ByteRange(startByte, endByte)
                            };

                            // 4. Execute the download for this part
                            // Manual stream copy is required to get progress *within* the part
                            // The AWS SDK's WriteResponseStreamToFileAsync doesn't support IProgress
                            using var response = await s3Client.GetObjectAsync(getObjectRequest, cancellationToken);
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
                            semaphore.Release(); // Release the slot
                        }
                    }, cancellationToken));
        }

        // 5. Wait for all parts to complete
        await Task.WhenAll(downloadTasks);

        // 6. Reassemble the file
        // Progress reporting on the reassembly phase is omitted, as it's typically very fast
        await using (var finalStream = new FileStream(destinationFilePath, FileMode.Create))
        {
            foreach (var partPath in partFilePaths)
            {
                await using var partStream = new FileStream(partPath, FileMode.Open);
                await partStream.CopyToAsync(finalStream, cancellationToken);
            }
        }

        progress.Report(1.0); // Report 100% completion

        // 7. Clean up temporary files
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

    private static void HandleMigrations(
        SQLiteConnection connection,
        string tempDbPath)
    {
        // TODO: Implement migration logic here.
    }

    private static void MergeNewRows(
        SQLiteConnection connection,
        string tempDbPath)
    {
        var attachCommand = connection.CreateCommand(
            $"ATTACH DATABASE '{tempDbPath.Replace("'", "''")}' AS TempDB;");
        attachCommand.ExecuteNonQuery();

        List<string> tables = [
            "Users",
            "Ingredients",
            "Authors",
            "RecipeUrls",
            "Recipes",
            "RecipeSteps",
            "RecipeStepIngredients"
        ];
        connection.BeginTransaction();
        try
        {
            foreach (var insertCommand in tables
                         .Select(table =>
                             connection.CreateCommand(
                                 $"INSERT OR IGNORE INTO {table} SELECT * FROM TempDB.{table};"
                             )))
            {
                insertCommand.ExecuteNonQuery();
            }

            connection.Commit();
        }
        catch
        {
            connection.Rollback();
            throw;
        }

        var detachCommand = connection.CreateCommand("DETACH DATABASE TempDB;");
        detachCommand.ExecuteNonQuery();
    }
}