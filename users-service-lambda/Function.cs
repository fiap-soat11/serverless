using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MySqlConnector;
using System.Text.Json;

// Assembly attribute para converter o input em classe .NET
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace users_service_lambda;

public class Function
{
    private readonly string _connectionString;

    public Function()
    {
        // pega do environment
        _connectionString = "Server=fiap-mysql-mysql.cbs2akm6e206.us-east-1.rds.amazonaws.com;Database=fiap;User ID=user_fiap;Password=pass_fiap;"
            //_connectionString = "Server=fiap.c238ww24ssn2.us-east-1.rds.amazonaws.com;Database=fiap;User ID=user_fiap;Password=pass_fiap;" //Environment.GetEnvironmentVariable("RDS_CONNECTION_STRING")
            ?? throw new InvalidOperationException("RDS_CONNECTION_STRING not configured");
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        Console.WriteLine(JsonSerializer.Serialize(request));

        return request.HttpMethod switch
        {
            "GET" => await HandleGet(request),
            "POST" => await HandlePost(request),
            "PUT" => await HandlePut(request),
            "DELETE" => await HandleDelete(request),
            _ => new APIGatewayProxyResponse { StatusCode = 500, Body = "Unknown Request" }
        };
    }
    private async Task<APIGatewayProxyResponse> HandleGet(APIGatewayProxyRequest request)
    {
        try
        {
            var userIdString = GetCPFUser(request);
           
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("SELECT CPF, Nome, Email FROM Cliente WHERE CPF = @cpf", conn);
            cmd.Parameters.AddWithValue("@cpf", userIdString);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var user = new User
                {                   
                    CPF = reader.GetString("CPF"),
                    Nome = reader.GetString("Nome"),
                    Email = reader.GetString("Email")
                };

                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonSerializer.Serialize(user)
                };
            }            
        }
        catch
        {
         
        }  

        return new APIGatewayProxyResponse
        {
            StatusCode = 404,
            Body = "User not found"
        };
    }

    private async Task<APIGatewayProxyResponse> HandlePost(APIGatewayProxyRequest request)
    {
        var user = JsonSerializer.Deserialize<User>(request.Body);
        if (user == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "Bad Request"
            };
        }       

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand(
            "INSERT INTO Cliente (CPF, Nome, Email, Ativo) VALUES (@cpf, @nome, @email, 1)", conn);
        cmd.Parameters.AddWithValue("@cpf", user.CPF);
        cmd.Parameters.AddWithValue("@nome", user.Nome);       
        cmd.Parameters.AddWithValue("@email", user.Email);

        await cmd.ExecuteNonQueryAsync();

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = "User Added"
        };
    }

    private async Task<APIGatewayProxyResponse> HandlePut(APIGatewayProxyRequest request)
    {
        var user = JsonSerializer.Deserialize<User>(request.Body);
        if (user == null || string.IsNullOrEmpty(user.CPF))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "Bad Request - User ID is required"
            };
        }

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand(
            "UPDATE Cliente SET Nome = @nome, Email = @email WHERE CPF = @cpf", conn);
        cmd.Parameters.AddWithValue("@cpf", user.CPF);
        cmd.Parameters.AddWithValue("@nome", user.Nome);       
        cmd.Parameters.AddWithValue("@email", user.Email);

        var rows = await cmd.ExecuteNonQueryAsync();

        if (rows > 0)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "User Updated"
            };
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 404,
            Body = "User not found"
        };
    }

    private string GetCPFUser(APIGatewayProxyRequest request)
    {
        var userCPF = string.Empty;
        try
        {
            userCPF = request.QueryStringParameters?["cpf"];
        }
        catch
        {
                        
        }
        return userCPF;
    }
    private async Task<APIGatewayProxyResponse> HandleDelete(APIGatewayProxyRequest request)
    {
        var user = JsonSerializer.Deserialize<User>(request.Body);
        var userCPF = string.Empty;

        if (user == null || string.IsNullOrEmpty(user.CPF))
        {
            userCPF = GetCPFUser(request);

            if (string.IsNullOrEmpty(userCPF)) { 
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = "Bad Request - User ID is required"
                };            
            }

            user = new User() { CPF = userCPF };
        }

        userCPF = user.CPF;

        if (string.IsNullOrEmpty(userCPF))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "Bad Request - Invalid UserId"
            };
        }

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand("DELETE FROM Cliente WHERE CPF = @cpf", conn);
        cmd.Parameters.AddWithValue("@cpf", userCPF);

        var rows = await cmd.ExecuteNonQueryAsync();

        if (rows > 0)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "User Deleted"
            };
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 404,
            Body = "User not found"
        };
    }
}
public class User
{    
    public string CPF { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}
