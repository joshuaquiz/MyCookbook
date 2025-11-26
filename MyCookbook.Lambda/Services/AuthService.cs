using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MyCookbook.Lambda.Interfaces;
using MyCookbook.Lambda.Models;

namespace MyCookbook.Lambda.Services;

public class AuthService : IAuthService
{
    private readonly IS3Service _s3Service;
    private readonly string _configBucketName;
    private readonly string _configKey;
    private readonly string _dbBucketName;
    private readonly string _dbKey;

    public AuthService(IS3Service s3Service)
    {
        _s3Service = s3Service;
        _configBucketName = Environment.GetEnvironmentVariable("CONFIG_BUCKET_NAME") ?? "mycookbook-config";
        _configKey = Environment.GetEnvironmentVariable("CONFIG_KEY") ?? "auth-config.json";
        _dbBucketName = Environment.GetEnvironmentVariable("DB_BUCKET_NAME") ?? "g3-cookbook-db-files";
        _dbKey = Environment.GetEnvironmentVariable("DB_KEY") ?? "MyCookbook.db";
    }

    public async Task<AuthResponse?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        // Download config from S3
        var configJson = await _s3Service.DownloadFileAsStringAsync(_configBucketName, _configKey, cancellationToken);
        var config = JsonSerializer.Deserialize<ConfigData>(configJson);

        if (config == null)
        {
            return null;
        }

        // Verify username and password
        if (config.Username != username)
        {
            return null;
        }

        var passwordHash = HashPassword(password);
        if (config.PasswordHash != passwordHash)
        {
            return null;
        }

        // Generate pre-signed URL for database file (valid for 1 hour)
        var databaseUrl = _s3Service.GetPreSignedUrl(_dbBucketName, _dbKey, TimeSpan.FromHours(1));

        // Return the two fields from config plus the database URL
        return new AuthResponse
        {
            Field1 = config.Field1,
            Field2 = config.Field2,
            DatabaseUrl = databaseUrl
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

