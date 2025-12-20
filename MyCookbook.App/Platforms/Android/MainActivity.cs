using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;
using Plugin.MauiMTAdmob;

namespace MyCookbook.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        string appId = "ca-app-pub-3327131024555440~4066729058";

        // If you don't have a license code, you can use the following line instead:
        CrossMauiMTAdmob.Current.Init(this, appId);
    }

    // Add the following code to the MainActivity class to handle the lifecycle events:
    // CrossMauiMTAdmob.Current.OnResume() allows to load and show the app open ad when the app is resumed.
    // You need to add your logic to decide if and when show the app open ad.
    // You can use CrossMauiMTAdmob.Current.AreOpenAdsEnabled to enable or disable the app open ads.
    protected override void OnResume()
    {
        base.OnResume();
        CrossMauiMTAdmob.Current.OnResume();
    }
}