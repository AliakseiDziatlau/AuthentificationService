using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Encrypt(tokenString, _configuration["EncryptionKey"]);
    }
    
    public async Task<string> GenerateAndStoreRefreshToken(int accountId)
    {
        var refreshToken = Guid.NewGuid().ToString();
        var encryptedToken = Encrypt(refreshToken, _configuration["EncryptionKey"]);

        var newRefreshToken = new RefreshTokens
        {
            Token = encryptedToken,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            AccountId = accountId
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken);
        return encryptedToken;
    }
    
    public string Encrypt(string originText, string encryptionKey)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
        aes.IV = aes.Key.Take(16).ToArray(); 
    
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);
        sw.Write(originText);
        sw.Close();
    
        return Convert.ToBase64String(ms.ToArray());
    }
    
    public string Decrypt(string encryptedText, string encryptionKey)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
        aes.IV = aes.Key.Take(16).ToArray();

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(encryptedText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
    
        return sr.ReadToEnd();
    }
}