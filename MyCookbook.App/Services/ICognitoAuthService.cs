using System;
using System.Threading.Tasks;

namespace MyCookbook.App.Services;

/// <summary>
/// Service for handling Cognito OAuth authentication
/// </summary>
public interface ICognitoAuthService
{
    /// <summary>
    /// Authenticate with Google via Cognito
    /// </summary>
    Task<CognitoAuthResult> AuthenticateWithGoogleAsync();

    /// <summary>
    /// Authenticate with Facebook via Cognito
    /// </summary>
    Task<CognitoAuthResult> AuthenticateWithFacebookAsync();

    /// <summary>
    /// Sign out the current user
    /// </summary>
    Task SignOutAsync();

    /// <summary>
    /// Get the current access token (refresh if needed)
    /// </summary>
    Task<string?> GetAccessTokenAsync();

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    Task<bool> IsAuthenticatedAsync();
}

/// <summary>
/// Result of Cognito authentication
/// </summary>
public class CognitoAuthResult
{
    public bool IsSuccess { get; set; }
    public string? AccessToken { get; set; }
    public string? IdToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
}

