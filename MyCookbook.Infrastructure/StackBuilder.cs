using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Route53;

namespace MyCookbook.Infrastructure;

internal sealed class StackBuilder
{
    public static App Build(
        App app,
        StackBuilderProps stackBuilderProps)
    {
        const string appName = "MyCookbook";
        var infrastructureStack = new Stack(
            app,
            $"{appName}Infrastructure",
            stackBuilderProps);
        var gitHubProvider = new OpenIdConnectProvider(
            infrastructureStack,
            "GithubOidcProvider",
            new OpenIdConnectProviderProps
            {
                Url = "https://token.actions.githubusercontent.com",
                ClientIds =
                [
                    "sts.amazonaws.com"
                ],
                Thumbprints =
                [
                    stackBuilderProps.GithubOidcThumbprint
                ]
            });
        var gitHubRole = new Role(
            infrastructureStack,
            "GithubActionsRole",
            new RoleProps
            {
                AssumedBy = new WebIdentityPrincipal(
                    gitHubProvider.OpenIdConnectProviderArn,
                    new Dictionary<string, object>
                    {
                        {
                            "StringLike",
                            new Dictionary<string, object>
                            {
                                {
                                    "token.actions.githubusercontent.com:sub",
                                    "repo:JoshuaQuiz/*:*"
                                }
                            }
                        },
                        {
                            "StringEquals",
                            new Dictionary<string, object>
                            {
                                {
                                    "token.actions.githubusercontent.com:aud",
                                    "sts.amazonaws.com"
                                }
                            }
                        }
                    }),
                InlinePolicies = new Dictionary<string, PolicyDocument>
                {
                    {
                        "ReadCodeArtifactPolicy",
                        new PolicyDocument(
                            new PolicyDocumentProps
                            {
                                Statements =
                                [
                                    new(
                                        new PolicyStatementProps
                                        {
                                            Effect = Effect.ALLOW,
                                            Actions =
                                            [
                                                "codeartifact:GetAuthorizationToken",
                                                "codeartifact:GetRepositoryEndpoint",
                                                "codeartifact:ReadFromRepository"
                                            ],
                                            Resources =
                                            [
                                                "*"
                                            ]
                                        }),
                                    new(
                                        new PolicyStatementProps
                                        {
                                            Effect = Effect.ALLOW,
                                            Actions =
                                            [
                                                "sts:GetServiceBearerToken"
                                            ],
                                            Resources =
                                            [
                                                "*"
                                            ]
                                        })
                                ]
                            }
                        )
                    }
                }
            });
        var vpc = new Vpc(
            infrastructureStack,
            $"{appName}Vpc",
            new VpcProps
            {
                EnableDnsSupport = true,
                EnableDnsHostnames = true,
                DefaultInstanceTenancy = DefaultInstanceTenancy.DEFAULT,
                MaxAzs = 2,
                NatGateways = 0,
                SubnetConfiguration =
                [
                    new SubnetConfiguration
                    {
                        Name = $"{appName} Public",
                        SubnetType = SubnetType.PUBLIC
                    }
                ]
            });
        var publicHostedZone = HostedZone.FromLookup(
            infrastructureStack,
            $"{appName}PublicHostedZone",
            new HostedZoneProviderProps
            {
                DomainName = stackBuilderProps.PublicDomainName
            });
        var publicCert = new DnsValidatedCertificate(
            infrastructureStack,
            $"{appName}PublicCertificate",
            new DnsValidatedCertificateProps
            {
                DomainName = $"*.{stackBuilderProps.PublicDomainName}",
                HostedZone = publicHostedZone,
                Region = vpc.Stack.Region,
                Validation = CertificateValidation.FromDns(publicHostedZone)
            });

        Tags.Of(infrastructureStack)
            .Add("Application", appName);
        return app;
    }
}