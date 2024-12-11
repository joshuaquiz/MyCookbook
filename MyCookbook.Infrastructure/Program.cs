using System.IO;
using Amazon.CDK;
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
            new App(),
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
                GithubOidcThumbprint = rootConfig.GithubOidcThumbprint,
                BranchName = rootConfig.BranchName
            })
            .Synth();
    }
}