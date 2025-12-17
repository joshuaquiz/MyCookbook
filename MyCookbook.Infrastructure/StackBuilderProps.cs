using Amazon.CDK;

namespace MyCookbook.Infrastructure;

internal sealed class StackBuilderProps : StackProps
{
    public string EnvironmentName { get; set; } = string.Empty;

    public string GithubOidcThumbprint { get; set; } = string.Empty;

    public string PublicDomainName { get; set; } = string.Empty;

    public string BranchName { get; set; } = string.Empty;

    public string HostedZoneId { get; set; } = string.Empty;
}
