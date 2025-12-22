# DynamoDB Setup Guide

## Creating the DynamoDB Table

### Using AWS CLI

```bash
aws dynamodb create-table \
    --table-name Users \
    --attribute-definitions \
        AttributeName=CPF,AttributeType=S \
    --key-schema \
        AttributeName=CPF,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST \
    --region us-east-1
```

### Using AWS Console

1. Go to DynamoDB in AWS Console
2. Click "Create table"
3. Table name: `Users`
4. Partition key: `CPF` (String)
5. Table settings: Use default settings or customize as needed
6. Click "Create table"

### Using CloudFormation/Terraform

#### CloudFormation Template Example

```yaml
Resources:
  UsersTable:
    Type: AWS::DynamoDB::Table
    Properties:
      TableName: Users
      AttributeDefinitions:
        - AttributeName: CPF
          AttributeType: S
      KeySchema:
        - AttributeName: CPF
          KeyType: HASH
      BillingMode: PAY_PER_REQUEST
```

## Lambda IAM Permissions

Ensure your Lambda execution role includes the following policy:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "dynamodb:GetItem",
        "dynamodb:PutItem",
        "dynamodb:UpdateItem",
        "dynamodb:DeleteItem"
      ],
      "Resource": "arn:aws:dynamodb:*:*:table/Users"
    }
  ]
}
```

## Environment Variables

Set these in your Lambda function configuration:

- `DATABASE_TYPE=DYNAMODB`
- `DYNAMODB_TABLE_NAME=Users` (optional, defaults to "Users")

## Testing Locally

For local testing, you can use DynamoDB Local:

```bash
docker run -p 8000:8000 amazon/dynamodb-local
```

Then set the AWS endpoint:
```bash
export AWS_ENDPOINT_URL=http://localhost:8000
```

