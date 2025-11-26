using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;

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
                                    new PolicyStatement(
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
                                    new PolicyStatement(
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
        /*var vpc = new Vpc(
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
            });*/

        // S3 Buckets
        var configBucket = new Bucket(
            infrastructureStack,
            $"{appName}ConfigBucket",
            new BucketProps
            {
                BucketName = $"g3-mycookbook-config-{stackBuilderProps.EnvironmentName.ToLower()}",
                Versioned = true,
                Encryption = BucketEncryption.S3_MANAGED,
                RemovalPolicy = RemovalPolicy.RETAIN
            });

        var dbBucket = Bucket.FromBucketName(
            infrastructureStack,
            $"{appName}DbBucket",
            "g3-cookbook-db-files");

        // Lambda Execution Role
        var lambdaExecutionRole = new Role(
            infrastructureStack,
            $"{appName}LambdaExecutionRole",
            new RoleProps
            {
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies =
                [
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole")
                ],
                InlinePolicies = new Dictionary<string, PolicyDocument>
                {
                    {
                        "S3AccessPolicy",
                        new PolicyDocument(
                            new PolicyDocumentProps
                            {
                                Statements =
                                [
                                    new PolicyStatement(
                                        new PolicyStatementProps
                                        {
                                            Effect = Effect.ALLOW,
                                            Actions =
                                            [
                                                "s3:GetObject",
                                                "s3:PutObject"
                                            ],
                                            Resources =
                                            [
                                                configBucket.BucketArn + "/*",
                                                dbBucket.BucketArn + "/*"
                                            ]
                                        })
                                ]
                            })
                    }
                }
            });

        // Auth Lambda Function (using Docker container)
        var authFunction = new DockerImageFunction(
            infrastructureStack,
            $"{appName}AuthFunction",
            new DockerImageFunctionProps
            {
                Code = DockerImageCode.FromImageAsset("..", new AssetImageCodeProps
                {
                    Cmd = ["MyCookbook.Lambda::MyCookbook.Lambda.Functions.AuthFunction::FunctionHandler"],
                    File = "MyCookbook.Lambda/Dockerfile",
                    BuildArgs = new Dictionary<string, string>()
                }),
                MemorySize = 512,
                Timeout = Duration.Seconds(30),
                Role = lambdaExecutionRole,
                Environment = new Dictionary<string, string>
                {
                    { "CONFIG_BUCKET_NAME", configBucket.BucketName },
                    { "CONFIG_KEY", "auth-config.json" },
                    { "DB_BUCKET_NAME", dbBucket.BucketName },
                    { "DB_KEY", "MyCookbook.db" }
                }
            });

        // URL Processor Lambda Function (using Docker container)
        var urlProcessorFunction = new DockerImageFunction(
            infrastructureStack,
            $"{appName}UrlProcessorFunction",
            new DockerImageFunctionProps
            {
                Code = DockerImageCode.FromImageAsset("..", new AssetImageCodeProps
                {
                    Cmd = ["MyCookbook.Lambda::MyCookbook.Lambda.Functions.UrlProcessorFunction::FunctionHandler"],
                    File = "MyCookbook.Lambda/Dockerfile",
                    BuildArgs = new Dictionary<string, string>()
                }),
                MemorySize = 1024,
                Timeout = Duration.Seconds(300),
                Role = lambdaExecutionRole,
                Environment = new Dictionary<string, string>
                {
                    { "DB_BUCKET_NAME", dbBucket.BucketName },
                    { "DB_KEY", "MyCookbook.db" }
                }
            });

        // API Gateway
        var api = new RestApi(
            infrastructureStack,
            $"{appName}LambdaApi",
            new RestApiProps
            {
                RestApiName = $"{appName} Lambda API",
                Description = "API Gateway for MyCookbook Lambda functions",
                DeployOptions = new StageOptions
                {
                    StageName = stackBuilderProps.EnvironmentName.ToLower()
                }
            });

        // Auth endpoint
        var authResource = api.Root.AddResource("auth");
        authResource.AddMethod(
            "POST",
            new LambdaIntegration(authFunction));

        // Process URL endpoint
        var processUrlResource = api.Root.AddResource("process-url");
        processUrlResource.AddMethod(
            "POST",
            new LambdaIntegration(urlProcessorFunction));

        // Outputs
        new CfnOutput(
            infrastructureStack,
            "ApiUrl",
            new CfnOutputProps
            {
                Value = api.Url,
                Description = "API Gateway URL"
            });

        new CfnOutput(
            infrastructureStack,
            "ConfigBucketName",
            new CfnOutputProps
            {
                Value = configBucket.BucketName,
                Description = "S3 bucket for configuration files"
            });

        Tags.Of(infrastructureStack)
            .Add("Application", appName);
        return app;
    }
}
