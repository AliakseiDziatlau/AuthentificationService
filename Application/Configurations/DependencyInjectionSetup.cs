using AuthentificationService.Application.Interfaces;
using AuthentificationService.Application.Services;
using AuthentificationService.Core.Interfaces;
using AuthentificationService.Infrastructure.Repositories;
using AuthentificationService.Infrastructure.Services;

namespace AuthentificationService.Application.Configurations;

public static class DependencyInjectionSetup
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Program));
        services.AddScoped<IAccountsRepository, AccountsRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    }
}