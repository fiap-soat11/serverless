namespace users_service_lambda.Domain.Entities;

public class User
{
    public string CPF { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

