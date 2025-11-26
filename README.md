# MyCookbook Lambda Functions

This project contains AWS Lambda functions for the MyCookbook application.

## Functions

### 1. AuthFunction
**Endpoint:** `POST /auth`

Authenticates a user with username and password, and returns two text fields from a JSON configuration file stored in S3.

**Request Body:**
```json
{
  "username": "your-username",
  "password": "your-password"
}
```

**Response (Success - 200):**
```json
{
  "field1": "value1",
  "field2": "value2"
}
```

**Response (Unauthorized - 401):**
```json
{
  "error": "Invalid credentials"
}
```

**Environment Variables:**
- `CONFIG_BUCKET_NAME`: S3 bucket containing the auth configuration file (default: `mycookbook-config`)
- `CONFIG_KEY`: S3 key for the auth configuration file (default: `auth-config.json`)

**S3 Configuration File Format (`auth-config.json`):**
```json
{
  "username": "admin",
  "passwordHash": "BASE64_ENCODED_SHA256_HASH",
  "field1": "value1",
  "field2": "value2"
}
```

To generate a password hash:
```csharp
using System.Security.Cryptography;
using System.Text;

var password = "your-password";
using var sha256 = SHA256.Create();
var bytes = Encoding.UTF8.GetBytes(password);
var hash = sha256.ComputeHash(bytes);
var passwordHash = Convert.ToBase64String(hash);
Console.WriteLine(passwordHash);
```

### 2. UrlProcessorFunction
**Endpoint:** `POST /process-url`

Processes a URL by downloading the SQLite database from S3, adding the URL to the database, processing it using the UrlDownloaderJob logic, and uploading the modified database back to S3.

**Request Body:**
```json
{
  "url": "https://example.com/recipe"
}
```

**Response (Success - 200):**
```json
{
  "message": "URL processed successfully",
  "url": "https://example.com/recipe"
}
```

**Response (Error - 500):**
```json
{
  "error": "Internal server error",
  "details": "Error message"
}
```

**Environment Variables:**
- `DB_BUCKET_NAME`: S3 bucket containing the SQLite database (default: `g3-cookbook-db-files`)
- `DB_KEY`: S3 key for the database file (default: `MyCookbook.db`)

## Deployment

### Prerequisites
1. .NET 9.0 SDK
2. Docker Desktop (required for building container images)
3. AWS CLI configured with appropriate credentials
4. AWS CDK CLI installed

### Build and Deploy

The Lambda functions are now deployed as Docker containers using .NET 9.

#### Using the Build Script (Recommended)

```bash
cd MyCookbook.Lambda
.\build-and-deploy.ps1 -Environment Development
```

This script will:
1. Verify Docker is running
2. Deploy the infrastructure using CDK
3. CDK will automatically build the Docker images and push them to Amazon ECR

#### Manual Deployment

```bash
cd MyCookbook.Infrastructure

# Update src/appsettings.json with your AWS account details
# Then deploy:
cdk deploy
```

The CDK stack will create:
- Two Lambda functions (AuthFunction and UrlProcessorFunction) as Docker containers
- Amazon ECR repositories for the container images
- API Gateway with two endpoints
- S3 bucket for configuration files
- IAM roles with appropriate permissions

### Docker Image Details

The Lambda functions use the official AWS Lambda .NET 9 base image:
- Base image: `public.ecr.aws/lambda/dotnet:9`
- Runtime: .NET 9 (container-only runtime)
- Build: Multi-stage Docker build for optimized image size

## Testing Locally

### Using Docker

You can test the Lambda functions locally using Docker:

```bash
cd MyCookbook.Lambda/src/MyCookbook.Lambda

# Build the Docker image
docker build -t mycookbook-lambda .

# Run the container locally (requires AWS credentials)
docker run -p 9000:8080 \
  -e AWS_ACCESS_KEY_ID=your_access_key \
  -e AWS_SECRET_ACCESS_KEY=your_secret_key \
  -e CONFIG_BUCKET_NAME=mycookbook-config-development \
  -e CONFIG_KEY=auth-config.json \
  mycookbook-lambda

# Test the function
curl -XPOST "http://localhost:9000/2015-03-31/functions/function/invocations" \
  -d '{"body": "{\"username\":\"admin\",\"password\":\"test\"}"}'
```

### Using AWS Lambda Test Tool

You can also test using the AWS Lambda Test Tool:

```bash
cd MyCookbook.Lambda/src/MyCookbook.Lambda
dotnet lambda-test-tool-9.0
```

## S3 Setup

### Create Configuration File

1. Create the auth configuration file locally:
```json
{
  "username": "admin",
  "passwordHash": "YOUR_PASSWORD_HASH_HERE",
  "field1": "value1",
  "field2": "value2"
}
```

2. Upload to S3:
```bash
aws s3 cp auth-config.json s3://mycookbook-config-development/auth-config.json
```

### Database File

The database file should already exist in the `g3-cookbook-db-files` bucket as `MyCookbook.db`. The UrlProcessorFunction will download it, modify it, and upload it back.

## API Gateway Endpoints

After deployment, the API Gateway URL will be output by the CDK stack. You can find it in the CloudFormation outputs or by running:

```bash
aws cloudformation describe-stacks --stack-name MyCookbookInfrastructure-Development --query "Stacks[0].Outputs"
```

Example endpoints:
- Auth: `https://your-api-id.execute-api.us-east-1.amazonaws.com/development/auth`
- Process URL: `https://your-api-id.execute-api.us-east-1.amazonaws.com/development/process-url`

