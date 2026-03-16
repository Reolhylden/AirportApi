using AirportApi.Models;
using RabbitMQ.Client;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace AirportApi.Services
{
    public class RabbitMQProducer : IMessageProducer, IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;
        private const string ExchangeName = "airport_topic_exchange";

        public RabbitMQProducer()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
        }

        private async Task EnsureConnectionAsync()
        {
            if (_connection == null || !_connection.IsOpen)
                _connection = await _factory.CreateConnectionAsync();

            if (_channel == null || !_channel.IsOpen)
            {
                _channel = await _connection.CreateChannelAsync();
                // Deklarere en Exchange af typen 'Topic' her én gang
                await _channel.ExchangeDeclareAsync(exchange: ExchangeName, type: ExchangeType.Topic);
            }
        }

        public async Task SendFlightMessageAsync<T>(T message)
        {
            await EnsureConnectionAsync();

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            // Bestemmer routing key ud fra flytypen
            string routingKey = "flight.all";
            if (message is Flight f && !string.IsNullOrEmpty(f.FlightType))
            {
                routingKey = $"flight.{f.FlightType.ToLower()}";
            }

            // Sender til exchangen
            await _channel!.BasicPublishAsync(exchange: ExchangeName,
                                            routingKey: routingKey,
                                            body: body);

            Console.WriteLine($"[SENDT] Besked sendt med emne: {routingKey}");
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null) await _channel.DisposeAsync();
            if (_connection != null) await _connection.DisposeAsync();
        }
    }
}
