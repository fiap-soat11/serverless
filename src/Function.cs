using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using AutenticacaoApi.Models;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

// Para que a Lambda consiga serializar/deserializar objetos JSON
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AutenticacaoApi
{
    public class Function
    {
        public async Task<AuthResponse> FunctionHandler(AuthRequest request, ILambdaContext context)
        {
            return new AuthResponse { Token = "tokenajndsakjndandksa", Message = "Autenticado com sucesso" };


            if (string.IsNullOrEmpty(request.Cpf) && string.IsNullOrEmpty(request.Email))
            {
                return new AuthResponse { Message = "Informe CPF ou Email" };
            }

            string connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION")
                                      ?? throw new Exception("MYSQL_CONNECTION not set");

            await using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();

            if (!string.IsNullOrEmpty(request.Cpf))
            {
                cmd.CommandText = "SELECT id, nome, email FROM usuarios WHERE cpf = @cpf LIMIT 1";
                cmd.Parameters.AddWithValue("@cpf", request.Cpf);
            }
            else
            {
                cmd.CommandText = "SELECT id, nome, email FROM usuarios WHERE email = @mail LIMIT 1";
                cmd.Parameters.AddWithValue("@mail", request.Email);
            }

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return new AuthResponse { Message = "Usuário não encontrado" };
            }

            var userId = reader.GetInt32("id");
            var nome = reader.GetString("nome");
            var email = reader.GetString("email");

            // Gerar JWT
            var token = GenerateJwt(userId, nome, email);

            return new AuthResponse { Token = token, Message = "Autenticado com sucesso" };
        }

        private string GenerateJwt(int userId, string nome, string email)
        {
            string jwtSecret = "mySuperSecretKey123!@#2025fiapSoat11";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, nome),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "fiap-soat11",
                audience: "fiap-soat11",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
