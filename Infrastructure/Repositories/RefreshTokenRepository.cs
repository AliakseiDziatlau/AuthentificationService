using AuthentificationService.Core.Entities;
using AuthentificationService.Core.Interfaces;
using AuthentificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthentificationService.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshTokens> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task AddAsync(RefreshTokens refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync(); 
    }

    public async Task DeleteAsync(RefreshTokens refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
    }
}