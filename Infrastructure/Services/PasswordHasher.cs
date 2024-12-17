namespace AuthentificationService.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        throw new NotImplementedException();
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        throw new NotImplementedException();
    }
}