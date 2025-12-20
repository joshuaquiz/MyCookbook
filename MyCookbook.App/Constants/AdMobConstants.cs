namespace MyCookbook.App.Constants;

/// <summary>
/// AdMob configuration constants for development and production environments.
/// </summary>
public static class AdMobConstants
{
    /// <summary>
    /// Set to true to use test ads, false to use production ads.
    /// IMPORTANT: Always set to false before releasing to production!
    /// </summary>
#if DEBUG
    public const bool UseTestAds = true;
#else
    public const bool UseTestAds = false;
#endif

    /// <summary>
    /// AdMob Application ID (same for test and production)
    /// </summary>
    public const string ApplicationId = "ca-app-pub-3327131024555440~4066729058";

    /// <summary>
    /// Production Ad Unit ID for banner ads
    /// </summary>
    public const string ProductionBannerAdUnitId = "ca-app-pub-3327131024555440/4066729058";

    /// <summary>
    /// Google's test Ad Unit ID for banner ads
    /// Use this during development to avoid invalid traffic
    /// </summary>
    public const string TestBannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";

    /// <summary>
    /// Gets the appropriate banner ad unit ID based on the current environment
    /// </summary>
    public static string BannerAdUnitId => UseTestAds ? TestBannerAdUnitId : ProductionBannerAdUnitId;
}

