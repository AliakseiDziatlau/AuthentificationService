using AuthentificationService.Core.Entities;

namespace AuthentificationService.Infrastructure.Services;

public interface ITokenGenerator
{
    string GenerateAccessToken(Accounts account);
    Task<string> GenerateAndStoreRefreshToken(int accountId);
    string Encrypt(string originText, string encryptionKey);
    string Decrypt(string encryptedText, string encryptionKey);
}