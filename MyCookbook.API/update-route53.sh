#!/bin/bash
set -e

echo "Starting Route53 DNS update script..."

# Check if required environment variables are set
if [ -z "$HOSTED_ZONE_ID" ] || [ -z "$DNS_RECORD_NAME" ]; then
    echo "WARNING: HOSTED_ZONE_ID or DNS_RECORD_NAME not set. Skipping Route53 update."
    exit 0
fi

# Get the ECS metadata to find the task's network interface
METADATA_URI="http://169.254.170.2/v2/metadata"
TASK_METADATA=$(curl -s $METADATA_URI)

# Extract the task ARN
TASK_ARN=$(echo $TASK_METADATA | grep -o '"TaskARN":"[^"]*"' | cut -d'"' -f4)
echo "Task ARN: $TASK_ARN"

# Get the AWS region from the task ARN
AWS_REGION=$(echo $TASK_ARN | cut -d':' -f4)
echo "AWS Region: $AWS_REGION"

# Get the cluster name and task ID
CLUSTER_ARN=$(echo $TASK_METADATA | grep -o '"Cluster":"[^"]*"' | cut -d'"' -f4)
TASK_ID=$(echo $TASK_ARN | awk -F'/' '{print $NF}')

echo "Getting task details from ECS..."

# Describe the task to get the network interface ID using AWS CLI query
ENI_ID=$(aws ecs describe-tasks \
    --cluster "$CLUSTER_ARN" \
    --tasks "$TASK_ARN" \
    --region "$AWS_REGION" \
    --query 'tasks[0].attachments[0].details[?name==`networkInterfaceId`].value' \
    --output text)

echo "Network Interface ID: $ENI_ID"

if [ -z "$ENI_ID" ]; then
    echo "ERROR: Could not find network interface ID"
    exit 1
fi

# Get the public IP address
echo "Getting public IP address..."
PUBLIC_IP=$(aws ec2 describe-network-interfaces \
    --network-interface-ids "$ENI_ID" \
    --region "$AWS_REGION" \
    --query 'NetworkInterfaces[0].Association.PublicIp' \
    --output text)

echo "Public IP: $PUBLIC_IP"

if [ -z "$PUBLIC_IP" ] || [ "$PUBLIC_IP" == "None" ]; then
    echo "ERROR: Could not get public IP address"
    exit 1
fi

# Create the Route53 change batch JSON
CHANGE_BATCH=$(cat <<EOF
{
    "Changes": [{
        "Action": "UPSERT",
        "ResourceRecordSet": {
            "Name": "$DNS_RECORD_NAME",
            "Type": "A",
            "TTL": 60,
            "ResourceRecords": [{"Value": "$PUBLIC_IP"}]
        }
    }]
}
EOF
)

echo "Updating Route53 record $DNS_RECORD_NAME to $PUBLIC_IP..."

# Update Route53
CHANGE_INFO=$(aws route53 change-resource-record-sets \
    --hosted-zone-id "$HOSTED_ZONE_ID" \
    --change-batch "$CHANGE_BATCH" \
    --region "$AWS_REGION" \
    --output json)

CHANGE_ID=$(echo $CHANGE_INFO | grep -o '"Id":"[^"]*"' | cut -d'"' -f4)
echo "Route53 change submitted: $CHANGE_ID"

# Wait for the change to be propagated
echo "Waiting for Route53 change to propagate..."
aws route53 wait resource-record-sets-changed \
    --id "$CHANGE_ID" \
    --region "$AWS_REGION"

echo "Route53 DNS record updated successfully!"
echo "DNS: $DNS_RECORD_NAME -> $PUBLIC_IP"

