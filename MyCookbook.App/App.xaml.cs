using MyCookbook.App.Interfaces;
using MyCookbook.App.Views;
//using Plugin.MauiMTAdmob;
//using Plugin.MauiMTAdmob.Extra;

namespace MyCookbook.App;

public partial class App
{
    public App(
        ICookbookStorage cookbookStorage,
        Login login)
    {
        if (cookbookStorage.GetUser().GetAwaiter().GetResult() is null)
        {
            MainPage = login;
        }
        else
        {
            MainPage = new AppShell();
        }

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
    }
}