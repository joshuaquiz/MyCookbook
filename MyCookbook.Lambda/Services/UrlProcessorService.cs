using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyCookbook.API.Interfaces;
using MyCookbook.Common.Database;
using MyCookbook.Lambda.Interfaces;

namespace MyCookbook.Lambda.Services;

public class UrlProcessorService : IUrlProcessorService
{
    private readonly IS3Service _s3Service;
    private readonly ILdJsonExtractor _ldJsonExtractor;
    private readonly IUrlLdJsonDataNormalizer _urlLdJsonDataNormalizer;
    private readonly IRecipeWebSiteWrapperProcessor _recipeWebSiteWrapperProcessor;
    private readonly ILogger<UrlProcessorService> _logger;
    private readonly string _dbBucketName;
    private readonly string _dbKey;

    public UrlProcessorService(
        IS3Service s3Service,
        ILdJsonExtractor ldJsonExtractor,
        IUrlLdJsonDataNormalizer urlLdJsonDataNormalizer,
        IRecipeWebSiteWrapperProcessor recipeWebSiteWrapperProcessor,
        ILogger<UrlProcessorService> logger)
    {
        _s3Service = s3Service;
        _ldJsonExtractor = ldJsonExtractor;
        _urlLdJsonDataNormalizer = urlLdJsonDataNormalizer;
        _recipeWebSiteWrapperProcessor = recipeWebSiteWrapperProcessor;
        _logger = logger;
        _dbBucketName = Environment.GetEnvironmentVariable("DB_BUCKET_NAME") ?? "g3-cookbook-db-files";
        _dbKey = Environment.GetEnvironmentVariable("DB_KEY") ?? "MyCookbook.db";
    }

    public async Task ProcessUrlAsync(
        Uri url,
        CancellationToken cancellationToken)
    {
        string? dbFilePath = null;
        try
        {
            // Download the SQLite database from S3
            _logger.LogInformation("Downloading database from S3: {Bucket}/{Key}", _dbBucketName, _dbKey);
            dbFilePath = await _s3Service.DownloadFileToTempAsync(_dbBucketName, _dbKey, cancellationToken);
            _logger.LogInformation("Database downloaded to: {Path}", dbFilePath);

            // Create DbContext with the downloaded database
            var optionsBuilder = new DbContextOptionsBuilder<MyCookbookContext>();
            optionsBuilder.UseSqlite($"Data Source={dbFilePath};");
            
            await using var db = new MyCookbookContext(optionsBuilder.Options);
            
            // Check if URL already exists
            var existingDataSource = await db.RawDataSources
                .FirstOrDefaultAsync(x => x.Url == url, cancellationToken);

            if (existingDataSource == null)
            {
                // Add new data source
                existingDataSource = new RawDataSource
                {
                    SourceId = Guid.NewGuid(),
                    ProcessingStatus = RecipeUrlStatus.NotStarted,
                    Url = url,
                    UrlHost = url.Host
                };
                await db.RawDataSources.AddAsync(existingDataSource, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }

            // Process the URL
            _logger.LogInformation("Starting to process {Url}", url);
            existingDataSource.ProcessingStatus = RecipeUrlStatus.Downloading;
            existingDataSource.ParserVersion = Enum.GetValues<ParserVersion>().Max();
            await db.SaveChangesAsync(cancellationToken);

            try
            {
                var results = await _ldJsonExtractor.ExtractLdJsonItems(
                    url,
                    existingDataSource.RawHtml,
                    cancellationToken);
                
                existingDataSource.RawHtml = results.RawHtml;
                var jsonSections = System.Text.Json.JsonSerializer.Serialize(results.Data);
                _logger.LogInformation("{Url} - extracted {Count} ld+json sections", url, results.Data.Count);
                existingDataSource.LdJsonData = jsonSections;
                existingDataSource.ProcessingStatus = RecipeUrlStatus.DownloadSucceeded;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error downloading URL: {Url}", url);
                existingDataSource.Error = e.ToString();
                existingDataSource.ProcessingStatus = RecipeUrlStatus.DownloadFailed;
            }
            finally
            {
                existingDataSource.ProcessedDatetime = DateTime.Now;
            }

            await db.SaveChangesAsync(cancellationToken);

            // Parse the downloaded data
            try
            {
                var siteWrapper = await _urlLdJsonDataNormalizer.NormalizeParsedLdJsonData(
                    db,
                    existingDataSource,
                    cancellationToken);
                await _recipeWebSiteWrapperProcessor.ProcessWrapper(
                    db,
                    existingDataSource,
                    siteWrapper,
                    cancellationToken);
                existingDataSource.ProcessingStatus = RecipeUrlStatus.FinishedSuccess;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error parsing URL: {url}");
                existingDataSource.Error = e.ToString();
                existingDataSource.ProcessingStatus = RecipeUrlStatus.FinishedError;
            }
            finally
            {
                await db.SaveChangesAsync(cancellationToken);
            }

            // Upload the modified database back to S3
            _logger.LogInformation("Uploading modified database to S3: {Bucket}/{Key}", _dbBucketName, _dbKey);
            await _s3Service.UploadFileAsync(_dbBucketName, _dbKey, dbFilePath, cancellationToken);
            _logger.LogInformation("Database uploaded successfully");
        }
        finally
        {
            // Clean up temp file
            if (dbFilePath != null && File.Exists(dbFilePath))
            {
                File.Delete(dbFilePath);
            }
        }
    }
}

