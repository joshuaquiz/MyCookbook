using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Implementations;
using MyCookbook.API.BackgroundJobs;
using MyCookbook.API.Implementations.SiteParsers;
using MyCookbook.Common.Database;

namespace MyCookbook.API;

public sealed class Program
{
    public static async Task Main(
        string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Configure database connection
        var connectionString = await GetDatabaseConnectionString();
        builder.Services.AddDbContextFactory<MyCookbookContext>(
            opt =>
            {
                opt.UseNpgsql(
                        connectionString,
                        options => options.CommandTimeout(30))
                    .LogTo(
                        Console.WriteLine,
                        [RelationalEventId.CommandExecuting, RelationalEventId.CommandError])
                    .EnableSensitiveDataLogging();
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
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        //app.UseAuthorization();
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
