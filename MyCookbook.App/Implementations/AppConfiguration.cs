using System;
using MyCookbook.App.Interfaces;

namespace MyCookbook.App.Implementations;

/// <summary>
/// Secure application configuration implementation
/// </summary>
public class AppConfiguration : IAppConfiguration
{
    public string ApiBaseUrl { get; }
    public int ApiTimeoutSeconds { get; }
    public bool UseCertificatePinning { get; }
    public string? CertificatePublicKeyHash { get; }
    public bool IsDebugMode { get; }

    public AppConfiguration()
    {
        // Get configuration from environment variables or use defaults
        ApiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") 
            ?? "http://api-development-mycookbook.g3software.net";
        
        ApiTimeoutSeconds = int.TryParse(
            Environment.GetEnvironmentVariable("API_TIMEOUT_SECONDS"), 
            out var timeout) ? timeout : 30;
        
        // Certificate pinning should be enabled in production
        UseCertificatePinning = Environment.GetEnvironmentVariable("USE_CERT_PINNING") == "true";
        
        // Public key hash for certificate pinning (should be set in production)
        CertificatePublicKeyHash = Environment.GetEnvironmentVariable("CERT_PUBLIC_KEY_HASH");
        
#if DEBUG
        IsDebugMode = true;
#else
        IsDebugMode = false;
#endif
    }
}

