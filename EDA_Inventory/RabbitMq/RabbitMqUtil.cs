using EDA_Inventory.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.RabbitMQ;
using Shared.Settings;
using System.Text;

namespace EDA_Inventory.RabbitMq
{
    public class RabbitMqUtil : IRabbitMqUtil
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly RabbitMqSettings rabbitMqSettings;

        public RabbitMqUtil(IServiceScopeFactory serviceScopeFactory, RabbitMqSettings rabbitMqSettings)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.rabbitMqSettings = rabbitMqSettings;
        }

        public async Task PublishMessageQueue(string routingKey, string eventData)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
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
            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);

                await ParseCustomerMessageFromTopic(message, ea, cancellationToken);
            };
            await channel.BasicConsumeAsync(rabbitMqSettings.CustomerRoutingKey,
                true,
                consumer);

            await Task.CompletedTask;
        }

        private async Task ParseCustomerMessageFromTopic(string message,
        BasicDeliverEventArgs ea,
        CancellationToken cancellationToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var productDbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
            var data = JObject.Parse(message);
            var type = ea.RoutingKey;
            if (type == rabbitMqSettings.CustomerRoutingKey)
            {
                var guidValue = Guid.Parse(data["ProductId"].Value<string>());

                var product = await productDbContext
                    .Products
                    .FirstAsync(a => a.ProductId == guidValue, cancellationToken);

                //Get the total Items customer bought
                var itemsBought = data["TotalBought"].Value<int>();
                var currentlyInHand = product.Quantity;
                if (itemsBought <= currentlyInHand)
                {
                    var grandTotal = currentlyInHand - itemsBought;
                    product.Quantity = grandTotal;
                    await productDbContext.SaveChangesAsync(cancellationToken);

                    var updatedProduct = JsonConvert.SerializeObject(new
                    {
                        product.Id,
                        product.ProductId,
                        product.Name,
                        product.Quantity
                    });
                    //Publish the updated stocks to customer
                    await PublishMessageQueue(rabbitMqSettings.ProductRoutingKey, updatedProduct);
                }
            }
        }
    }
}
