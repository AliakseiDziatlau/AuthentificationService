using AuthentificationService.Core.Entities;

namespace AuthentificationService.Infrastructure.Services;

public interface ITokenGenerator
{
    string GenerateToken(Accounts account);
}