namespace MyCookbook.Infrastructure;

internal sealed class AppSettings
{
    public string GithubOidcThumbprint { get; set; }

    public string PublicDomainName { get; set; }

    public string BranchName { get; set; }

    public string AccountId { get; set; }
}