using users_service_lambda.Domain.Interfaces;

namespace users_service_lambda.Infrastructure.Repositories;

public static class RepositoryFactory
{
    public static IUserRepository CreateUserRepository()
    {
        var databaseType = Environment.GetEnvironmentVariable("DATABASE_TYPE")?.ToUpperInvariant() ?? "DYNAMODB";

        return databaseType switch
        {
            "DYNAMODB" => new DynamoDbUserRepository(),
            "MYSQL" => new MySqlUserRepository(),
            _ => throw new InvalidOperationException($"Unsupported database type: {databaseType}")
        };
    }
}

