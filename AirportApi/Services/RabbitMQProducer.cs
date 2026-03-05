using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace AirportApi.Services
{
    public class RabbitMQProducer : IMessageProducer
    {

        public async Task SendFlightMessageAsync<T>(T message)
        {
            //Her opsættes forbindelse til RabbitMQ
            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            //Her deklareres en kø (hvis den ikke findes)
            await channel.QueueDeclareAsync(queue: "flights",
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

            //Her serialiseres objektet til JSON (så det kan sendes som tekst)
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            //Her sendes beskeden
           await channel.BasicPublishAsync(exchange: string.Empty,
                                 routingKey: "flights",
                                 body: body);
        }
    }
}
