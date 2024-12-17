using AuthentificationService.Core.Entities;

namespace AuthentificationService.Core.Interfaces;

public interface IAccountsRepository
{
    Task<Accounts> GetByIdAsync(int id);
    Task<Accounts> GetByEmailAsync(string email);
    Task AddAsync(Accounts account);
    Task UpdateAsync(Accounts account);
    Task DeleteAsync(int id);
}