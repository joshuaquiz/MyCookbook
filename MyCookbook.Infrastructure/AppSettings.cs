namespace MyCookbook.Infrastructure;

internal sealed class AppSettings
{
    public string GithubOidcThumbprint { get; set; }

    public string PublicDomainName { get; set; }

    public string HostedZoneId { get; set; }

    public string BranchName { get; set; }

    public string AccountId { get; set; }

    public string GoogleClientId { get; set; }

    public string GoogleClientSecret { get; set; }

    public string FacebookAppId { get; set; }

    public string FacebookAppSecret { get; set; }
}
