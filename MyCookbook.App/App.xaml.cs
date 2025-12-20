using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MyCookbook.App.Controls;
using MyCookbook.App.Interfaces;
using MyCookbook.App.Views;
//using Plugin.MauiMTAdmob;
//using Plugin.MauiMTAdmob.Extra;

namespace MyCookbook.App;

public partial class App
{
    private readonly ICookbookStorage _cookbookStorage;
    private readonly Login _login;

    public App(
        ICookbookStorage cookbookStorage,
        IServiceProvider serviceProvider)
    {
        _cookbookStorage = cookbookStorage;
        InitializeComponent();
        _login = serviceProvider.GetRequiredService<Login>();
        var imageCacheService = serviceProvider.GetRequiredService<IImageCacheService>();
        CachedImage.Initialize(imageCacheService);
        var sqliteCacheService = serviceProvider.GetRequiredService<ISqliteCacheService>();
        Task.Run(async () => await sqliteCacheService.InitializeAsync()).Wait();
        //CrossMauiMTAdmob.Current.UserPersonalizedAds = true;
        //CrossMauiMTAdmob.Current.ComplyWithFamilyPolicies = true;
        //CrossMauiMTAdmob.Current.UseRestrictedDataProcessing = true;
        //CrossMauiMTAdmob.Current.AdsId = "ca-app-pub-3327131024555440/4066729058";
        //CrossMauiMTAdmob.Current.TagForChildDirectedTreatment = MTTagForChildDirectedTreatment.TagForChildDirectedTreatmentUnspecified;
        //CrossMauiMTAdmob.Current.TagForUnderAgeOfConsent = MTTagForUnderAgeOfConsent.TagForUnderAgeOfConsentUnspecified;
        //CrossMauiMTAdmob.Current.MaxAdContentRating = MTMaxAdContentRating.MaxAdContentRatingG;
        //CrossMauiMTAdmob.Current.AdChoicesCorner = AdChoicesCorner.ADCHOICES_BOTTOM_LEFT;
        //CrossMauiMTAdmob.Current.MaximumNumberOfAdsCached = 3;
        UserAppTheme = cookbookStorage.GetCurrentAppTheme(this).GetAwaiter().GetResult();
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        InitializeAppAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Start with login page, will switch to AppShell if user is logged in
        var window = new Window(_login);

        // Handle deep linking
        window.Created += (s, e) =>
        {
            // Subscribe to app links
            AppActions.OnAppAction += HandleAppLink;
        };

        return window;
    }

    private async void HandleAppLink(object? sender, AppActionEventArgs e)
    {
        await HandleDeepLink(e.AppAction.Id);
    }

    private async Task HandleDeepLink(string uri)
    {
        try
        {
            // Parse the URI to extract recipe GUID
            // Supports: mycookbook://recipe/{guid} or https://mycookbook.app/recipe/{guid}
            var uriParts = uri.Split('/');
            if (uriParts.Length > 0)
            {
                var lastPart = uriParts[^1];
                if (Guid.TryParse(lastPart, out var recipeGuid))
                {
                    // Navigate to recipe page
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        if (Windows.Count > 0 && Windows[0].Page is AppShell)
                        {
                            await Shell.Current.GoToAsync($"///{nameof(RecipePage)}?Guid={recipeGuid}");
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Deep link handling error: {ex.Message}");
        }
    }

    private async void InitializeAppAsync()
    {
        try
        {
            // Check if user is already logged in
            var user = await _cookbookStorage.GetUser();

            // Also check if we have a valid access token
            var accessToken = await _cookbookStorage.GetAccessToken();

            // If logged in AND have a valid token, navigate to main app
            if (user != null && !string.IsNullOrEmpty(accessToken))
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (Windows.Count > 0)
                    {
                        Windows[0].Page = new AppShell();
                    }
                });
            }
            else
            {
                // Token expired or invalid, clear storage and stay on login page
                if (user != null && string.IsNullOrEmpty(accessToken))
                {
                    await _cookbookStorage.Empty();
                }
            }
            // Otherwise stay on login page (already set in CreateWindow)
        }
        catch (Exception)
        {
            // On error, stay on login page (already set in CreateWindow)
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // Reserved for future global exception handling
        // 401 errors are now handled at the HTTP client level
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        // Reserved for future global exception handling
        // 401 errors are now handled at the HTTP client level
    }

    private async Task HandleUnauthorizedAccess()
    {
        try
        {
            // Clear all stored authentication data
            await _cookbookStorage.Empty();

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (Windows.Count > 0)
                {
                    // Navigate to login page
                    Windows[0].Page = _login;

                    // Show a message to the user
                    if (Windows[0].Page != null)
                    {
                        await Windows[0].Page!.DisplayAlertAsync(
                            "Session Expired",
                            "Your session has expired. Please log in again.",
                            "OK");
                    }
                }
            });
        }
        catch
        {
            // If anything fails, just navigate to login
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (Windows.Count > 0)
                {
                    Windows[0].Page = _login;
                }
            });
        }
    }
}