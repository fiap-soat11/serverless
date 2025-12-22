using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using users_service_lambda.Domain.Entities;
using users_service_lambda.Domain.Interfaces;

namespace users_service_lambda.Infrastructure.Repositories;

public class DynamoDbUserRepository : IUserRepository
{
    private readonly IAmazonDynamoDB _client;
    private readonly string _tableName;

    public DynamoDbUserRepository()
    {
        _client = new AmazonDynamoDBClient();
        _tableName = Environment.GetEnvironmentVariable("DYNAMODB_TABLE_NAME") ?? "Users";
    }

    public DynamoDbUserRepository(IAmazonDynamoDB dynamoDbClient, string tableName)
    {
        _client = dynamoDbClient;
        _tableName = tableName;
    }

    public async Task<User?> GetByCpfAsync(string cpf)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "CPF", new AttributeValue { S = cpf } }
                }
            };

            var response = await _client.GetItemAsync(request);

            if (!response.Item.Any())
                return null;

            return new User
            {
                CPF = response.Item["CPF"].S ?? string.Empty,
                Nome = response.Item["Nome"].S ?? string.Empty,
                Email = response.Item["Email"].S ?? string.Empty,
                Ativo = response.Item.ContainsKey("Ativo") && (response.Item["Ativo"].BOOL ?? false)
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CreateAsync(User user)
    {
        try
        {
            var request = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "CPF", new AttributeValue { S = user.CPF } },
                    { "Nome", new AttributeValue { S = user.Nome } },
                    { "Email", new AttributeValue { S = user.Email } },
                    { "Ativo", new AttributeValue { BOOL = user.Ativo } }
                },
                ConditionExpression = "attribute_not_exists(CPF)"
            };

            await _client.PutItemAsync(request);
            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(User user)
    {
        try
        {
            var request = new UpdateItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "CPF", new AttributeValue { S = user.CPF } }
                },
                UpdateExpression = "SET Nome = :nome, Email = :email, Ativo = :ativo",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":nome", new AttributeValue { S = user.Nome } },
                    { ":email", new AttributeValue { S = user.Email } },
                    { ":ativo", new AttributeValue { BOOL = user.Ativo } }
                },
                ConditionExpression = "attribute_exists(CPF)",
                ReturnValues = ReturnValue.NONE
            };

            await _client.UpdateItemAsync(request);
            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string cpf)
    {
        try
        {
            var request = new DeleteItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "CPF", new AttributeValue { S = cpf } }
                },
                ConditionExpression = "attribute_exists(CPF)",
                ReturnValues = ReturnValue.NONE
            };

            await _client.DeleteItemAsync(request);
            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}

