using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using MyCookbook.App.Implementations;
using MyCookbook.App.Interfaces;
using MyCookbook.App.Services;
using MyCookbook.Common.ApiModels;

namespace MyCookbook.App.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly ICookbookStorage _cookbookStorage;
    private readonly IAccountService _accountService;
    private readonly ICognitoAuthService _cognitoAuthService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    public LoginViewModel(
        ICookbookStorage cookbookStorage,
        IAccountService accountService,
        ICognitoAuthService cognitoAuthService)
    {
        _cookbookStorage = cookbookStorage;
        _accountService = accountService;
        _cognitoAuthService = cognitoAuthService;

        // Get app version
        AppVersion = $"Version {AppInfo.Current.VersionString} (Build {AppInfo.Current.BuildString})";
    }

    [RelayCommand]
    private async Task Login()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var loginResponse = await _accountService.LoginAsync(
                Username,
                Password,
                new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);

            // Validate response
            if (loginResponse == null)
            {
                throw new InvalidOperationException("Login response was null");
            }

            if (string.IsNullOrEmpty(loginResponse.AccessToken))
            {
                throw new InvalidOperationException("Access token was null or empty in login response");
            }

            if (string.IsNullOrEmpty(loginResponse.RefreshToken))
            {
                throw new InvalidOperationException("Refresh token was null or empty in login response");
            }

            // Store user profile
            await _cookbookStorage.SetUser(loginResponse.UserProfile);

            // Store JWT access token and refresh token
            await _cookbookStorage.SetAccessToken(loginResponse.AccessToken, loginResponse.ExpiresIn);
            await _cookbookStorage.SetRefreshToken(loginResponse.RefreshToken);

            // Navigate to main app
            if (Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0)
            {
                Microsoft.Maui.Controls.Application.Current.Windows[0].Page = new AppShell();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoginWithGoogle()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var result = await _cognitoAuthService.AuthenticateWithGoogleAsync();

            if (result.IsSuccess)
            {
                // Authentication successful, navigate to main app
                if (Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0)
                {
                    Microsoft.Maui.Controls.Application.Current.Windows[0].Page = new AppShell();
                }
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to authenticate with Google";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoginWithFacebook()
    {
        ErrorMessage = null;
        IsBusy = true;
        try
        {
            var result = await _cognitoAuthService.AuthenticateWithFacebookAsync();

            if (result.IsSuccess)
            {
                // Authentication successful, navigate to main app
                if (Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0)
                {
                    Microsoft.Maui.Controls.Application.Current.Windows[0].Page = new AppShell();
                }
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to authenticate with Facebook";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Keep the old method for backward compatibility if needed elsewhere
    public async Task LogIn(
        string username,
        string password)
    {
        Username = username;
        Password = password;
        await LoginCommand.ExecuteAsync(null);
    }
}