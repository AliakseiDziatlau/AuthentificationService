using AuthentificationService.Application.DTOs;
using AuthentificationService.Application.Interfaces;

namespace AuthentificationService.Application.Services;

public class AuthService : IAuthService
{
    public Task RegisterUserAsync(RegisterDTO registerDTO)
    {
        throw new NotImplementedException();
    }

    public Task LoginUserAsync(LoginDTO loginDTO)
    {
        throw new NotImplementedException();
    }

    public Task ConfirmEmailAsync(string token, string email)
    {
        throw new NotImplementedException();
    }
}