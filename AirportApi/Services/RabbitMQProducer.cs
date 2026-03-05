using AirportApi.Models;
using RabbitMQ.Client;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace AirportApi.Services
{
    public class RabbitMQProducer : IMessageProducer
    {
        public async Task SendFlightMessageAsync<T>(T message)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Deklarere en Exchange af typen 'Topic'
            string exchangeName = "airport_topic_exchange";
            await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            // Bestemer routing key ud fra flytypen
            string routingKey = "flight.all";
            if (message is Flight f)
            {
                routingKey = $"flight.{f.FlightType.ToLower()}";
            }

            // Sender til exchangen i stedet for køen
            await channel.BasicPublishAsync(exchange: exchangeName,
                                            routingKey: routingKey,
                                            body: body);

            Console.WriteLine($"[SENDT] Besked sendt med emne: {routingKey}");
        }
    }
}
