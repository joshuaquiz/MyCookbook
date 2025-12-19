using System;
using System.Threading.Tasks;

namespace MyCookbook.App.Interfaces;

/// <summary>
/// Service for displaying user notifications (toasts, alerts, etc.)
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Show a short toast message
    /// </summary>
    Task ShowToastAsync(string message, ToastDuration duration = ToastDuration.Short);
    
    /// <summary>
    /// Show an error message to the user
    /// </summary>
    Task ShowErrorAsync(string message, string? title = null);
    
    /// <summary>
    /// Show a success message to the user
    /// </summary>
    Task ShowSuccessAsync(string message, string? title = null);
    
    /// <summary>
    /// Show a warning message to the user
    /// </summary>
    Task ShowWarningAsync(string message, string? title = null);
    
    /// <summary>
    /// Show a confirmation dialog
    /// </summary>
    Task<bool> ShowConfirmationAsync(string message, string title = "Confirm", string acceptButton = "Yes", string cancelButton = "No");
}

public enum ToastDuration
{
    Short,
    Long
}

