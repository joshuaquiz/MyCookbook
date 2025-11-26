using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCookbook.Lambda.Interfaces;
using MyCookbook.Lambda.Models;
using MyCookbook.Lambda.Services;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MyCookbook.Lambda.Functions;

public class AuthFunction
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthFunction> _logger;

    public AuthFunction()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<AuthFunction>>();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });

        services.AddAWSService<Amazon.S3.IAmazonS3>();
        services.AddSingleton<IS3Service, S3Service>();
        services.AddSingleton<IAuthService, AuthService>();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            _logger.LogInformation("Auth request received");

            if (string.IsNullOrEmpty(request.Body))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(new { error = "Request body is required" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var authRequest = JsonSerializer.Deserialize<AuthRequest>(request.Body);
            if (authRequest == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(new { error = "Invalid request format" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            var result = await authService.AuthenticateAsync(authRequest.Username, authRequest.Password);

            if (result == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 401,
                    Body = JsonSerializer.Serialize(new { error = "Invalid credentials" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(result),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auth request");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Internal server error", details = ex.Message }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}

