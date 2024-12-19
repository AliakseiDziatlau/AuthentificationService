using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthentificationService.Core.Entities;
using AuthentificationService.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationService.Infrastructure.Services;

public class TokenGenerator : ITokenGenerator
{
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public TokenGenerator(IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
    {
        _configuration = configuration;
        _refreshTokenRepository = refreshTokenRepository;
    }
    
    public string GenerateAccessToken(Accounts account)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("role", account.RoleId switch
            {
                0 => "Doctor",
                1 => "Receptionist",
                2 => "Patient",
                _ => "Unknown"
            }),
            new Claim("email-confirmation", "true") 
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public async Task<string> GenerateAndStoreRefreshToken(int accountId)
    {
        var refreshToken = Guid.NewGuid().ToString();

        var newRefreshToken = new RefreshTokens
        {
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            AccountId = accountId
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken);
        return refreshToken;
    }
}