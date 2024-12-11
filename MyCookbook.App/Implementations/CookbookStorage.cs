using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MyCookbook.App.Interfaces;
using MyCookbook.Common;

namespace MyCookbook.App.Implementations;

public sealed class CookbookStorage(
    ISecureStorage secureStorage,
    IPreferences preferences)
    : ICookbookStorage
{
    public ValueTask<AppTheme> GetCurrentAppTheme(
        Application application)
    {
        var value = preferences.Get(
            nameof(AppTheme),
            (int) application.UserAppTheme);
        return ValueTask.FromResult(
            (AppTheme) value);
    }

    public Task SetAppTheme(
        AppTheme appTheme,
        Application application)
    {
        preferences.Set(
            nameof(AppTheme),
            (int) appTheme);
        application.UserAppTheme = appTheme;
        return Task.CompletedTask;
    }

    public Task Empty()
    {
        preferences.Clear();
        secureStorage.RemoveAll();
        return Task.CompletedTask;
    }

    public async Task SetUser(
        UserProfile user) =>
        await secureStorage.SetAsync(
            "UserProfile",
            JsonSerializer.Serialize(
                user));

    public async ValueTask<UserProfile?> GetUser()
    {
        var profileAsString = secureStorage.GetAsync(
                "UserProfile")
            .GetAwaiter()
            .GetResult();
        return profileAsString == null
            ? null
            : JsonSerializer.Deserialize<UserProfile>(
                profileAsString);
    }
}