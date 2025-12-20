using System;
using System.Net.Http;
using System.Reflection;
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
using MyCookbook.App.Implementations;
using MyCookbook.App.Interfaces;
using MyCookbook.App.Services;
using MyCookbook.App.ViewModels;
using MyCookbook.App.Views;
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
                    fonts.AddFont("Lexend-VariableFont_wght.ttf", "Lexend");
                });
        builder.Services
            .AddCoreDeviceServices();

        // Configure HTTP client with authentication
        builder.Services
            .AddLogging(
                x =>
                    x.SetMinimumLevel(LogLevel.Debug)
                        .AddConsole())
            .AddSingleton(new HttpClient())
            .AddSingleton<ICognitoAuthService, CognitoAuthService>()
            .AddSingleton<CookbookHttpClient>(
                serviceProvider =>
                {
                    var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://api-development-mycookbook.g3software.net";

                    // Create platform-specific inner handler
                    HttpMessageHandler innerHandler;
#if ANDROID
                    // Use SocketsHttpHandler for Android to support HTTP (not just HTTPS)
                    innerHandler = new SocketsHttpHandler
                    {
                        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
                    };
#else
                    innerHandler = new HttpClientHandler();
#endif

                    // Create the auth handler with the inner handler
                    var authHandler = new CachingDelegatingHandler(
                        serviceProvider.GetService<ICognitoAuthService>(),
                        serviceProvider.GetRequiredService<ICookbookStorage>())
                    {
                        InnerHandler = innerHandler
                    };

                    var httpClient = new HttpClient(authHandler)
                    {
                        BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute)
                    };

                    return new CookbookHttpClient(
                        serviceProvider.GetRequiredService<IConnectivity>(),
                        httpClient,
                        serviceProvider.GetRequiredService<IMemoryCache>(),
                        serviceProvider.GetRequiredService<ILogger<CookbookHttpClient>>());
                });
        builder.Services
            .AddMemoryCache()
            .AddSingleton(SecureStorage.Default)
            .AddSingleton(Preferences.Default);

        builder.Services
            .AddSingleton<ICookbookStorage, CookbookStorage>()
            .AddSingleton<INotificationService, NotificationService>()
            .AddSingleton<ISqliteCacheService, SqliteCacheService>()
            .AddSingleton<IOfflineCacheService>(sp => sp.GetRequiredService<ISqliteCacheService>())
            .AddSingleton<IAppConfiguration, AppConfiguration>()
            .AddSingleton<IImageCacheService, ImageCacheService>();

        // Register API services
        builder.Services
            .AddSingleton<IRecipeService, RecipeService>()
            .AddSingleton<IAccountService, AccountService>()
            .AddSingleton<IAuthorService, AuthorService>()
            .AddSingleton<ISearchService, SearchService>();

        builder.Services
            .AddSingleton<LoginViewModel>()
            .AddSingleton<HomePageViewModel>()
            .AddTransient<AuthorProfilePageViewModel>()
            .AddTransient<RecipeViewModel>()
            .AddTransient<ShoppingListViewModel>()
            .AddTransient<ShareRecipeViewModel>()
            .AddTransient<SettingsViewModel>()
            .AddSingleton<MyCookbookViewModel>()
            .AddSingleton<CalendarHomeViewModel>();

        // Register factory for SettingsViewModel (needed by AuthorProfilePageViewModel)
        builder.Services.AddTransient<Func<SettingsViewModel>>(sp => sp.GetRequiredService<SettingsViewModel>);

        builder.Services
            .AddSingleton<Login>()
            .AddSingleton<HomePage>()
            .AddTransient<AuthorHome>()
            .AddTransient<RecipePage>()
            .AddTransient<ShareRecipePage>()
            .AddTransient<ShoppingListHome>()
            .AddSingleton<MyCookbookHome>()
            .AddSingleton<CalendarHome>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}