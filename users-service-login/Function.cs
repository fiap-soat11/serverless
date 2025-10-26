using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

// Assembly attribute para serializar JSON
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace users_service_login;

public class Function
{
    private readonly IAmazonLambda _lambdaClient;
    private string privateKey = "mySuperSecretKey123!@#2025fiapSoat11";

    public Function()
    {
        _lambdaClient = new AmazonLambdaClient(); // usa as credenciais da Role da Lambda
    }
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {        
        var body = System.Text.Json.JsonDocument.Parse(request.Body);
        string cpf = body.RootElement.GetProperty("cpf").GetString();

        if (string.IsNullOrEmpty(cpf))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 401,
                Body = "{\"error\":\"Credenciais inválidas\"}",
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        var resultLambda = await InvokeUsersServiceLambda(cpf);
        
        var nome = string.Empty;
        var email = string.Empty;

        var outer = JsonSerializer.Deserialize<Dictionary<string, object>>(resultLambda);

        if (outer.TryGetValue("body", out var bodyRaw))
        {
            var bodyJson = bodyRaw?.ToString();

            var userData = JsonSerializer.Deserialize<Dictionary<string, string>>(bodyJson);

            if (userData != null)
            {                
                nome = userData.GetValueOrDefault("Nome");
                email = userData.GetValueOrDefault("Email");
            }
        }

        // 🔹 Gera o token JWT
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var tokenOptions = new JwtSecurityToken(
            issuer: "users_service",
            audience: "users_service_client",
            claims: new List<Claim>
            {
                new Claim("Nome", nome),
                new Claim("Email", email),
                new Claim($"CPF", cpf)
            },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: signinCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = $"{{\"token\":\"{tokenString}\"}}",
            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
        };
    }

    private async Task<string> InvokeUsersServiceLambda(string cpf)
    {
        var payload = new APIGatewayProxyRequest
        {
            HttpMethod = "GET",
            QueryStringParameters = new Dictionary<string, string>
        {
            { "cpf", cpf }
        }
        };

        var request = new InvokeRequest
        {
            FunctionName = "users_service_lambda",
            InvocationType = InvocationType.RequestResponse,
            Payload = JsonSerializer.Serialize(payload)
        };

        var response = await _lambdaClient.InvokeAsync(request);

        if (response.StatusCode == 200)
        {
            using var reader = new StreamReader(response.Payload, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        throw new Exception($"Erro ao chamar users_service_lambda, StatusCode={response.StatusCode}");
    }
}
