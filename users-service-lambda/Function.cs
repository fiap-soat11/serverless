using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Text.Json;
using users_service_lambda.Domain.Entities;
using users_service_lambda.Domain.Interfaces;
using users_service_lambda.Infrastructure.Repositories;

// Assembly attribute para converter o input em classe .NET
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace users_service_lambda;

public class Function
{
    private readonly IUserRepository _userRepository;

    public Function()
    {
        _userRepository = RepositoryFactory.CreateUserRepository();
    }

    public Function(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
            var cpf = GetCPFUser(request);
            
            if (string.IsNullOrEmpty(cpf))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = "Bad Request - CPF is required"
                };
            }

            var user = await _userRepository.GetByCpfAsync(cpf);
            
            if (user != null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonSerializer.Serialize(user)
                };
            }
        }
        catch
        {
            // Log error if needed
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
        if (user == null || string.IsNullOrEmpty(user.CPF))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "Bad Request - User data is required"
            };
        }

        if (user.Ativo == false)
        {
            user.Ativo = true;
        }

        var success = await _userRepository.CreateAsync(user);

        if (success)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 201,
                Body = "User Added"
            };
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 500,
            Body = "Internal Server Error - Failed to create user"
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

        var success = await _userRepository.UpdateAsync(user);

        if (success)
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
        try
        {
            return request.QueryStringParameters?["cpf"] ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
    private async Task<APIGatewayProxyResponse> HandleDelete(APIGatewayProxyRequest request)
    {
        var user = JsonSerializer.Deserialize<User>(request.Body);
        var userCPF = string.Empty;

        if (user == null || string.IsNullOrEmpty(user.CPF))
        {
            userCPF = GetCPFUser(request);

            if (string.IsNullOrEmpty(userCPF))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 400,
                    Body = "Bad Request - User ID is required"
                };
            }
        }
        else
        {
            userCPF = user.CPF;
        }

        var success = await _userRepository.DeleteAsync(userCPF);

        if (success)
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
