using RabbitMQ.Client;

namespace Shared.RabbitMQ;

public interface IRabbitMqUtil
{
    Task ListenMessageQueue(IChannel channel, CancellationToken cancellationToken);
    Task PublishMessageQueue(string routingKey, string eventData);
}