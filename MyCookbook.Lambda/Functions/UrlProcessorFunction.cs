using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Implementations;
using MyCookbook.API.Implementations.SiteParsers;
using MyCookbook.API.Interfaces;
using MyCookbook.Lambda.Interfaces;
using MyCookbook.Lambda.Models;
using MyCookbook.Lambda.Services;
using System.Text.Json;

namespace MyCookbook.Lambda.Functions;

public class UrlProcessorFunction
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UrlProcessorFunction> _logger;

    public UrlProcessorFunction()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<UrlProcessorFunction>>();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });

        // AWS Services
        services.AddAWSService<Amazon.S3.IAmazonS3>();
        
        // Lambda Services
        services.AddSingleton<IS3Service, S3Service>();
        services.AddSingleton<IUrlProcessorService, UrlProcessorService>();
        
        // API Services (from MyCookbook.API)
        services.AddSingleton<IJobQueuer, JobQueuer>();
        services.AddSingleton<ILdJsonExtractor, LdJsonExtractor>();
        services.AddSingleton<ILdJsonSectionJsonObjectExtractor, LdJsonSectionJsonObjectExtractor>();
        services.AddSingleton<IJsonNodeGraphExploder, JsonNodeGraphExploder>();
        services.AddSingleton<IUrlQueuerFromJsonObjectMap, UrlQueuerFromJsonObjectMap>();
        services.AddSingleton<IRecipeWebSiteWrapperProcessor, RecipeWebSiteWrapperProcessor>();
        services.AddSingleton<IUrlLdJsonDataNormalizer, UrlLdJsonDataNormalizer>();
        services.AddSingleton<IIngredientsCache, IngredientsCache>();
        services.AddSingleton<ISiteNormalizerFactory, SiteNormalizerFactory>();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            _logger.LogInformation("URL processor request received");

            if (string.IsNullOrEmpty(request.Body))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(new { error = "Request body is required" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            var urlRequest = JsonSerializer.Deserialize<UrlRequest>(request.Body);
            if (urlRequest == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = JsonSerializer.Serialize(new { error = "Invalid request format or missing URL" }),
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };
            }

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(context.RemainingTime);
            var urlProcessorService = _serviceProvider.GetRequiredService<IUrlProcessorService>();
            await urlProcessorService.ProcessUrlAsync(urlRequest.Url, cts.Token);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(new { message = "URL processed successfully", url = urlRequest.Url }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing URL request");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Internal server error", details = ex.Message }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}

