using System;
using Microsoft.Maui.Controls;
using MyCookbook.App.Exceptions;
using MyCookbook.App.ViewModels;

namespace MyCookbook.App.Views;

public partial class Login
{
    public LoginViewModel ViewModel { get; set; }

    public Login(
        LoginViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = ViewModel;
        InitializeComponent();
    }

    private async void Button_OnClicked(object? sender, EventArgs args)
    {
        /*WebAuthenticatorResult authResult = await WebAuthenticator.Default.AuthenticateAsync(
            new Uri("https://mysite.com/mobileauth/Microsoft"),
            new Uri("myapp://"));
        string accessToken = authResult?.AccessToken;*/
        try
        {
            await ViewModel.LogIn(
                Username.Text,
                Password.Text);
            Application.Current!.MainPage = new AppShell();
        }
        catch (NoInternetException e)
        {
            await DisplayAlert(
                "Connection issue",
                e.Message,
                "OK");
        }
        catch (Exception e)
        {
            await DisplayAlert(
                "Ops",
                e.Message,
                "OK");
        }
    }

    private void Google_OnClicked(object? sender, EventArgs e)
    {
        /*WebAuthenticatorResult authResult = await WebAuthenticator.Default.AuthenticateAsync(
            new Uri("https://mysite.com/mobileauth/Microsoft"),
            new Uri("myapp://"));
        string accessToken = authResult?.AccessToken;*/
    }

    private void Facebook_OnClicked(object? sender, EventArgs e)
    {
        /*WebAuthenticatorResult authResult = await WebAuthenticator.Default.AuthenticateAsync(
            new Uri("https://mysite.com/mobileauth/Microsoft"),
            new Uri("myapp://"));
        string accessToken = authResult?.AccessToken;*/
    }
}