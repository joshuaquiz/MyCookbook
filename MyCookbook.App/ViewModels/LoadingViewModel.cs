using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyCookbook.App.Helpers;
using MyCookbook.App.Interfaces;
using SQLite;

namespace MyCookbook.App.ViewModels;

public partial class LoadingViewModel(
    ICookbookStorage cookbookStorage,
    IConfiguration configuration,
    IAmazonS3 s3Client,
    ILogger<SQLiteAsyncConnection> logger)
    : ObservableObject
{
    [ObservableProperty]
    private double _loadingProgress;

    [ObservableProperty]
    private string _loadingMessage = "Initializing...";

    public async Task<bool> InitializeAppAsync()
    {
        try
        {
            LoadingProgress = 0;
            LoadingMessage = "Loading user data...";
            var user = await cookbookStorage.GetUser();
            LoadingMessage = "Downloading data...";
            var downloadProgress = new Progress<double>(percentage =>
            {
                if (LoadingProgress is >= 0 and < 1)
                {
                    if (percentage == 0 || LoadingProgress < percentage)
                    {
                        LoadingProgress = percentage;
                    }
                }
                else
                {
                    LoadingProgress = .99;
                }
            });
            await DatabaseSetupHelper.DownloadDatabase(
                configuration,
                s3Client,
                downloadProgress,
                logger,
                CancellationToken.None);
            LoadingProgress = 1;
            return user != null;
        }
        catch (Exception ex)
        {
            LoadingMessage = $"Error: {ex.Message}";
            throw;
        }
    }
}

