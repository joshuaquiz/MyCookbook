using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SQLite;

namespace MyCookbook.App.Helpers;

internal sealed class DatabaseSetupHelper
{
    internal static async Task<SQLiteAsyncConnection> GetDatabaseConnection(
        IConfiguration configuration,
        IAmazonS3 s3Client,
        ILogger<SQLiteAsyncConnection> logger)
    {
        using var transferUtility = new TransferUtility(s3Client);
        var databasePath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "MyCookbook.db");
        var tempPath = databasePath + ".tmp.db";
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }

        var dbAlreadyExists = File.Exists(databasePath);
        if (!dbAlreadyExists)
        {
            await transferUtility.DownloadAsync(
                configuration["S3DbBucket"],
                "FreshMyCookbook.db",
                databasePath);
        }

        var connection = new SQLiteAsyncConnection(
            databasePath)
        {
            Trace = true,
            Tracer = x => logger.LogDebug(x)
        };
        if (dbAlreadyExists)
        {
            await transferUtility.DownloadAsync(
                configuration["S3DbBucket"],
                "MyCookbook.db",
                tempPath);
            await HandleMigrations(
                connection,
                tempPath);
            await MergeNewRows(
                connection,
                tempPath);
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }

        return connection;
    }

    private static async Task HandleMigrations(
        SQLiteAsyncConnection connection,
        string sourceDbPath)
    {
        // TODO: Implement migration logic here.
        await Task.CompletedTask;
    }

    private static async Task MergeNewRows(
        SQLiteAsyncConnection connection,
        string sourceDbPath)
    {
        await connection.ExecuteAsync($"ATTACH DATABASE '{sourceDbPath}' AS SourceDB;");
        var tables = await connection.ExecuteScalarAsync<string>("SELECT name FROM SourceDB.sqlite_master WHERE type = 'table' and name not like '%EFM%' ORDER BY name");
        foreach (var tableName in tables)
        {
            await connection.ExecuteAsync($"INSERT OR IGNORE INTO {tableName} SELECT * FROM SourceDB.{tableName};");
        }

        await connection.ExecuteAsync("DETACH DATABASE SourceDB;");
    }
}