using System;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MyCookbook.App.Interfaces;

namespace MyCookbook.App.Implementations;

/// <summary>
/// Implementation of notification service using CommunityToolkit.Maui
/// </summary>
public class NotificationService : INotificationService
{
    public async Task ShowToastAsync(string message, Interfaces.ToastDuration duration = Interfaces.ToastDuration.Short)
    {
        try
        {
            var toastDuration = duration == Interfaces.ToastDuration.Short
                ? CommunityToolkit.Maui.Core.ToastDuration.Short
                : CommunityToolkit.Maui.Core.ToastDuration.Long;

            var toast = Toast.Make(message, toastDuration);
            await toast.Show();
        }
        catch (Exception ex)
        {
            // Fallback to debug output if toast fails
            System.Diagnostics.Debug.WriteLine($"Toast error: {ex.Message}");
        }
    }

    public async Task ShowErrorAsync(string message, string? title = null)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    title ?? "Error",
                    message,
                    "OK");
            }
        });
    }

    public async Task ShowSuccessAsync(string message, string? title = null)
    {
        await ShowToastAsync(message, Interfaces.ToastDuration.Short);
    }

    public async Task ShowWarningAsync(string message, string? title = null)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    title ?? "Warning",
                    message,
                    "OK");
            }
        });
    }

    public async Task<bool> ShowConfirmationAsync(
        string message, 
        string title = "Confirm", 
        string acceptButton = "Yes", 
        string cancelButton = "No")
    {
        var result = false;
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (Application.Current?.MainPage != null)
            {
                result = await Application.Current.MainPage.DisplayAlert(
                    title,
                    message,
                    acceptButton,
                    cancelButton);
            }
        });
        return result;
    }
}

