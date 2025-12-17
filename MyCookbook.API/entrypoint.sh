#!/bin/bash
set -e

echo "Starting MyCookbook API container..."

# Update Route53 DNS record with the task's public IP (run in background but redirect output)
(/app/update-route53.sh 2>&1 | while IFS= read -r line; do echo "[Route53] $line"; done) &

# Start the .NET application
exec dotnet MyCookbook.API.dll

