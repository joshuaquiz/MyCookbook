using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MyCookbook.API.Interfaces;
using MyCookbook.API.Implementations;
using MyCookbook.API.BackgroundJobs;
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
                    "Data Source=MyCookbook.db;");
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
        builder.Services.AddSingleton<IUrlProcessor, UrlProcessor>();
        builder.Services.AddSingleton<IIngredientsCache, IngredientsCache>();
        builder.Services.AddSingleton<JobRunner>();
        builder.Services.AddHostedService<JobRunner>();
        //builder.Services.AddSingleton<JobReRunner>();
        //builder.Services.AddHostedService<JobReRunner>();
        //builder.Services.AddSingleton<OneOffs>();
        //builder.Services.AddHostedService<OneOffs>();
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