using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthentificationService.Application.DTOs;
using AuthentificationService.Application.Interfaces;
using AuthentificationService.Core.Entities;
using AuthentificationService.Core.Enum;
using AuthentificationService.Core.Interfaces;
using AuthentificationService.Infrastructure.Events;
using AuthentificationService.Infrastructure.Services;
using AutoMapper;
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
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly EventPublisher _publisher;

    public AuthService(IAccountsRepository accountsRepository,
                       ITokenGenerator tokenGenerator,
                       IEmailService emailService,
                       IPasswordHasher passwordHasher,
                       IConfiguration configuration,
                       IMemoryCache cache,
                       IRefreshTokenRepository refreshTokenRepository,
                       IMapper mapper,
                       ILogger<AuthService> logger,
                       EventPublisher publisher)
    {
        _accountsRepository = accountsRepository;
        _tokenGenerator = tokenGenerator;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _cache = cache;
        _refreshTokenRepository = refreshTokenRepository;
        _mapper = mapper;
        _logger = logger;
        _publisher = publisher;
    }
    
    public async Task RegisterUserAsync(RegisterDTO registerDTO)
    {
        _logger.LogInformation("RegisterUserAsync started for email: {Email}", registerDTO.Email);
        var existingUser = await _accountsRepository.GetByEmailAsync(registerDTO.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Attempt to register an already existing user with email: {Email}", registerDTO.Email);            
            throw new Exception("User already exists.");
        }

        if (!Enum.TryParse(typeof(RolesEnum), registerDTO.Role, true, out var parsedRole))
        {
            _logger.LogWarning("Invalid role provided: {Role}", registerDTO.Role);
            throw new Exception("Invalid role.");
        }
        
        var roleId = (int)(RolesEnum)parsedRole;
        var passwordHash = _passwordHasher.HashPassword(registerDTO.Password);
        
        var newUser = _mapper.Map<Accounts>(registerDTO);
        newUser.passwordHash = passwordHash;
        newUser.RoleId = roleId;

        await _accountsRepository.AddAsync(newUser);
        _logger.LogInformation("User registered successfully with email: {Email}", registerDTO.Email);
        
        var token = Guid.NewGuid().ToString();
        _cache.Set(token, registerDTO.Email, TimeSpan.FromHours(24));
        
        var authentificationServicePath = _configuration["AuthentificationServicePath"];
        var confirmationLink = $"{authentificationServicePath}/api/auths/confirm-email?token={token}&email={registerDTO.Email}";
        var subject = _configuration["EmailSettings:ConfirmEmailSubject"];
        var bodyTemplate = _configuration["EmailSettings:ConfirmEmailBody"];
        var body = string.Format(bodyTemplate, confirmationLink);
        
        await _emailService.SendEmailAsync(registerDTO.Email, subject, body);
        
        _logger.LogInformation("Confirmation email sent to: {Email}", registerDTO.Email);
    }
    
    
    public async Task<(string AccessToken, string RefreshToken)> LoginUserAsync(LoginDTO loginDTO)
    {
        _logger.LogInformation("LoginUserAsync started for email: {Email}", loginDTO.Email);

        var user = await _accountsRepository.GetByEmailAsync(loginDTO.Email);
        if (user == null || !_passwordHasher.VerifyPassword(loginDTO.Password, user.passwordHash))
        {
            _logger.LogWarning("Invalid login attempt for email: {Email}", loginDTO.Email);
            throw new Exception("Invalid email or password.");
        }

        if (!user.isEmailVerified)
        {
            _logger.LogWarning("Login attempt with unverified email: {Email}", loginDTO.Email);
            throw new Exception("Email is not verified.");
        }
        
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = await _tokenGenerator.GenerateAndStoreRefreshToken(user.id);

        _logger.LogInformation("Login successful for email: {Email}", loginDTO.Email);
        return (AccessToken: accessToken, RefreshToken: refreshToken);
    }
    
    public async Task ConfirmEmailAsync(string token, string email)
    {
        _logger.LogInformation("ConfirmEmailAsync started for email: {Email}", email);
        if (!_cache.TryGetValue(token, out string cachedEmail))
        {
            _logger.LogWarning("Invalid or expired confirmation token for email: {Email}", email);
            throw new Exception("Invalid or expired token.");
        }
        
        if (cachedEmail != email)
        {
            _logger.LogWarning("Token email mismatch for email: {Email}", email);
            throw new Exception("Invalid email for this token.");
        }
        
        var user = await _accountsRepository.GetByEmailAsync(email);
        if (user == null)
        {
            _logger.LogError("User not found for email: {Email}", email);
            throw new Exception("User not found.");
        }
        
        user.isEmailVerified = true;
        user.updatedAt = DateTime.UtcNow;
        await _accountsRepository.UpdateAsync(user);
        _cache.Remove(token);
        _logger.LogInformation("Email confirmed successfully for email: {Email}", email);
    }
    
    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("RefreshTokenAsync started for refreshToken: {RefreshToken}", refreshToken);
        var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (storedRefreshToken == null || storedRefreshToken.ExpiryDate < DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid refresh token: {RefreshToken}", refreshToken);
            throw new Exception("Invalid or expired refresh token.");
        }
        
        var user = await _accountsRepository.GetByIdAsync(storedRefreshToken.AccountId);
        if (user == null)
        {
            _logger.LogError("User not found for refresh token: {RefreshToken}", refreshToken);
            throw new Exception("User not found.");
        }
        
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = await _tokenGenerator.GenerateAndStoreRefreshToken(user.id);
        await _refreshTokenRepository.DeleteAsync(storedRefreshToken);
        _logger.LogInformation("Old refresh token deleted: {RefreshToken}", refreshToken);
        _logger.LogInformation("New tokens generated for user ID: {UserId}", user.id);
        return (AccessToken: accessToken, RefreshToken: newRefreshToken);
    }
    
    public string Authorize(string encryptedToken)
    {
        _logger.LogInformation("Authorization started for encrypted token.");
        var decryptedToken = _tokenGenerator.Decrypt(encryptedToken, _configuration["EncryptionKey"]);
        _logger.LogInformation("Token successfully decrypted.");
        var handler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]))
        };

        try
        {
            _logger.LogInformation("Validating token...");
            var principal = handler.ValidateToken(decryptedToken, validationParameters, out var validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                _logger.LogWarning("Invalid token format.");
                throw new UnauthorizedAccessException("Invalid token.");
            }
            var roleClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim == null)
            {
                _logger.LogWarning("Role claim missing in token.");
                throw new UnauthorizedAccessException("Access denied. Invalid role.");
            }

            if (!Enum.TryParse<RolesEnum>(roleClaim.Value, out var userRole))
            {
                _logger.LogWarning("Invalid role value in token: {RoleValue}", roleClaim.Value);
                throw new UnauthorizedAccessException("Access denied. Invalid role.");
            }
            
            _logger.LogInformation("Role extracted from token: {Role}", userRole);
            
            if (userRole != RolesEnum.Receptionist)
            {
                _logger.LogWarning("Access denied for role: {Role}", userRole);
                throw new UnauthorizedAccessException("Access denied. User does not have the required role.");
            }

            _logger.LogInformation("Authorization successful for role: {Role}", userRole);
            return userRole.ToString();
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "Token validation failed.");
            throw new UnauthorizedAccessException($"Token validation failed: {ex.Message}");
        }
    }
    
    public async Task UpdateUserAsync(int id, UpdateUserDTO updateUserDto)
    {
        var user = await _accountsRepository.GetByIdAsync(id);
        if (user == null)
            throw new Exception("User not found");

        bool isPhoneNumberUpdated = false;

        if (!string.IsNullOrEmpty(updateUserDto.Email))
            user.email = updateUserDto.Email;

        if (!string.IsNullOrEmpty(updateUserDto.PhoneNumber) && user.phoneNumber != updateUserDto.PhoneNumber)
        {
            user.phoneNumber = updateUserDto.PhoneNumber;
            isPhoneNumberUpdated = true;
        }

        user.updatedAt = DateTime.UtcNow;

        await _accountsRepository.UpdateAsync(user);

        if (isPhoneNumberUpdated)
        {
            var phoneNumberChangedEvent = new PhoneNumberChangedEvent
            {
                UserEmail = user.email,
                NewPhoneNumber = updateUserDto.PhoneNumber,
                Timestamp = DateTime.UtcNow,
                RoleId = user.RoleId,
            };

            _publisher.PublishPhoneNumberChangedEvent(phoneNumberChangedEvent);
        }
    }
    
    public async Task<IEnumerable<AccountsDTO>> GetAllAccountsAsync()
    {
        var accounts = await _accountsRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<AccountsDTO>>(accounts);
    }

    public async Task<bool> CheckEmailExistsAsync(string email)
    {
        return await _accountsRepository.CheckEmailExistsAsync(email);
    }
}