using AuthentificationService.Core.Entities;
using AuthentificationService.Core.Interfaces;
using AuthentificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthentificationService.Infrastructure.Repositories;

public class AccountsRepository : IAccountsRepository
{
    private readonly AppDbContext _context;

    public AccountsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Accounts> GetByIdAsync(int id)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.id == id);
    }

    public async Task<Accounts> GetByEmailAsync(string email)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.email == email);
    }

    public async Task AddAsync(Accounts account)
    {
        await _context.Accounts.AddAsync(account); 
        await _context.SaveChangesAsync(); 
    }

    public async Task UpdateAsync(Accounts account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var account = await GetByIdAsync(id);
        if (account != null)
        {
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }
}