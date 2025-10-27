using System;
using MyCookbook.App.Interfaces;
using MyCookbook.App.ViewModels;
using MyCookbook.App.Views;
//using Plugin.MauiMTAdmob;
//using Plugin.MauiMTAdmob.Extra;

namespace MyCookbook.App;

public partial class App
{
    private readonly ICookbookStorage _cookbookStorage;
    private readonly LoadingViewModel _loadingViewModel;
    private readonly Login _login;


    public App(
        ICookbookStorage cookbookStorage,
        LoadingScreen loadingScreen,
        LoadingViewModel loadingViewModel,
        Login login)
    {
        _cookbookStorage = cookbookStorage;
        _loadingViewModel = loadingViewModel;
        _login = login;

        // Show loading screen first
        MainPage = loadingScreen;

        InitializeComponent();

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