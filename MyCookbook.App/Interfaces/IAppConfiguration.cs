namespace MyCookbook.App.Interfaces;

/// <summary>
/// Secure application configuration service
/// </summary>
public interface IAppConfiguration
{
    /// <summary>
    /// Get the API base URL
    /// </summary>
    string ApiBaseUrl { get; }
    
    /// <summary>
    /// Get the API timeout in seconds
    /// </summary>
    int ApiTimeoutSeconds { get; }
    
    /// <summary>
    /// Whether to use certificate pinning
    /// </summary>
    bool UseCertificatePinning { get; }
    
    /// <summary>
    /// Get the expected certificate public key hash (for pinning)
    /// </summary>
    string? CertificatePublicKeyHash { get; }
    
    /// <summary>
    /// Whether the app is in debug mode
    /// </summary>
    bool IsDebugMode { get; }
}

