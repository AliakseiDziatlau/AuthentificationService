using AuthentificationService.Application.DTOs;

namespace AuthentificationService.Application.Interfaces;

public interface IAuthService
{
    Task RegisterUserAsync(RegisterDTO registerDTO);
    Task<(string AccessToken, string RefreshToken)> LoginUserAsync(LoginDTO loginDTO);
    Task ConfirmEmailAsync(string token, string email);
    Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);
}