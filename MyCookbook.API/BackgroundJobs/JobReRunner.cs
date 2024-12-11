using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;

namespace MyCookbook.API.BackgroundJobs;

public sealed class JobReRunner(
    IDbContextFactory<MyCookbookContext> myCookbookContextFactory,
    IUrlProcessor urlProcessor,
    IIngredientsCache ingredientsCache,
    ILogger<JobReRunner> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await using var db = await myCookbookContextFactory.CreateDbContextAsync(
                stoppingToken);
            await ingredientsCache.LoadData(
                db,
                stoppingToken);
            var latestParser = Enum.GetValues(
                    typeof(ParserVersion))
                .Cast<ParserVersion>()
                .Max();
            var recipeUrls = await db.RecipeUrls
                .Where(
                    x =>
                        (x.ProcessingStatus == RecipeUrlStatus.NotStarted
                        /*|| x.ProcessingStatus == RecipeUrlStatus.FinishedSuccess
                        || x.ProcessingStatus == RecipeUrlStatus.FinishedError*/)
                        && x.ParserVersion <= latestParser)
                .ToListAsync(
                    stoppingToken);
            foreach (var recipeUrl in recipeUrls)
            {
                logger.LogInformation(
                    $"Reprocessing {recipeUrl.Uri}");
                recipeUrl.ParserVersion = latestParser;
                recipeUrl.Exception = null;
                logger.LogInformation(
                    $"Starting to process {recipeUrl.Uri}");
                recipeUrl.ProcessingStatus = RecipeUrlStatus.Started;
                try
                {
                    var recipe = await urlProcessor.ProcessUrl(
                        db,
                        recipeUrl,
                        true,
                        stoppingToken);
                    if (recipe != null)
                    {
                        db.Recipes.Attach(
                            recipe);
                        db.Authors.Attach(
                            recipe.Author);
                        db.RecipeSteps.AttachRange(
                            recipe.RecipeSteps);
                        db.RecipeStepIngredients.AttachRange(
                            recipe.RecipeSteps
                                .Where(
                                    y =>
                                        y.RecipeIngredients != null)
                                .SelectMany(
                                    y =>
                                        y.RecipeIngredients));
                    }

                    recipeUrl.ProcessingStatus = RecipeUrlStatus.FinishedSuccess;
                }
                catch (TaskCanceledException e)
                {
                    logger.LogError(
                        e.Message,
                        e);
                    recipeUrl.Exception = $"{e.Message}{Environment.NewLine}{e.StackTrace}";
                    recipeUrl.ProcessingStatus = RecipeUrlStatus.FinishedError;
                }
                catch (Exception e)
                {
                    logger.LogError(
                        e.Message,
                        e);
                    recipeUrl.Exception = $"{e.Message}{Environment.NewLine}{e.StackTrace}";
                    recipeUrl.ProcessingStatus = RecipeUrlStatus.FinishedError;
                }
                finally
                {
                    recipeUrl.CompletedAt = DateTimeOffset.UtcNow;
                    await db.SaveChangesAsync(
                        stoppingToken);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}