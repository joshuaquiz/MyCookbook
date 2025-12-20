using System;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.RateLimiting;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Implementations;
using MyCookbook.API.BackgroundJobs;
using MyCookbook.API.Implementations.SiteParsers;
using MyCookbook.API.Middleware;
using MyCookbook.Common.Database;
using Npgsql;

namespace MyCookbook.API;

public sealed class Program
{
    public static async Task Main(
        string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Configure response compression for better performance
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<GzipCompressionProvider>();
            options.Providers.Add<BrotliCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/json" });
        });

        builder.Services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        // Configure output caching for GET endpoints
        builder.Services.AddOutputCache(options =>
        {
            // Default policy - cache for 1 minute
            options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(1)));

            // Popular recipes - cache for 5 minutes (high traffic, changes infrequently)
            options.AddPolicy("PopularRecipes", builder =>
                builder.Expire(TimeSpan.FromMinutes(5))
                    .SetVaryByQuery("take", "skip"));

            // Search results - cache for 2 minutes (varies by search params)
            options.AddPolicy("SearchResults", builder =>
                builder.Expire(TimeSpan.FromMinutes(2))
                    .SetVaryByQuery("term", "category", "ingredient", "exclude", "take", "skip"));

            // Individual recipes - cache for 10 minutes (rarely change)
            options.AddPolicy("RecipeDetails", builder =>
                builder.Expire(TimeSpan.FromMinutes(10)));

            // User cookbook - cache for 1 minute (user-specific, changes more often)
            options.AddPolicy("UserCookbook", builder =>
                builder.Expire(TimeSpan.FromMinutes(1))
                    .SetVaryByQuery("take", "skip")
                    .SetVaryByRouteValue("userId"));

            // Ingredients list - cache for 30 minutes (rarely changes)
            options.AddPolicy("Ingredients", builder =>
                builder.Expire(TimeSpan.FromMinutes(30)));
        });

        // Configure JWT Authentication
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "MyCookbook-Development-Secret-Key-Change-In-Production-12345678901234567890";
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "MyCookbook.API";
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "MyCookbook.App";

        // Get Cognito configuration for OAuth token validation
        var cognitoUserPoolId = Environment.GetEnvironmentVariable("COGNITO_USER_POOL_ID");
        var cognitoRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuers = new[]
                {
                    jwtIssuer, // Our custom JWT issuer for username/password login
                    string.IsNullOrEmpty(cognitoUserPoolId)
                        ? null
                        : $"https://cognito-idp.{cognitoRegion}.amazonaws.com/{cognitoUserPoolId}" // Cognito issuer for OAuth
                }.Where(x => x != null).ToArray(),
                ValidAudiences = new[]
                {
                    jwtAudience, // Our custom JWT audience
                    // Cognito doesn't use audience validation by default
                }.Where(x => x != null).ToArray(),
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                {
                    // For custom JWT tokens (username/password login)
                    var customKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

                    // For Cognito tokens (OAuth), we would need to fetch JWKS
                    // For now, return custom key - we'll enhance this if needed
                    return new[] { customKey };
                },
                // Allow some clock skew for token expiration
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Authentication failed: {Message}", context.Exception.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Token validated for user: {UserName}", context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization();

        // Configure rate limiting to prevent abuse and reduce costs
        builder.Services.AddRateLimiter(options =>
        {
            // Global rate limit: 100 requests per minute per IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    }));

            // Stricter limit for login endpoint: 5 attempts per 15 minutes per IP
            options.AddPolicy("login", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(15),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Handle rate limit rejections
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? retryAfterValue.TotalSeconds
                    : (double?)null;

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests. Please try again later.",
                    retryAfter = retryAfter
                }, cancellationToken);
            };
        });

        // Configure database connection with optimized pooling
        var connectionString = await GetDatabaseConnectionString();

        // Configure connection pooling for optimal performance
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            // Connection pooling configuration
            Pooling = true,
            MinPoolSize = 5,                    // Maintain minimum connections
            MaxPoolSize = 100,                  // Maximum connections in pool
            ConnectionIdleLifetime = 300,       // 5 minutes - prune idle connections
            ConnectionPruningInterval = 10,     // Check for pruning every 10 seconds

            // Performance optimizations
            MaxAutoPrepare = 20,                // Auto-prepare frequently used statements
            AutoPrepareMinUsages = 2,           // Prepare after 2 uses

            // Timeouts
            Timeout = 30,                       // Connection timeout
            CommandTimeout = 30,                // Command timeout

            // Keep alive to prevent connection drops
            KeepAlive = 30                      // Send keepalive every 30 seconds
        };

        builder.Services.AddDbContextFactory<MyCookbookContext>(
            opt =>
            {
                var npgsqlOptions = opt.UseNpgsql(
                    connectionStringBuilder.ConnectionString,
                    options => options.CommandTimeout(30));

                // Only enable sensitive data logging in development
                // In production, this would log passwords, tokens, and PII to CloudWatch
                if (builder.Environment.IsDevelopment())
                {
                    npgsqlOptions.EnableSensitiveDataLogging();
                }
            });
        builder.Services.AddSingleton<IJobQueuer, JobQueuer>();
        builder.Services.AddSingleton<ILdJsonExtractor, LdJsonExtractor>();
        builder.Services.AddSingleton<ILdJsonSectionJsonObjectExtractor, LdJsonSectionJsonObjectExtractor>();
        builder.Services.AddSingleton<IJsonNodeGraphExploder, JsonNodeGraphExploder>();
        builder.Services.AddSingleton<IUrlQueuerFromJsonObjectMap, UrlQueuerFromJsonObjectMap>();
        builder.Services.AddSingleton<IRecipeWebSiteWrapperProcessor, RecipeWebSiteWrapperProcessor>();
        builder.Services.AddSingleton<IUrlLdJsonDataNormalizer, UrlLdJsonDataNormalizer>();
        builder.Services.AddSingleton<IIngredientsCache, IngredientsCache>();
        builder.Services.AddSingleton<ISiteNormalizerFactory, SiteNormalizerFactory>();
        builder.Services.AddSingleton<UrlDownloaderJob>();
        builder.Services.AddHostedService<UrlDownloaderJob>();
        //builder.Services.AddSingleton<UrlReRunnerJob>();
        //builder.Services.AddHostedService<UrlReRunnerJob>();
        //builder.Services.AddSingleton<OneOffs>();
        //builder.Services.AddHostedService<OneOffs>();
        //builder.Services.AddSingleton<WebDataParserJob>();
        //builder.Services.AddHostedService<WebDataParserJob>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<MyCookbookContext>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        // Global exception handler should be first to catch all exceptions
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        // Response compression MUST be early in the pipeline
        app.UseResponseCompression();

        // Output caching should be early in the pipeline
        app.UseOutputCache();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Rate limiting should be after routing but before authentication
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/healthz");
        await app.RunAsync();
    }

    private static async Task<string> GetDatabaseConnectionString()
    {
        // Check if running in AWS (ECS/Lambda) or locally
        var dbSecretArn = Environment.GetEnvironmentVariable("DB_SECRET_ARN");
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "mycookbook";
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";

        if (string.IsNullOrEmpty(dbSecretArn) || string.IsNullOrEmpty(dbHost))
        {
            // Local development - use SQLite
            Console.WriteLine("Using SQLite for local development");
            return "Data Source=MyCookbook.db;";
        }

        // AWS environment - get credentials from Secrets Manager
        Console.WriteLine($"Fetching database credentials from Secrets Manager: {dbSecretArn}");

        using var client = new AmazonSecretsManagerClient();
        var response = await client.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = dbSecretArn
        });

        var credentials = JsonSerializer.Deserialize<DatabaseCredentials>(response.SecretString)
            ?? throw new InvalidOperationException("Failed to deserialize database credentials");

        var connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={credentials.username};Password={credentials.password};SSL Mode=Require;Trust Server Certificate=true";

        Console.WriteLine($"Database connection configured for: {dbHost}:{dbPort}/{dbName}");

        return connectionString;
    }

    private class DatabaseCredentials
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }
}
