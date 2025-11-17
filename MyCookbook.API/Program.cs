using System;
using System.Threading.Tasks;
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

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContextFactory<MyCookbookContext>(
            opt =>
            {
                opt.UseSqlite(
                        "Data Source=MyCookbook.db;",
                        options => options.CommandTimeout(30))
                    .LogTo(
                        Console.WriteLine,
                        [RelationalEventId.CommandExecuting, RelationalEventId.CommandError])
                    .EnableSensitiveDataLogging();
            });
        /*builder.Services.AddDbContextFactory<MyCookbookContext>(
            opt =>
            {
                opt.UseNpgsql(
                    "");
            });*/
        builder.Services.AddSingleton<IJobQueuer, JobQueuer>();
        builder.Services.AddSingleton<ILdJsonExtractor, LdJsonExtractor>();
        builder.Services.AddSingleton<ILdJsonSectionJsonObjectExtractor, LdJsonSectionJsonObjectExtractor>();
        builder.Services.AddSingleton<IJsonNodeGraphExploder, JsonNodeGraphExploder>();
        builder.Services.AddSingleton<IUrlQueuerFromJsonObjectMap, UrlQueuerFromJsonObjectMap>();
        builder.Services.AddSingleton<IRecipeWebSiteWrapperProcessor, RecipeWebSiteWrapperProcessor>();
        builder.Services.AddSingleton<IUrlLdJsonDataNormalizer, UrlLdJsonDataNormalizer>();
        builder.Services.AddSingleton<IIngredientsCache, IngredientsCache>();
        builder.Services.AddSingleton<ISiteNormalizerFactory, SiteNormalizerFactory>();
        //builder.Services.AddSingleton<UrlDownloaderJob>();
        //builder.Services.AddHostedService<UrlDownloaderJob>();
        //builder.Services.AddSingleton<UrlReRunnerJob>();
        //builder.Services.AddHostedService<UrlReRunnerJob>();
        builder.Services.AddSingleton<OneOffs>();
        builder.Services.AddHostedService<OneOffs>();
        //builder.Services.AddSingleton<WebDataParserJob>();
        //builder.Services.AddHostedService<WebDataParserJob>();
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
        await app.RunAsync();
    }
}
