using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Shared.Settings;

namespace Shared.RabbitMQ;

public class RabbitMqService : BackgroundService
{
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IRabbitMqUtil _rabbitMqUtil;
    private readonly IServiceProvider _serviceProvider;
    private IChannel _channel;
    private IConnection _connection;


    public RabbitMqService(
        IServiceProvider serviceProvider,
        IRabbitMqUtil rabbitMqUtil,
        RabbitMqSettings rabbitMqSettings)
    {
        _serviceProvider = serviceProvider;
        _rabbitMqUtil = rabbitMqUtil;
        _rabbitMqSettings = rabbitMqSettings;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.InstanceName,
            UserName = _rabbitMqSettings.UserName,
            Password = _rabbitMqSettings.Password,
            ConsumerDispatchConcurrency = 1
        };
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now:u} : Listening the RabbitMq");

        using var scope = _serviceProvider.CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<IRabbitScopedService>();
        await _rabbitMqUtil.ListenMessageQueue(_channel, cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await _connection.CloseAsync();
    }
}