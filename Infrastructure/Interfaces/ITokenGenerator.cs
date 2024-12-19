using AuthentificationService.Core.Entities;

namespace AuthentificationService.Infrastructure.Services;

public interface ITokenGenerator
{
    string GenerateAccessToken(Accounts account);
    string GenerateAndStoreRefreshToken(string email);
}