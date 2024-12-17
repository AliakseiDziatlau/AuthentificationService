using AuthentificationService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthentificationService.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<Accounts> Accounts { get; set; }
}