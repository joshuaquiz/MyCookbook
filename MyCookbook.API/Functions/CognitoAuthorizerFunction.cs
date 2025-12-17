using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace MyCookbook.API.Functions;

public class CognitoAuthorizerFunction
{
    private readonly ILogger<CognitoAuthorizerFunction> _logger;
    private readonly string _userPoolId;
    private readonly string _region;
    private static readonly HttpClient HttpClient = new();
    private static JsonWebKeySet? _jwks;

    public CognitoAuthorizerFunction()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder => builder.AddLambdaLogger());
        var serviceProvider = serviceCollection.BuildServiceProvider();
        _logger = serviceProvider.GetRequiredService<ILogger<CognitoAuthorizerFunction>>();

        _userPoolId = Environment.GetEnvironmentVariable("USER_POOL_ID") ?? throw new InvalidOperationException("USER_POOL_ID not set");
        _region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";
    }

    public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandler(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
    {
        try
        {
            _logger.LogInformation("Authorizer request received");

            var token = ExtractToken(request);
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No token found in request");
                return GenerateDenyPolicy("user", request.MethodArn);
            }

            var principal = await ValidateToken(token);
            if (principal == null)
            {
                _logger.LogWarning("Token validation failed");
                return GenerateDenyPolicy("user", request.MethodArn);
            }

            var userId = principal.FindFirst("sub")?.Value ?? "unknown";
            var email = principal.FindFirst("email")?.Value;
            var username = principal.FindFirst("cognito:username")?.Value;

            _logger.LogInformation($"Token validated for user: {userId}");

            var authContext = new APIGatewayCustomAuthorizerContextOutput();
            authContext["userId"] = userId;
            authContext["email"] = email ?? "";
            authContext["username"] = username ?? "";

            return GenerateAllowPolicy(userId, request.MethodArn, authContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in authorizer");
            return GenerateDenyPolicy("user", request.MethodArn);
        }
    }

    private string? ExtractToken(APIGatewayCustomAuthorizerRequest request)
    {
        if (request.Headers != null && request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring(7);
            }
        }

        if (request.QueryStringParameters != null && request.QueryStringParameters.TryGetValue("token", out var tokenParam))
        {
            return tokenParam;
        }

        return null;
    }

    private async Task<ClaimsPrincipal?> ValidateToken(string token)
    {
        try
        {
            // Get JWKS if not cached
            if (_jwks == null)
            {
                var jwksUrl = $"https://cognito-idp.{_region}.amazonaws.com/{_userPoolId}/.well-known/jwks.json";
                var jwksJson = await HttpClient.GetStringAsync(jwksUrl);
                _jwks = new JsonWebKeySet(jwksJson);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Find the signing key
            var kid = jwtToken.Header.Kid;
            var signingKey = _jwks.Keys.FirstOrDefault(k => k.Kid == kid);

            if (signingKey == null)
            {
                _logger.LogWarning($"Signing key not found for kid: {kid}");
                return null;
            }

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = $"https://cognito-idp.{_region}.amazonaws.com/{_userPoolId}",
                ValidateAudience = false, // Cognito doesn't use audience
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation error");
            return null;
        }
    }

    private APIGatewayCustomAuthorizerResponse GenerateAllowPolicy(string principalId, string resource, APIGatewayCustomAuthorizerContextOutput? context = null)
    {
        return new APIGatewayCustomAuthorizerResponse
        {
            PrincipalID = principalId,
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                {
                    new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                    {
                        Action = new HashSet<string> { "execute-api:Invoke" },
                        Effect = "Allow",
                        Resource = new HashSet<string> { resource }
                    }
                }
            },
            Context = context ?? new APIGatewayCustomAuthorizerContextOutput()
        };
    }

    private APIGatewayCustomAuthorizerResponse GenerateDenyPolicy(string principalId, string resource)
    {
        return new APIGatewayCustomAuthorizerResponse
        {
            PrincipalID = principalId,
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                {
                    new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                    {
                        Action = new HashSet<string> { "execute-api:Invoke" },
                        Effect = "Deny",
                        Resource = new HashSet<string> { resource }
                    }
                }
            }
        };
    }
}

