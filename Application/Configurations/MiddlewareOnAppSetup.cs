using Serilog;

namespace AuthentificationService.Application.Configurations;

public static class MiddlewareOnAppSetup
{
    public static void AddMiddlewares(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}