using System;
using System.Net.Http;
using System.Reflection;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.S3;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using G3.Maui.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using MyCookbook.App.Helpers;
using MyCookbook.App.Implementations;
using MyCookbook.App.Interfaces;
using MyCookbook.App.ViewModels;
using MyCookbook.App.Views;
using MyCookbook.App.Views.Home;
using MyCookbook.App.Views.MyCookbook;
using MyCookbook.App.Views.Profile;
using MyCookbook.App.Views.Search;
using SQLite;
//using Plugin.MauiMTAdmob;

namespace MyCookbook.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.Configuration.AddUserSecrets<App>();
        var a = Assembly.GetExecutingAssembly();
        using var stream = a.GetManifestResourceStream($"{a.GetName().Name}.appsettings.json");
        if (stream != null)
        {
            builder.Configuration.AddJsonStream(stream);
        }

        using var localStream = a.GetManifestResourceStream($"{a.GetName().Name}.appsettings.local.json");
        if (localStream != null)
        {
            builder.Configuration.AddJsonStream(localStream);
        }

        var awsConfig = builder.Configuration.GetSection("AWS");
        var accessKey = awsConfig["AccessKey"];
        var secretKey = awsConfig["SecretKey"];
        var region = awsConfig["Region"];
        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("AWS credentials were not properly loaded from configuration.");
        }

        builder.Services.AddSingleton(new BasicAWSCredentials(accessKey, secretKey));
        builder.Services.AddSingleton(
            s =>
                new AWSOptions
                {
                    Credentials = s.GetRequiredService<BasicAWSCredentials>(),
                    Region = Amazon.RegionEndpoint.GetBySystemName(region)
                });
        builder.Services.AddSingleton<IAmazonS3, AmazonS3Client>(
            s =>
                new AmazonS3Client(
                    s.GetRequiredService<BasicAWSCredentials>(),
                    Amazon.RegionEndpoint.GetBySystemName(region)));

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitCore()
            //.UseMauiMTAdmob()
            .ConfigureFonts(
                fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
        builder.Services
            .AddCoreDeviceServices();
#if DEBUG
        builder.Services
            .AddLogging(
                x =>
                    x.SetMinimumLevel(LogLevel.Trace)
                        .AddConsole())
            .AddSingleton(
                s =>
                    DatabaseSetupHelper.SetupDatabaseConnection(
                            s.GetRequiredService<IConfiguration>(),
                            s.GetRequiredService<IAmazonS3>(),
                            s.GetRequiredService<ILogger<SQLiteAsyncConnection>>()))
            .AddDevelopmentHttpClient<CookbookDelegatingHandler, CookbookHttpClient>(
                (serviceProvider, baseDelegatingHandler, baseUri) =>
                    new CookbookHttpClient(
                        serviceProvider.GetRequiredService<IConnectivity>(),
                        new HttpClient(
                            baseDelegatingHandler)
                        {
                            BaseAddress = baseUri
                        },
                        serviceProvider.GetRequiredService<IMemoryCache>(),
                        serviceProvider.GetRequiredService<ILogger<CookbookHttpClient>>()));
#else
        builder.Services.AddSingleton(
            new HttpClient()
            {
                BaseAddress = new Uri(
                    "https://lol.fake.com",
                    UriKind.Absolute)
            });
#endif
        builder.Services
            .AddMemoryCache()
            .AddSingleton(SecureStorage.Default)
            .AddSingleton(Preferences.Default)
            .AddSingleton(Connectivity.Current);

        builder.Services
            .AddSingleton<ICookbookStorage, CookbookStorage>();

        builder.Services
            .AddSingleton<LoadingViewModel>()
            .AddSingleton<LoginViewModel>()
            .AddTransient<ProfileViewModel>()
            .AddTransient<RecipeViewModel>()
            .AddTransient<ShoppingListViewModel>()
            .AddSingleton<MyCookbookViewModel>();

        builder.Services
            .AddSingleton<LoadingScreen>()
            .AddSingleton<Login>()
            .AddSingleton<HomePage>()
            .AddTransient<ProfileHome>()
            .AddTransient<RecipePage>()
            .AddTransient<ShoppingListHome>()
            .AddSingleton<MyCookbookHome>()
            .AddTransient<SearchHome>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}