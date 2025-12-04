using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using MyCookbook.App.Interfaces;
using MyCookbook.App.ViewModels;
using MyCookbook.App.Views;
//using Plugin.MauiMTAdmob;
//using Plugin.MauiMTAdmob.Extra;

namespace MyCookbook.App;

public partial class App
{
    private readonly ICookbookStorage _cookbookStorage;
    private readonly IServiceProvider _serviceProvider;
    private LoadingViewModel _loadingViewModel;
    private Login _login;


    public App(
        ICookbookStorage cookbookStorage,
        IServiceProvider serviceProvider)
    {
        _cookbookStorage = cookbookStorage;
        _serviceProvider = serviceProvider;

        // Initialize resources FIRST before resolving any pages
        InitializeComponent();

        // NOW resolve pages after resources are loaded
        _loadingViewModel = _serviceProvider.GetRequiredService<LoadingViewModel>();
        var loadingScreen = _serviceProvider.GetRequiredService<LoadingScreen>();
        _login = _serviceProvider.GetRequiredService<Login>();

        // Show loading screen after resources are loaded
        MainPage = loadingScreen;

        //CrossMauiMTAdmob.Current.UserPersonalizedAds = true;
        //CrossMauiMTAdmob.Current.ComplyWithFamilyPolicies = true;
        //CrossMauiMTAdmob.Current.UseRestrictedDataProcessing = true;
        //CrossMauiMTAdmob.Current.AdsId = "ca-app-pub-3327131024555440/4066729058";
        //CrossMauiMTAdmob.Current.TagForChildDirectedTreatment = MTTagForChildDirectedTreatment.TagForChildDirectedTreatmentUnspecified;
        //CrossMauiMTAdmob.Current.TagForUnderAgeOfConsent = MTTagForUnderAgeOfConsent.TagForUnderAgeOfConsentUnspecified;
        //CrossMauiMTAdmob.Current.MaxAdContentRating = MTMaxAdContentRating.MaxAdContentRatingG;
        //CrossMauiMTAdmob.Current.AdChoicesCorner = AdChoicesCorner.ADCHOICES_BOTTOM_LEFT;
        //CrossMauiMTAdmob.Current.MaximumNumberOfAdsCached = 3;
        UserAppTheme = AppTheme.Light;//cookbookStorage.GetCurrentAppTheme(this).GetAwaiter().GetResult();

        // Start the initialization process
        InitializeAppAsync();
    }

    private async void InitializeAppAsync()
    {
        try
        {
            // Run data loading in the ViewModel
            await _loadingViewModel.InitializeAppAsync();

            // After loading completes, navigate to the appropriate page
            var user = await _cookbookStorage.GetUser();
            if (user is null)
            {
                MainPage = _login;
            }
            else
            {
                MainPage = new AppShell();
            }
        }
        catch (Exception ex)
        {
            // Handle error - show login page as fallback
            MainPage = _login;
        }
    }
}