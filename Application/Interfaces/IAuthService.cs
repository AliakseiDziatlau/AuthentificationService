using AuthentificationService.Application.DTOs;

namespace AuthentificationService.Application.Interfaces;

public interface IAuthService
{
    Task RegisterUserAsync(RegisterDTO registerDTO);
    Task<string> LoginUserAsync(LoginDTO loginDTO);
    Task ConfirmEmailAsync(string token, string email);
}