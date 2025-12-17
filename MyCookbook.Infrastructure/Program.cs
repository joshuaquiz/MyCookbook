using System.IO;
using Microsoft.Extensions.Configuration;
using Environment = System.Environment;

namespace MyCookbook.Infrastructure;

internal sealed class Program
{
    private static void Main()
    {
        var environmentName = Environment.GetEnvironmentVariable("ENVIRONMENT")
                        ?? "Development";
        var rootConfig = new AppSettings();
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("src/appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"src/appsettings.{environmentName}.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build()
            .Bind(rootConfig);
        StackBuilder.Build(
            new Amazon.CDK.App(),
            new StackBuilderProps
            {
                Description = "Infrastructure for the MyCookbook application",
                StackName = $"MyCookbookInfrastructure-{environmentName}",
                Env = new Amazon.CDK.Environment
                {
                    Region = "us-east-1",
                    Account = rootConfig.AccountId
                },
                TerminationProtection = true,
                EnvironmentName = environmentName,
                PublicDomainName = rootConfig.PublicDomainName,
                HostedZoneId = rootConfig.HostedZoneId,
                GithubOidcThumbprint = rootConfig.GithubOidcThumbprint,
                BranchName = rootConfig.BranchName,
                GoogleClientId = rootConfig.GoogleClientId,
                GoogleClientSecret = rootConfig.GoogleClientSecret,
                FacebookAppId = rootConfig.FacebookAppId,
                FacebookAppSecret = rootConfig.FacebookAppSecret
            })
            .Synth();
    }
}