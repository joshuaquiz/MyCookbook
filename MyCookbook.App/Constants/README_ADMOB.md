# AdMob Configuration Guide

## Overview
This app uses Google AdMob for monetization with automatic switching between test and production ads based on build configuration.

## Configuration

### AdMobConstants.cs
The `AdMobConstants` class manages all AdMob-related configuration:

- **UseTestAds**: Automatically set based on build configuration
  - `true` in DEBUG builds (uses Google's test ad units)
  - `false` in RELEASE builds (uses production ad units)

- **ApplicationId**: `ca-app-pub-3327131024555440~4066729058`
  - Your AdMob application ID (same for all environments)

- **ProductionBannerAdUnitId**: `ca-app-pub-3327131024555440/4066729058`
  - Your real ad unit ID for production

- **TestBannerAdUnitId**: `ca-app-pub-3940256099942544/6300978111`
  - Google's official test ad unit ID for banner ads
  - Safe to use during development without risk of invalid traffic

- **BannerAdUnitId**: Property that returns the appropriate ID based on `UseTestAds`

## How It Works

### Development (DEBUG builds)
When you build in DEBUG mode:
- Test ads will be shown automatically
- You'll see Google's sample ads with "Test Ad" label
- No risk of invalid traffic or account suspension
- Safe to click on test ads during development

### Production (RELEASE builds)
When you build in RELEASE mode:
- Real ads will be shown
- Actual revenue will be generated
- Clicks count toward your earnings

## Pages with Ads

Banner ads are displayed at the bottom of these pages:
1. **HomePage** - Main recipe feed
2. **MyCookbookHome** - User's saved recipes
3. **ShoppingListHome** - Shopping list
4. **CalendarHome** - Meal planner
5. **AuthorHome** - Author profile page

## Testing

### To test with test ads:
```powershell
# Build in Debug mode (default)
dotnet build MyCookbook.App/MyCookbook.App.csproj -c Debug
```

### To test with production ads:
```powershell
# Build in Release mode
dotnet build MyCookbook.App/MyCookbook.App.csproj -c Release
```

**⚠️ WARNING**: Do NOT repeatedly click on production ads during testing. This can result in invalid traffic and account suspension.

## Switching Between Test and Production

The switching is automatic based on build configuration. However, if you need to manually override:

1. Open `MyCookbook.App/Constants/AdMobConstants.cs`
2. Modify the `UseTestAds` property:
   ```csharp
   // Force test ads (for testing)
   public const bool UseTestAds = true;
   
   // Force production ads (for release)
   public const bool UseTestAds = false;
   ```

## Ad Configuration Settings

The following settings are configured in `App.xaml.cs`:
- **UserPersonalizedAds**: `true` - Shows personalized ads based on user interests
- **ComplyWithFamilyPolicies**: `true` - Ensures compliance with family-friendly policies
- **UseRestrictedDataProcessing**: `true` - Complies with privacy regulations
- **TagForChildDirectedTreatment**: `Unspecified` - Not specifically targeting children
- **TagForUnderAgeOfConsent**: `Unspecified` - Not specifically targeting users under age of consent
- **MaxAdContentRating**: `G` - General audiences (most restrictive)
- **AdChoicesCorner**: `BOTTOM_LEFT` - Position of ad choices icon
- **MaximumNumberOfAdsCached**: `3` - Number of ads to pre-cache

## Troubleshooting

### Ads not showing
1. Check internet connection
2. Verify you're using the correct ad unit ID
3. In DEBUG mode, you should see test ads immediately
4. In RELEASE mode with new ad units, it may take a few hours for ads to start showing

### "Test Ad" label showing in production
- You're likely building in DEBUG mode
- Build in RELEASE mode for production deployment

### Invalid traffic warnings
- Make sure you're using test ads during development
- Don't repeatedly click on production ads
- Use different devices for testing

## Resources

- [AdMob Help Center](https://support.google.com/admob)
- [Plugin.MauiMTAdmob Documentation](https://github.com/marcojak/MTAdmob)
- [Google AdMob Test Ads](https://developers.google.com/admob/android/test-ads)

