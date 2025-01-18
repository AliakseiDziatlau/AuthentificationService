using AuthentificationService.Infrastructure.Services;

namespace AuthentificationService.Application.Configurations;

public static class EventPublisherSetup
{
    public static void AddEventPublisher(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var hostName = config["RabbitMQ:HostName"] ?? "localhost";
            var userName = config["RabbitMQ:UserName"] ?? "guest";
            var password = config["RabbitMQ:Password"] ?? "guest";

            var factory = new RabbitMQ.Client.ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password
            };

            return new EventPublisher(factory.HostName);
        });
    }
}