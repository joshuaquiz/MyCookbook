using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCookbook.Common.Database;

namespace MyCookbook.API.Functions;

/// <summary>
/// Lambda function triggered by Cognito Post Authentication trigger
/// Creates or updates user in the database after successful authentication
/// </summary>
public class PostAuthenticationFunction
{
    private readonly ILogger<PostAuthenticationFunction> _logger;
    private readonly string _dbSecretArn;
    private readonly string _dbHost;
    private readonly string _dbName;
    private readonly int _dbPort;

    public PostAuthenticationFunction()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder => builder.AddLambdaLogger());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        _logger = serviceProvider.GetRequiredService<ILogger<PostAuthenticationFunction>>();

        _dbSecretArn = Environment.GetEnvironmentVariable("DB_SECRET_ARN") ?? throw new InvalidOperationException("DB_SECRET_ARN not set");
        _dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? throw new InvalidOperationException("DB_HOST not set");
        _dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "mycookbook";
        _dbPort = int.Parse(Environment.GetEnvironmentVariable("DB_PORT") ?? "5432");
    }

    public async Task<CognitoPostAuthenticationEvent> FunctionHandler(CognitoPostAuthenticationEvent cognitoEvent, ILambdaContext context)
    {
        try
        {
            _logger.LogInformation($"Post authentication for user: {cognitoEvent.UserName}");

            var userAttributes = cognitoEvent.Request.UserAttributes;
            var sub = userAttributes["sub"];
            var email = userAttributes.GetValueOrDefault("email");
            var givenName = userAttributes.GetValueOrDefault("given_name");
            var familyName = userAttributes.GetValueOrDefault("family_name");
            var picture = userAttributes.GetValueOrDefault("picture");
            var identities = userAttributes.GetValueOrDefault("identities");

            // Determine auth provider
            string authProvider = "local";
            string? providerUserId = null;

            if (!string.IsNullOrEmpty(identities))
            {
                try
                {
                    var identitiesArray = JsonSerializer.Deserialize<CognitoIdentity[]>(identities);
                    if (identitiesArray != null && identitiesArray.Length > 0)
                    {
                        var identity = identitiesArray[0];
                        authProvider = identity.ProviderName?.ToLower() ?? "local";
                        providerUserId = identity.UserId;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse identities");
                }
            }

            // Get database credentials from Secrets Manager
            var dbCredentials = await GetDatabaseCredentials();

            // Create connection string
            var connectionString = $"Host={_dbHost};Port={_dbPort};Database={_dbName};Username={dbCredentials.Username};Password={dbCredentials.Password};SSL Mode=Require;Trust Server Certificate=true";

            // Create DbContext
            var optionsBuilder = new DbContextOptionsBuilder<MyCookbookContext>();
            optionsBuilder.UseNpgsql(connectionString);

            await using var dbContext = new MyCookbookContext(optionsBuilder.Options);

            // Check if author already exists
            var existingAuthor = await dbContext.Authors
                .FirstOrDefaultAsync(a => a.CognitoSub == sub);

            if (existingAuthor == null)
            {
                _logger.LogInformation($"Creating new author for sub: {sub}");

                // Create Author profile with authentication fields
                var author = new Author
                {
                    AuthorId = Guid.NewGuid(),
                    Name = !string.IsNullOrEmpty(givenName) || !string.IsNullOrEmpty(familyName)
                        ? $"{givenName} {familyName}".Trim()
                        : email ?? cognitoEvent.UserName,
                    Bio = string.Empty,
                    IsVisible = false,
                    AuthorType = AuthorType.NormalUser,
                    Email = email,
                    EmailVerified = userAttributes.GetValueOrDefault("email_verified") == "true",
                    AuthProvider = authProvider,
                    ProviderUserId = providerUserId,
                    CognitoSub = sub,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                dbContext.Authors.Add(author);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation($"Author created successfully: {author.AuthorId}");
            }
            else
            {
                _logger.LogInformation($"Updating existing author: {existingAuthor.AuthorId}");

                // Update last login
                existingAuthor.LastLoginAt = DateTime.UtcNow;

                await dbContext.SaveChangesAsync();
            }

            return cognitoEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in post authentication");
            throw;
        }
    }

    private async Task<DatabaseCredentials> GetDatabaseCredentials()
    {
        using var client = new AmazonSecretsManagerClient();
        var response = await client.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = _dbSecretArn
        });

        return JsonSerializer.Deserialize<DatabaseCredentials>(response.SecretString)
            ?? throw new InvalidOperationException("Failed to deserialize database credentials");
    }

    private class DatabaseCredentials
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private class CognitoIdentity
    {
        public string? UserId { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderType { get; set; }
    }
}

// Cognito Post Authentication Event Model
public class CognitoPostAuthenticationEvent
{
    public string Version { get; set; } = string.Empty;
    public string TriggerSource { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string UserPoolId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public CognitoPostAuthenticationRequest Request { get; set; } = new();
    public CognitoPostAuthenticationResponse Response { get; set; } = new();
}

public class CognitoPostAuthenticationRequest
{
    public Dictionary<string, string> UserAttributes { get; set; } = new();
}

public class CognitoPostAuthenticationResponse
{
}

