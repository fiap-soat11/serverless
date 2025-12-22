using MySqlConnector;
using users_service_lambda.Domain.Entities;
using users_service_lambda.Domain.Interfaces;

namespace users_service_lambda.Infrastructure.Repositories;

public class MySqlUserRepository : IUserRepository
{
    private readonly string _connectionString;

    public MySqlUserRepository()
    {
        _connectionString = Environment.GetEnvironmentVariable("RDS_CONNECTION_STRING") 
            ?? throw new InvalidOperationException("RDS_CONNECTION_STRING not configured");
    }

    public MySqlUserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> GetByCpfAsync(string cpf)
    {
        try
        {
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("SELECT CPF, Nome, Email, Ativo FROM Cliente WHERE CPF = @cpf", conn);
            cmd.Parameters.AddWithValue("@cpf", cpf);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    CPF = reader.GetString("CPF"),
                    Nome = reader.GetString("Nome"),
                    Email = reader.GetString("Email"),
                    Ativo = reader.GetBoolean("Ativo")
                };
            }

            return null;
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
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "INSERT INTO Cliente (CPF, Nome, Email, Ativo) VALUES (@cpf, @nome, @email, @ativo)", conn);
            cmd.Parameters.AddWithValue("@cpf", user.CPF);
            cmd.Parameters.AddWithValue("@nome", user.Nome);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@ativo", user.Ativo);

            await cmd.ExecuteNonQueryAsync();
            return true;
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
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand(
                "UPDATE Cliente SET Nome = @nome, Email = @email, Ativo = @ativo WHERE CPF = @cpf", conn);
            cmd.Parameters.AddWithValue("@cpf", user.CPF);
            cmd.Parameters.AddWithValue("@nome", user.Nome);
            cmd.Parameters.AddWithValue("@email", user.Email);
            cmd.Parameters.AddWithValue("@ativo", user.Ativo);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
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
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new MySqlCommand("DELETE FROM Cliente WHERE CPF = @cpf", conn);
            cmd.Parameters.AddWithValue("@cpf", cpf);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch
        {
            return false;
        }
    }
}

