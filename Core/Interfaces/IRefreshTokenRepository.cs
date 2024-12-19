using AuthentificationService.Core.Entities;

namespace AuthentificationService.Core.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshTokens> GetByTokenAsync(string token);
    Task AddAsync(RefreshTokens refreshToken);
    Task DeleteAsync(RefreshTokens refreshToken);
}