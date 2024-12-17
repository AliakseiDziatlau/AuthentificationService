using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthentificationService.Application.DTOs;
using AuthentificationService.Application.Interfaces;
using AuthentificationService.Core.Entities;
using AuthentificationService.Core.Interfaces;
using AuthentificationService.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _cache;

    public AuthService(IAccountsRepository accountsRepository,
                       ITokenGenerator tokenGenerator,
                       IEmailService emailService,
                       IPasswordHasher passwordHasher,
                       IConfiguration configuration,
                       IMemoryCache cache)
    {
        _accountsRepository = accountsRepository;
        _tokenGenerator = tokenGenerator;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _cache = cache;
    }

    // Регистрация пользователя
    public async Task RegisterUserAsync(RegisterDTO registerDTO)
    {
        var existingUser = await _accountsRepository.GetByEmailAsync(registerDTO.Email);
        if (existingUser != null)
            throw new Exception("User already exists.");

        var passwordHash = _passwordHasher.HashPassword(registerDTO.Password);

        var newUser = new Accounts
        {
            email = registerDTO.Email,
            passwordHash = passwordHash,
            phoneNumber = registerDTO.PhoneNumber,
            isEmailVerified = false,
            createdAt = DateTime.UtcNow
        };

        await _accountsRepository.AddAsync(newUser);

        // Генерация токена
        var token = Guid.NewGuid().ToString();

        // Сохранение токена в MemoryCache
        _cache.Set(token, registerDTO.Email, TimeSpan.FromHours(24));

        // Отправка токена по email
        var confirmationLink = $"https://yourapp.com/api/auth/confirm-email?token={token}";
        await _emailService.SendEmailAsync(registerDTO.Email, "Confirm Your Email", 
            $"Click here to confirm your email: {confirmationLink}");
    }
    
    public async Task<string> LoginUserAsync(LoginDTO loginDTO)
    {
        var user = await _accountsRepository.GetByEmailAsync(loginDTO.Email);
        if (user == null || !_passwordHasher.VerifyPassword(loginDTO.Password, user.passwordHash))
            throw new Exception("Invalid email or password.");

        if (!user.isEmailVerified)
            throw new Exception("Email is not verified.");
        
        return _tokenGenerator.GenerateToken(user);
    }
    
    public async Task ConfirmEmailAsync(string token, string email)
    {
        // Проверяем наличие токена в MemoryCache
        if (!_cache.TryGetValue(token, out string cachedEmail))
            throw new Exception("Invalid or expired token.");

        if (cachedEmail != email)
            throw new Exception("Invalid email for this token.");

        // Получаем пользователя по email
        var user = await _accountsRepository.GetByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found.");

        // Обновляем статус подтверждения email
        user.isEmailVerified = true;
        user.updatedAt = DateTime.UtcNow;

        await _accountsRepository.UpdateAsync(user);

        // Удаляем токен из MemoryCache
        _cache.Remove(token);
    }
}