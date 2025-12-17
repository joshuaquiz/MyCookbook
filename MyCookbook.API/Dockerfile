# Optimized multi-stage Dockerfile for MyCookbook API with Alpine Linux

# Base stage with AWS CLI pre-installed
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base

# Install AWS CLI v1 using pip (compatible with Alpine/musl libc)
RUN apk add --no-cache \
    python3 \
    py3-pip \
    bash \
    curl \
    && pip3 install --no-cache-dir awscli --break-system-packages \
    && aws --version

WORKDIR /app
EXPOSE 80

# Build stage - optimized for layer caching
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy only project files first for better caching
COPY ["MyCookbook.API/MyCookbook.API.csproj", "MyCookbook.API/"]
COPY ["MyCookbook.Common/MyCookbook.Common.csproj", "MyCookbook.Common/"]

# Restore dependencies (this layer will be cached if project files don't change)
RUN dotnet restore "MyCookbook.API/MyCookbook.API.csproj"

# Copy source code
COPY MyCookbook.API/ MyCookbook.API/
COPY MyCookbook.Common/ MyCookbook.Common/

# Build
WORKDIR "/src/MyCookbook.API"
RUN dotnet build "MyCookbook.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MyCookbook.API.csproj" -c Release -o /app/publish \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false \
    /p:PublishReadyToRun=true \
    /p:PublishReadyToRunShowWarnings=true

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY MyCookbook.API/update-route53.sh /app/update-route53.sh
COPY MyCookbook.API/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/update-route53.sh /app/entrypoint.sh

# Set ASP.NET Core to listen on port 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["/app/entrypoint.sh"]
