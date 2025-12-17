using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.ApplicationAutoScaling;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.ResourceGroups;
using Amazon.CDK.AWS.Route53;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SecretsManager;

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

        // Import existing VPC
        var vpc = Vpc.FromLookup(
            infrastructureStack,
            $"{appName}Vpc",
            new VpcLookupOptions
            {
                VpcId = "vpc-07bdc77ff3b2da50a"
            });

        // S3 Config Bucket with Intelligent-Tiering
        var configBucket = new Bucket(
            infrastructureStack,
            $"{appName}ConfigBucket",
            new BucketProps
            {
                BucketName = $"g3-mycookbook-config-{stackBuilderProps.EnvironmentName.ToLower()}",
                Versioned = true,
                Encryption = BucketEncryption.S3_MANAGED,
                RemovalPolicy = RemovalPolicy.RETAIN,
                IntelligentTieringConfigurations =
                [
                    new IntelligentTieringConfiguration
                    {
                        Name = "ArchiveConfig",
                        ArchiveAccessTierTime = Duration.Days(90),
                        DeepArchiveAccessTierTime = Duration.Days(180)
                    }
                ]
            });

        // Import existing Aurora Security Group
        var auroraSecurityGroup = SecurityGroup.FromSecurityGroupId(
            infrastructureStack,
            $"{appName}AuroraSecurityGroup",
            "sg-05f5fe0698ce4cf44",
            new SecurityGroupImportOptions
            {
                Mutable = true
            });

        // Import existing Aurora Serverless v2 Cluster
        var auroraCluster = DatabaseCluster.FromDatabaseClusterAttributes(
            infrastructureStack,
            $"{appName}AuroraCluster",
            new DatabaseClusterAttributes
            {
                ClusterIdentifier = "my-cookbook",
                ClusterEndpointAddress = "my-cookbook.cluster-cvf8xttiq5pt.us-east-1.rds.amazonaws.com",
                Port = 5432,
                SecurityGroups = [auroraSecurityGroup]
            });

        // Import existing database secret
        var dbSecret = Amazon.CDK.AWS.SecretsManager.Secret.FromSecretNameV2(
            infrastructureStack,
            $"{appName}DbSecret",
            "mycookbook/db/development");

        // Cognito User Pool
        var userPool = new UserPool(
            infrastructureStack,
            $"{appName}UserPool",
            new UserPoolProps
            {
                UserPoolName = $"{appName}UserPool-{stackBuilderProps.EnvironmentName}",
                SelfSignUpEnabled = true,
                SignInAliases = new SignInAliases { Email = true },
                AutoVerify = new AutoVerifiedAttrs { Email = true },
                StandardAttributes = new StandardAttributes
                {
                    Email = new StandardAttribute { Required = true, Mutable = true }
                },
                PasswordPolicy = new PasswordPolicy
                {
                    MinLength = 8,
                    RequireLowercase = true,
                    RequireUppercase = true,
                    RequireDigits = true,
                    RequireSymbols = true
                },
                AccountRecovery = AccountRecovery.EMAIL_ONLY,
                RemovalPolicy = RemovalPolicy.RETAIN
            });

        // Cognito User Pool Domain
        var userPoolDomain = userPool.AddDomain(
            $"{appName}UserPoolDomain",
            new UserPoolDomainOptions
            {
                CognitoDomain = new CognitoDomainOptions
                {
                    DomainPrefix = $"g3-mycookbook-{stackBuilderProps.EnvironmentName.ToLower()}"
                }
            });

        // Cognito User Pool Client
        var userPoolClient = new UserPoolClient(
            infrastructureStack,
            $"{appName}UserPoolClient",
            new UserPoolClientProps
            {
                UserPool = userPool,
                UserPoolClientName = $"{appName}-{stackBuilderProps.EnvironmentName}-Client",
                GenerateSecret = false,
                AuthFlows = new AuthFlow
                {
                    UserPassword = true,
                    UserSrp = true
                },
                OAuth = new OAuthSettings
                {
                    Flows = new OAuthFlows
                    {
                        AuthorizationCodeGrant = true
                    },
                    Scopes = [OAuthScope.EMAIL, OAuthScope.OPENID, OAuthScope.PROFILE],
                    CallbackUrls = ["myapp://callback", "http://localhost:3000/callback"]
                },
                SupportedIdentityProviders = [UserPoolClientIdentityProvider.COGNITO]
            });

        // Google Identity Provider (only if credentials are provided)
        if (!string.IsNullOrEmpty(stackBuilderProps.GoogleClientId) &&
            !stackBuilderProps.GoogleClientId.StartsWith("YOUR_") &&
            !string.IsNullOrEmpty(stackBuilderProps.GoogleClientSecret) &&
            !stackBuilderProps.GoogleClientSecret.StartsWith("YOUR_"))
        {
            new UserPoolIdentityProviderGoogle(
                infrastructureStack,
                $"{appName}GoogleProvider",
                new UserPoolIdentityProviderGoogleProps
                {
                    UserPool = userPool,
                    ClientId = stackBuilderProps.GoogleClientId,
                    ClientSecretValue = SecretValue.UnsafePlainText(stackBuilderProps.GoogleClientSecret),
                    Scopes = ["profile", "email", "openid"],
                    AttributeMapping = new AttributeMapping
                    {
                        Email = ProviderAttribute.GOOGLE_EMAIL,
                        GivenName = ProviderAttribute.GOOGLE_GIVEN_NAME,
                        FamilyName = ProviderAttribute.GOOGLE_FAMILY_NAME
                    }
                });
        }

        // Facebook Identity Provider (only if credentials are provided)
        if (!string.IsNullOrEmpty(stackBuilderProps.FacebookAppId) &&
            !stackBuilderProps.FacebookAppId.StartsWith("YOUR_") &&
            !string.IsNullOrEmpty(stackBuilderProps.FacebookAppSecret) &&
            !stackBuilderProps.FacebookAppSecret.StartsWith("YOUR_"))
        {
            new UserPoolIdentityProviderFacebook(
                infrastructureStack,
                $"{appName}FacebookProvider",
                new UserPoolIdentityProviderFacebookProps
                {
                    UserPool = userPool,
                    ClientId = stackBuilderProps.FacebookAppId,
                    ClientSecret = stackBuilderProps.FacebookAppSecret,
                    Scopes = ["public_profile", "email"],
                    AttributeMapping = new AttributeMapping
                    {
                        Email = ProviderAttribute.FACEBOOK_EMAIL,
                        GivenName = ProviderAttribute.FACEBOOK_NAME
                    }
                });
        }

        // Lookup ECR repository for API container image
        var ecrRepository = Repository.FromRepositoryName(
            infrastructureStack,
            $"{appName}ApiEcrRepository",
            "mycookbook-api");

        // ECS Cluster
        var cluster = new Cluster(
            infrastructureStack,
            $"{appName}Cluster",
            new ClusterProps
            {
                Vpc = vpc,
                ClusterName = $"{appName}Cluster-{stackBuilderProps.EnvironmentName}"
            });

        // Fargate Task Role
        var fargateTaskRole = new Role(
            infrastructureStack,
            $"{appName}FargateTaskRole",
            new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
            });

        // Grant Fargate access to Secrets Manager
        dbSecret.GrantRead(fargateTaskRole);

        // Grant Fargate access to Route53 and EC2 (for Route53 DNS update script)
        fargateTaskRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = [
                "route53:ChangeResourceRecordSets",
                "route53:GetChange",
                "ecs:DescribeTasks",
                "ec2:DescribeNetworkInterfaces"
            ],
            Resources = ["*"]
        }));

        // Fargate Task Execution Role
        var fargateTaskExecutionRole = new Role(
            infrastructureStack,
            $"{appName}FargateTaskExecutionRole",
            new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
                ManagedPolicies =
                [
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonECSTaskExecutionRolePolicy")
                ]
            });

        // Fargate Security Group
        var fargateSecurityGroup = new SecurityGroup(
            infrastructureStack,
            $"{appName}FargateSecurityGroup",
            new SecurityGroupProps
            {
                Vpc = vpc,
                Description = "Security group for Fargate tasks",
                AllowAllOutbound = true
            });

        // Allow Fargate to access Aurora
        auroraSecurityGroup.AddIngressRule(
            fargateSecurityGroup,
            Port.Tcp(5432),
            "Allow Fargate access to Aurora");

        // Allow HTTP traffic to Fargate
        fargateSecurityGroup.AddIngressRule(
            Peer.AnyIpv4(),
            Port.Tcp(80),
            "Allow HTTP traffic");

        // Fargate Task Definition
        var taskDefinition = new FargateTaskDefinition(
            infrastructureStack,
            $"{appName}ApiTaskDefinition",
            new FargateTaskDefinitionProps
            {
                MemoryLimitMiB = 512,
                Cpu = 256,
                TaskRole = fargateTaskRole,
                ExecutionRole = fargateTaskExecutionRole
            });

        // Add container to task definition
        var container = taskDefinition.AddContainer(
            $"{appName}ApiContainer",
            new ContainerDefinitionOptions
            {
                Image = ContainerImage.FromEcrRepository(ecrRepository, "latest"),
                Logging = LogDriver.AwsLogs(new AwsLogDriverProps
                {
                    StreamPrefix = "api",
                    LogGroup = new LogGroup(
                        infrastructureStack,
                        $"{appName}ApiLogGroup",
                        new LogGroupProps
                        {
                            LogGroupName = $"/ecs/{appName}-api-{stackBuilderProps.EnvironmentName.ToLower()}",
                            Retention = RetentionDays.THREE_DAYS,
                            RemovalPolicy = RemovalPolicy.DESTROY
                        })
                }),
                Environment = new Dictionary<string, string>
                {
                    { "ASPNETCORE_ENVIRONMENT", stackBuilderProps.EnvironmentName },
                    { "DB_SECRET_ARN", dbSecret.SecretArn },
                    { "DB_HOST", auroraCluster.ClusterEndpoint.Hostname },
                    { "DB_NAME", "mycookbook" },
                    { "DB_PORT", "5432" },
                    { "HOSTED_ZONE_ID", stackBuilderProps.HostedZoneId },
                    { "DNS_RECORD_NAME", $"api-{stackBuilderProps.EnvironmentName.ToLower()}-mycookbook.{stackBuilderProps.PublicDomainName}" },
                    { "AWS_REGION", "us-east-1" }
                }
            });

        container.AddPortMappings(new PortMapping
        {
            ContainerPort = 80,
            Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP
        });

        // Fargate Service
        var fargateService = new FargateService(
            infrastructureStack,
            $"{appName}ApiServiceV2",
            new FargateServiceProps
            {
                Cluster = cluster,
                TaskDefinition = taskDefinition,
                DesiredCount = 0,
                AssignPublicIp = true,
                SecurityGroups = [fargateSecurityGroup],
                VpcSubnets = new SubnetSelection { SubnetType = SubnetType.PUBLIC }
            });

        // Auto Scaling based on schedule
        var scalableTarget = fargateService.AutoScaleTaskCount(new EnableScalingProps
        {
            MinCapacity = 0,
            MaxCapacity = 1
        });

        // Scale up: Weekdays 9:30 AM EST
        scalableTarget.ScaleOnSchedule($"{appName}ScaleUpMorning", new ScalingSchedule
        {
            Schedule = Schedule.Cron(new CronOptions { Minute = "30", Hour = "14", WeekDay = "MON-FRI" }),
            MinCapacity = 1,
            MaxCapacity = 1
        });

        // Scale down: Weekdays 12:00 PM EST
        scalableTarget.ScaleOnSchedule($"{appName}ScaleDownNoon", new ScalingSchedule
        {
            Schedule = Schedule.Cron(new CronOptions { Minute = "0", Hour = "17", WeekDay = "MON-FRI" }),
            MinCapacity = 0,
            MaxCapacity = 0
        });

        // Scale up: Weekdays 3:00 PM EST
        scalableTarget.ScaleOnSchedule($"{appName}ScaleUpAfternoon", new ScalingSchedule
        {
            Schedule = Schedule.Cron(new CronOptions { Minute = "0", Hour = "20", WeekDay = "MON-FRI" }),
            MinCapacity = 1,
            MaxCapacity = 1
        });

        // Scale down: Weekdays 6:00 PM EST
        scalableTarget.ScaleOnSchedule($"{appName}ScaleDownEvening", new ScalingSchedule
        {
            Schedule = Schedule.Cron(new CronOptions { Minute = "0", Hour = "23", WeekDay = "MON-FRI" }),
            MinCapacity = 0,
            MaxCapacity = 0
        });

        // CloudFormation Outputs
        new CfnOutput(
            infrastructureStack,
            "VpcId",
            new CfnOutputProps
            {
                Value = vpc.VpcId,
                Description = "VPC ID"
            });

        new CfnOutput(
            infrastructureStack,
            "AuroraClusterEndpoint",
            new CfnOutputProps
            {
                Value = auroraCluster.ClusterEndpoint.Hostname,
                Description = "Aurora cluster endpoint"
            });

        new CfnOutput(
            infrastructureStack,
            "DbSecretArn",
            new CfnOutputProps
            {
                Value = dbSecret.SecretArn,
                Description = "ARN of the secret containing database credentials"
            });

        new CfnOutput(
            infrastructureStack,
            "UserPoolId",
            new CfnOutputProps
            {
                Value = userPool.UserPoolId,
                Description = "Cognito User Pool ID"
            });

        new CfnOutput(
            infrastructureStack,
            "UserPoolClientId",
            new CfnOutputProps
            {
                Value = userPoolClient.UserPoolClientId,
                Description = "Cognito User Pool Client ID"
            });

        new CfnOutput(
            infrastructureStack,
            "UserPoolDomain",
            new CfnOutputProps
            {
                Value = userPoolDomain.DomainName,
                Description = "Cognito User Pool Domain"
            });

        new CfnOutput(
            infrastructureStack,
            "CognitoHostedUIUrl",
            new CfnOutputProps
            {
                Value = $"https://{userPoolDomain.DomainName}.auth.us-east-1.amazoncognito.com",
                Description = "Cognito Hosted UI URL"
            });

        new CfnOutput(
            infrastructureStack,
            "EcsClusterName",
            new CfnOutputProps
            {
                Value = cluster.ClusterName,
                Description = "ECS Cluster Name"
            });

        new CfnOutput(
            infrastructureStack,
            "FargateServiceName",
            new CfnOutputProps
            {
                Value = fargateService.ServiceName,
                Description = "Fargate Service Name"
            });

        new CfnOutput(
            infrastructureStack,
            "ConfigBucketName",
            new CfnOutputProps
            {
                Value = configBucket.BucketName,
                Description = "S3 bucket for configuration files"
            });

        new CfnOutput(
            infrastructureStack,
            "ApiUrl",
            new CfnOutputProps
            {
                Value = $"http://api-{stackBuilderProps.EnvironmentName.ToLower()}-mycookbook.{stackBuilderProps.PublicDomainName}",
                Description = "API URL (DNS updated automatically by Fargate task on startup)"
            });

        // Create Resource Group for all MyCookbook resources
        new Amazon.CDK.AWS.ResourceGroups.CfnGroup(
            infrastructureStack,
            $"{appName}ResourceGroup",
            new Amazon.CDK.AWS.ResourceGroups.CfnGroupProps
            {
                Name = $"{appName}-{stackBuilderProps.EnvironmentName}",
                Description = $"Resource group for {appName} application in {stackBuilderProps.EnvironmentName} environment",
                ResourceQuery = new Amazon.CDK.AWS.ResourceGroups.CfnGroup.ResourceQueryProperty
                {
                    Type = "TAG_FILTERS_1_0",
                    Query = new Amazon.CDK.AWS.ResourceGroups.CfnGroup.QueryProperty
                    {
                        ResourceTypeFilters = new[] { "AWS::AllSupported" },
                        TagFilters = new object[]
                        {
                            new Amazon.CDK.AWS.ResourceGroups.CfnGroup.TagFilterProperty
                            {
                                Key = "Application",
                                Values = new[] { appName }
                            }
                        }
                    }
                }
            });

        // Apply tags to all resources in the stack
        Tags.Of(infrastructureStack)
            .Add("Application", appName);

        return app;
    }
}

