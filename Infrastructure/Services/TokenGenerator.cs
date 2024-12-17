using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthentificationService.Core.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationService.Infrastructure.Services;

public class TokenGenerator : ITokenGenerator
{
    private readonly IConfiguration _configuration;

    public TokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GenerateToken(Accounts account)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, account.email), 
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), 
            new Claim(ClaimTypes.NameIdentifier, account.id.ToString()), 
            new Claim(ClaimTypes.Email, account.email),
            new Claim(ClaimTypes.Role, "User") 
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"], 
            audience: _configuration["Jwt:Audience"],
            claims: claims, 
            expires: DateTime.UtcNow.AddHours(1), 
            signingCredentials: creds 
        );

        return new JwtSecurityTokenHandler().WriteToken(token); 
    }
}