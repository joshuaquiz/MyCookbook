using System.Net.Http;
using System.Reflection;
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
        var a = Assembly.GetExecutingAssembly();
        using var stream = a.GetManifestResourceStream($"{a.GetName().Name}.appsettings.json");
        if (stream != null)
        {
            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();
            builder.Configuration.AddConfiguration(config);
        }

        builder.Configuration.AddUserSecrets<App>();
        builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
        builder.Services.AddAWSService<IAmazonS3>();

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
                    DatabaseSetupHelper.GetDatabaseConnection(
                            s.GetRequiredService<IConfiguration>(),
                            s.GetRequiredService<IAmazonS3>(),
                            s.GetRequiredService<ILogger<SQLiteAsyncConnection>>())
                        .GetAwaiter()
                        .GetResult())
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
            .AddSingleton<LoginViewModel>()
            .AddTransient<ProfileViewModel>()
            .AddTransient<RecipeViewModel>()
            .AddTransient<ShoppingListViewModel>()
            .AddSingleton<MyCookbookViewModel>();

        builder.Services
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