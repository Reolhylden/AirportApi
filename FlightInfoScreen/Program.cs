using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AirportApi.Models; // Sørg for at denne reference er på plads

// En liste over de aktuelle flyafgange i hukommelsen
List<Flight> flightBoard = new List<Flight>();
string exchangeName = "airport_topic_exchange";

Console.WriteLine("==============================================");
Console.WriteLine("   VELKOMMEN TIL LUFTHAVNENS SKÆRMSYSTEM   ");
Console.WriteLine("==============================================");
Console.Write("Vælg terminal type (domestic / international / all): ");
string terminalType = Console.ReadLine()?.ToLower() ?? "all";

// RabbitMQ setup
var factory = new ConnectionFactory { HostName = "localhost" };

using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic);

// Opretter en midlertidig kø, som er unik for denne skærm
var queueResult = await channel.QueueDeclareAsync();
string queueName = queueResult.QueueName;

// Binder køen til exchanget baseret på terminalvalg
if (terminalType == "all")
{
    // '#' betyder "alt under dette emne"
    await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: "flight.#");
    Console.WriteLine($"Skærmen er sat op til at vise ALLE afgange.");
}
else
{
    await channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: $"flight.{terminalType}");
    Console.WriteLine($"Skærmen er sat op til terminal: {terminalType.ToUpper()}");
}

// Consumer Logik
var consumer = new AsyncEventingBasicConsumer(channel);

consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var updatedFlight = JsonSerializer.Deserialize<Flight>(message);

    if (updatedFlight != null)
    {
        var existingFlight = flightBoard.FirstOrDefault(f => f.FlightNumber == updatedFlight.FlightNumber);

        if (updatedFlight.Status == "Deleted")
        {
            if (existingFlight != null) flightBoard.Remove(existingFlight);
        }
        else
        {
            if (existingFlight != null)
            {
                // Opdater eksisterende data
                existingFlight.Destination = updatedFlight.Destination;
                existingFlight.DepartureTime = updatedFlight.DepartureTime;
                existingFlight.Gate = updatedFlight.Gate;
                existingFlight.Status = updatedFlight.Status;
                existingFlight.FlightType = updatedFlight.FlightType;
            }
            else
            {
                // Tilføj som nyt fly
                flightBoard.Add(updatedFlight);
            }
        }

        // Tegn skærmen forfra
        RenderFlightBoard(flightBoard, terminalType);
    }
    await Task.CompletedTask;
};

// Starter aflytning
await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

Console.WriteLine("\nVenter på flyopdateringer... (Tryk ENTER for at lukke)");
Console.ReadLine();

// Layout hjælpe-metode
void RenderFlightBoard(List<Flight> flights, string type)
{
    Console.Clear();
    Console.WriteLine("================================================================================");
    Console.WriteLine("                         AARHUS AIRPORT - DEPARTURES                            ");
    Console.WriteLine("================================================================================");
    Console.WriteLine($"{"FLIGHT",-10} | {"DESTINATION",-20} | {"TIME",-10} | {"GATE",-8} | {"STATUS"} ");
    Console.WriteLine("--------------------------------------------------------------------------------");

    // Sorter flyene efter afgangstid
    foreach (var f in flights.OrderBy(f => f.DepartureTime))
    {
        // Skift farve baseret på status
        switch (f.Status)
        {
            case "Delayed": Console.ForegroundColor = ConsoleColor.Yellow; break;
            case "Boarding": Console.ForegroundColor = ConsoleColor.Green; break;
            case "Cancelled": Console.ForegroundColor = ConsoleColor.Red; break;
            case "Departed": Console.ForegroundColor = ConsoleColor.Gray; break;
            default: Console.ResetColor(); break;
        }

        Console.WriteLine($"{f.FlightNumber,-10} | {f.Destination,-20} | {f.DepartureTime:HH:mm}      | {f.Gate,-8} | {f.Status}");

        Console.ResetColor();
    }
    Console.WriteLine("=================================================================================");
    Console.WriteLine("\nSidst opdateret: " + DateTime.Now.ToLongTimeString());
   
}