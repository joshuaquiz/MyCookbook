FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN ls

# Restore dependencies
RUN dotnet restore "MyCookbook.Lambda/MyCookbook.Lambda.csproj"

# Build and publish
WORKDIR "/src/MyCookbook.Lambda"
RUN dotnet publish "MyCookbook.Lambda.csproj" -c Release -r linux-x64 --self-contained false -o /app/publish

# Runtime stage - use AWS Lambda .NET 10 base image
FROM public.ecr.aws/lambda/dotnet:10-preview AS runtime

# Copy the published application
COPY --from=build /app/publish ${LAMBDA_TASK_ROOT}

