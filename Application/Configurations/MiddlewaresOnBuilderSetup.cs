namespace AuthentificationService.Application.Configurations;

public static class MiddlewaresOnBuilderSetup
{
    public static void AddMiddlearesAndSwagger(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddControllers();
        services.AddSwaggerGen();
    }
}