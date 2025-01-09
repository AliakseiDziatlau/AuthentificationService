using AuthentificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AuthentificationService.Application.Configurations;

public static class DatabaseSetup
{
    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    }
}