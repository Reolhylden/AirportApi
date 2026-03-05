using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AirportApi.Models;

List<Flight> flightBoard = new List<Flight>();

Console.WriteLine("Lufthavn informationsskærm kører");
Console.WriteLine("Venter på flyopdateringer \n");

//Her opsættes forbindelse
var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

//Her deklareres køen
await channel.QueueDeclareAsync(queue: "flights",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

//Her bruges den asynkorne consumer
var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var flight = JsonSerializer.Deserialize<Flight>(message);

    if (flight != null)
    {
        Console.WriteLine($"Fly {flight.Flightnumber} til {flight.Destination} afgang: {flight.DepartureTime} gate: {flight.Gate} status: {flight.Status}");
    }
    await Task.CompletedTask;
};

//Her startes lytning asynkront
await channel.BasicConsumeAsync(queue: "flights",
                                autoAck: true,
                                consumer: consumer);

Console.WriteLine("Venter på fly (Tryk 'Enter' for at lukke)");
Console.ReadLine();
