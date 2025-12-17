# OAuth Provider Setup Guide

This guide explains how to configure OAuth providers (Google and Facebook) for the MyCookbook application using AWS Secrets Manager.

## Prerequisites

- AWS CLI configured with appropriate credentials
- Access to AWS Secrets Manager in your account
- Google OAuth credentials (Client ID and Client Secret)
- Facebook OAuth credentials (App ID and App Secret)

## Setup Instructions

### 1. Create Secrets in AWS Secrets Manager

The CDK infrastructure expects OAuth credentials to be stored in AWS Secrets Manager with specific secret names and JSON structure.

#### Google OAuth Secret

Create a secret named `mycookbook/oauth/google` with the following JSON structure:

```bash
aws secretsmanager create-secret \
  --name "mycookbook/oauth/google" \
  --description "Google OAuth credentials for MyCookbook" \
  --secret-string '{"client_id":"YOUR_GOOGLE_CLIENT_ID","client_secret":"YOUR_GOOGLE_CLIENT_SECRET"}' \
  --region us-east-1
```



#### Facebook OAuth Secret

Create a secret named `mycookbook/oauth/facebook` with the following JSON structure:

```bash
aws secretsmanager create-secret \
  --name "mycookbook/oauth/facebook" \
  --description "Facebook OAuth credentials for MyCookbook" \
  --secret-string '{"client_id":"YOUR_FACEBOOK_APP_ID","client_secret":"YOUR_FACEBOOK_APP_SECRET"}' \
  --region us-east-1
```



### 2. Verify Secrets

Verify that the secrets were created successfully:

```bash
# List all secrets
aws secretsmanager list-secrets --region us-east-1

# Get Google OAuth secret
aws secretsmanager get-secret-value --secret-id "mycookbook/oauth/google" --region us-east-1

# Get Facebook OAuth secret
aws secretsmanager get-secret-value --secret-id "mycookbook/oauth/facebook" --region us-east-1
```

### 3. Update Secrets (if needed)

If you need to update the credentials later:

```bash
# Update Google OAuth secret
aws secretsmanager update-secret \
  --secret-id "mycookbook/oauth/google" \
  --secret-string '{"client_id":"NEW_CLIENT_ID","client_secret":"NEW_CLIENT_SECRET"}' \
  --region us-east-1

# Update Facebook OAuth secret
aws secretsmanager update-secret \
  --secret-id "mycookbook/oauth/facebook" \
  --secret-string '{"client_id":"NEW_APP_ID","client_secret":"NEW_APP_SECRET"}' \
  --region us-east-1
```

### 4. Deploy Infrastructure

Once the secrets are created, deploy the CDK infrastructure:

```bash
cd MyCookbook.Infrastructure
cdk deploy --all
```

The CDK code will automatically retrieve the OAuth credentials from AWS Secrets Manager during deployment.

## How It Works

The CDK infrastructure (`StackBuilder.cs`) uses the following code to retrieve secrets:

```csharp
// Google OAuth
var googleSecret = Amazon.CDK.AWS.SecretsManager.Secret.FromSecretNameV2(
    infrastructureStack,
    $"{appName}GoogleOAuthSecret",
    "mycookbook/oauth/google");

// Facebook OAuth
var facebookSecret = Amazon.CDK.AWS.SecretsManager.Secret.FromSecretNameV2(
    infrastructureStack,
    $"{appName}FacebookOAuthSecret",
    "mycookbook/oauth/facebook");
```

The secrets are then used to configure the Cognito User Pool Identity Providers.

## Security Benefits

✅ **No credentials in source code** - OAuth credentials are never committed to Git  
✅ **Centralized secret management** - All secrets managed in AWS Secrets Manager  
✅ **Automatic rotation support** - Can enable automatic secret rotation in AWS  
✅ **Access control** - IAM policies control who can access secrets  
✅ **Audit trail** - CloudTrail logs all secret access  

## Troubleshooting

### Secret Not Found Error

If you get an error during deployment about secrets not being found:

1. Verify the secret exists: `aws secretsmanager list-secrets --region us-east-1`
2. Check the secret name matches exactly: `mycookbook/oauth/google` and `mycookbook/oauth/facebook`
3. Ensure you're deploying to the same region where secrets are stored (us-east-1)

### Invalid JSON Format

If the secret JSON is malformed, you'll get an error. Ensure the JSON structure is exactly:

```json
{
  "client_id": "your_client_id_here",
  "client_secret": "your_client_secret_here"
}
```

### Permission Denied

If you get permission errors, ensure your AWS credentials have the following permissions:

- `secretsmanager:GetSecretValue`
- `secretsmanager:DescribeSecret`

## Cost

AWS Secrets Manager pricing (as of 2024):
- $0.40 per secret per month
- $0.05 per 10,000 API calls

For this setup: ~$0.80/month for 2 secrets (Google + Facebook)

