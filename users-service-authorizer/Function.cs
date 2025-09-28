using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace users_service_authorizer;

public class Function
{
    private readonly IAmazonLambda _lambdaClient;

    public Function()
    {
        _lambdaClient = new AmazonLambdaClient(); // usa as credenciais da Role da Lambda
    }

    public async Task<APIGatewayCustomAuthorizerResponse> FunctionHandler(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
    {
        Console.WriteLine(JsonSerializer.Serialize(request));

        var claimsPricipal = ValidateToken(request.AuthorizationToken);

        if (claimsPricipal == null)
            return UnauthorizedResponse();

        // Se chegou aqui, usuário é válido → chama a outra Lambda
        var cpf = FindClaim(claimsPricipal, "cpf", "CPF", "Cpf", "sub") ?? claimsPricipal.FindFirst(ClaimTypes.NameIdentifier)?.Value?.Trim();

        var response = await InvokeUsersServiceLambda(cpf);

        return AuthorizedResponse(request, claimsPricipal, response);
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
    private APIGatewayCustomAuthorizerResponse AuthorizedResponse(APIGatewayCustomAuthorizerRequest request, ClaimsPrincipal principal, string resultLambda)
    {
        var cpf = string.Empty;
        var nome = string.Empty;
        var email = string.Empty;

        var outer = JsonSerializer.Deserialize<Dictionary<string, object>>(resultLambda);

        if (outer.TryGetValue("body", out var bodyRaw))
        {
            var bodyJson = bodyRaw?.ToString();

            var userData = JsonSerializer.Deserialize<Dictionary<string, string>>(bodyJson);

            if (userData != null)
            {
                cpf = userData.GetValueOrDefault("CPF");
                nome = userData.GetValueOrDefault("Nome");
                email = userData.GetValueOrDefault("Email");               
            }
        }

        if (string.IsNullOrEmpty(resultLambda)) { 
             cpf = FindClaim(principal, "cpf", "CPF", "Cpf", "sub") ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value?.Trim();
             nome = FindClaim(principal, "Nome", "nome") ?? principal.FindFirst(ClaimTypes.GivenName)?.Value?.Trim();
             email = FindClaim(principal, "Email", "email", "emailaddress") ?? principal.FindFirst(ClaimTypes.Email)?.Value?.Trim();        
        }

        return new APIGatewayCustomAuthorizerResponse()
        {
            PrincipalID = cpf,
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy()
            {
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                {
                    new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement()
                    {
                        Effect = "Allow",
                        Resource = new HashSet<string> { "*" },
                        Action = new HashSet<string> { "execute-api:Invoke"}
                    }
                }
            },
            Context = new APIGatewayCustomAuthorizerContextOutput()
            {
                {"CPF", cpf },
                {"Email", email },
                {"Nome", $"{nome}" }
            }
        };
    }

    // Procura pelo claim tentando várias formas (case-insensitive e URIs comuns)
    private static string FindClaim(ClaimsPrincipal p, params string[] names)
    {
        foreach (var n in names)
        {
            var c = p.Claims.FirstOrDefault(cl => string.Equals(cl.Type, n, StringComparison.OrdinalIgnoreCase)
                                                  && !string.IsNullOrWhiteSpace(cl.Value));
            if (c != null) return c.Value.Trim();

            c = p.Claims.FirstOrDefault(cl => cl.Type.EndsWith("/" + n, StringComparison.OrdinalIgnoreCase)
                                              && !string.IsNullOrWhiteSpace(cl.Value));
            if (c != null) return c.Value.Trim();
        }

        var fallback = p.Claims.FirstOrDefault(cl =>
                    string.Equals(cl.Type, ClaimTypes.Email, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(cl.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(cl.Type, ClaimTypes.Name, StringComparison.OrdinalIgnoreCase));

        return fallback?.Value?.Trim();
    }

    private static ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("token is required");

        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            token = token.Substring("Bearer ".Length).Trim();

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        const string privateKey = "mySuperSecretKey123!@#2025fiapSoat11";

        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(privateKey);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = false,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        try
        {
            return handler.ValidateToken(token, validationParameters, out _);
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException("Token inválido: " + ex.Message, ex);
        }
    }

    private APIGatewayCustomAuthorizerResponse UnauthorizedResponse() =>
        new APIGatewayCustomAuthorizerResponse()
        {
            PrincipalID = "unauthorized-user",
            PolicyDocument = new APIGatewayCustomAuthorizerPolicy()
            {
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement> {
                    new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement()
                    {
                        Effect = "Deny"
                    }
                }
            }
        };
}
