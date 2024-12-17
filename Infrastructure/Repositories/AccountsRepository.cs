using AuthentificationService.Core.Entities;
using AuthentificationService.Core.Interfaces;

namespace AuthentificationService.Infrastructure.Repositories;

public class AccountsRepository : IAccountsRepository
{
    public Task<Accounts> GetByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<Accounts> GetByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(Accounts account)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Accounts account)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}