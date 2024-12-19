using AuthentificationService.Core.Entities;
using AuthentificationService.Core.Enum;
using Microsoft.EntityFrameworkCore;

namespace AuthentificationService.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<Accounts> Accounts { get; set; }
    public DbSet<RefreshTokens> RefreshTokens { get; set; }
    public DbSet<Roles> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Accounts>()
            .HasOne<Roles>()
            .WithMany()
            .HasForeignKey(a => a.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<RefreshTokens>()
            .HasOne<Accounts>()
            .WithMany()
            .HasForeignKey(rt => rt.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Roles>()
            .Property(r => r.Id)
            .ValueGeneratedNever();
        
        modelBuilder.Entity<Roles>().HasData(
            Enum.GetValues(typeof(RolesEnum))
                .Cast<RolesEnum>()
                .Select(role => new Roles
                {
                    Id = (int)role, 
                    Name = role.ToString()
                })
        );
    }
}