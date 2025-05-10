using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace AuthentificationService.Infrastructure.Services;

public class EventPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName = "UserEvents";

    public EventPublisher(string hostname)
    {
        var factory = new ConnectionFactory() { HostName = hostname };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Fanout);
    }

    public void PublishPhoneNumberChangedEvent(object phoneNumberChangedEvent)
    {
        var message = JsonSerializer.Serialize(phoneNumberChangedEvent);
        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: _exchangeName, routingKey: "", basicProperties: null, body: body);
        Console.WriteLine($"[Publisher] Sent event: {message}");
    }

    public void Close()
    {
        _channel.Close();
        _connection.Close();
    }
}