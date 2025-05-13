using EDA_Customer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.RabbitMQ;
using Shared.Settings;
using System.Text;

namespace EDA_Customer.RabbitMq
{
    public class RabbitMqUtil : IRabbitMqUtil
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RabbitMqSettings _rabbitMqSettings;

        public RabbitMqUtil(IServiceScopeFactory serviceScopeFactory, RabbitMqSettings rabbitMqSettings)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _rabbitMqSettings = rabbitMqSettings;
        }

        public async Task PublishMessageQueue(string routingKey, string eventData)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _rabbitMqSettings.InstanceName,
                //Port = 5672,
                /*UserName = "admin",
                Password = "Admin@1"*/
            };
            var connection = await factory.CreateConnectionAsync();
            using (var channel = await connection.CreateChannelAsync())
            {
                /*await channel.QueueDeclareAsync(queue: routingKey,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);*/
                var body = Encoding.UTF8.GetBytes(eventData);
                var props = new BasicProperties();
                props.ContentType = "text/plain";
                props.DeliveryMode = (DeliveryModes)2;
                await channel.BasicPublishAsync(exchange: "topic.exchange",
                                     routingKey: routingKey,
                                     mandatory: true,
                                     basicProperties: props,
                                     body: body);
            }
            await Task.CompletedTask;
        }

        public async Task ListenMessageQueue(IChannel channel, CancellationToken cancellationToken)
        {
            /*await channel.QueueDeclareAsync(
                queue: "inventory.product",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );*/
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await ParseInventoryProductMessage(message, ea, cancellationToken);
            };
            await channel.BasicConsumeAsync(queue: _rabbitMqSettings.ProductRoutingKey,
                                 autoAck: true,
                                 consumer: consumer);
            await Task.CompletedTask;
        }

        private async Task ParseInventoryProductMessage(string message,
        BasicDeliverEventArgs ea, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var customerDbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();

            var data = JObject.Parse(message);
            var type = ea.RoutingKey;
            if (type == _rabbitMqSettings.ProductRoutingKey)
            {
                var guidValue = Guid.Parse(data["ProductId"].Value<string>());

                var product = await customerDbContext
                    .Products
                    .FirstOrDefaultAsync(a => a.ProductId == guidValue, cancellationToken);

                if (product != null)
                {
                    product.Name = data["Name"].Value<string>();
                    product.Quantity = data["Quantity"].Value<int>();
                }
                else
                {
                    await customerDbContext.Products.AddAsync(new Product
                    {
                        Id = data["Id"].Value<int>(),
                        ProductId = guidValue,
                        Name = data["Name"].Value<string>(),
                        Quantity = data["Quantity"].Value<int>()
                    }, cancellationToken);
                }

                await customerDbContext.SaveChangesAsync(cancellationToken);

                await Task.Delay(new Random().Next(1, 3) * 1000, cancellationToken);
            }
        }
    }
}
