using users_service_lambda.Domain.Entities;

namespace users_service_lambda.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByCpfAsync(string cpf);
    Task<bool> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(string cpf);
}

