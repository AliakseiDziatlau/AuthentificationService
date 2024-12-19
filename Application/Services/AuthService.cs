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
    
    public async Task RegisterUserAsync(RegisterDTO registerDTO)
    {
        var existingUser = await _accountsRepository.GetByEmailAsync(registerDTO.Email);
        if (existingUser != null)
            throw new Exception("User already exists.");
        
        var validRoles = new[] { "Doctor", "Receptionist", "Patient" };
        if (!validRoles.Contains(registerDTO.Role))
            throw new Exception("Invalid role.");
        
        var passwordHash = _passwordHasher.HashPassword(registerDTO.Password);
        
        var roleId = registerDTO.Role switch
        {
            "Doctor" => 0,
            "Receptionist" => 1,
            "Patient" => 2,
            _ => throw new Exception("Invalid role.")
        };
        
        var newUser = new Accounts
        {
            email = registerDTO.Email,
            passwordHash = passwordHash,
            phoneNumber = registerDTO.PhoneNumber,
            isEmailVerified = false,
            createdAt = DateTime.UtcNow,
            RoleId = roleId 
        };

        await _accountsRepository.AddAsync(newUser);
        
        var token = Guid.NewGuid().ToString();
        _cache.Set(token, registerDTO.Email, TimeSpan.FromHours(24));
        
        var authentificationServicePath = _configuration["AuthentificationServicePath"];
        var confirmationLink = $"{authentificationServicePath}/api/auth/confirm-email?token={token}&email={registerDTO.Email}";
        
        await _emailService.SendEmailAsync(registerDTO.Email, "Confirm Your Email", 
            $"Please confirm your email by clicking on the link: <a href='{confirmationLink}'>{confirmationLink}</a>");
        
        
        // var existingUser = await _accountsRepository.GetByEmailAsync(registerDTO.Email);
        // if (existingUser != null)
        //     throw new Exception("User already exists.");
        //
        // var passwordHash = _passwordHasher.HashPassword(registerDTO.Password);
        //
        // var newUser = new Accounts
        // {
        //     email = registerDTO.Email,
        //     passwordHash = passwordHash,
        //     phoneNumber = registerDTO.PhoneNumber,
        //     isEmailVerified = false,
        //     createdAt = DateTime.UtcNow
        // };
        //
        // await _accountsRepository.AddAsync(newUser);
        // var token = Guid.NewGuid().ToString();
        // _cache.Set(token, registerDTO.Email, TimeSpan.FromHours(24));
        //
        // var authentificationServicePath = _configuration["AuthentificationServicePath"];
        // var confirmationLink = $"{authentificationServicePath}/api/auth/confirm-email?token={token}&email={registerDTO.Email}";
        //
        // await _emailService.SendEmailAsync(registerDTO.Email, "Confirm Your Email", 
        //     $"Please confirm your email by clicking on the link: <a href='{confirmationLink}'>{confirmationLink}</a>");
    }
    
    
    public async Task<(string AccessToken, string RefreshToken)> LoginUserAsync(LoginDTO loginDTO)
    {
        var user = await _accountsRepository.GetByEmailAsync(loginDTO.Email);
        if (user == null || !_passwordHasher.VerifyPassword(loginDTO.Password, user.passwordHash))
            throw new Exception("Invalid email or password.");

        if (!user.isEmailVerified)
            throw new Exception("Email is not verified.");
        
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = _tokenGenerator.GenerateAndStoreRefreshToken(user.email);

        return (AccessToken: accessToken, RefreshToken: refreshToken);
    }
    
    public async Task ConfirmEmailAsync(string token, string email)
    {
        if (!_cache.TryGetValue(token, out string cachedEmail))
            throw new Exception("Invalid or expired token.");

        if (cachedEmail != email)
            throw new Exception("Invalid email for this token.");
        
        var user = await _accountsRepository.GetByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found.");
        
        user.isEmailVerified = true;
        user.updatedAt = DateTime.UtcNow;
        await _accountsRepository.UpdateAsync(user);
        _cache.Remove(token);
    }
    
    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        if (!_cache.TryGetValue(refreshToken, out string email))
            throw new Exception("Invalid or expired refresh token.");

        var user = await _accountsRepository.GetByEmailAsync(email);
        if (user == null)
            throw new Exception("User not found.");
        
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _tokenGenerator.GenerateAndStoreRefreshToken(email);
        _cache.Remove(refreshToken);

        return (AccessToken: accessToken, RefreshToken: newRefreshToken);
    }
}