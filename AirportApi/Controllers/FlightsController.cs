using Microsoft.AspNetCore.Mvc;
using AirportApi.Models;
using AirportApi.Services;

namespace AirportApi.Controllers
{
    [ApiController]
    [Route("api/controller")]
    public class FlightsController : ControllerBase
    {
        // Statisk liste til at simulere database
        private static List<Flight> _flights = new List<Flight>();
        private readonly IMessageProducer _messageProducer;

        // Constructor injection
        public FlightsController(IMessageProducer messageProducer)
        {
            _messageProducer = messageProducer;
        }

        // POST api/flights
        [HttpPost]
        public async Task<IActionResult> CreateFlightAsync([FromBody] Flight flight)
        {

            _flights.Add(flight);

            // Besked til RabbitMQ
            await _messageProducer.SendFlightMessageAsync(flight);

            return Ok(new { Message = "Fly er oprettet og sendt til skærme" });
        }

        // PUT api/flights/{flightNumber}
        [HttpPut("{flightNumber}")]
        public async Task<IActionResult> UpdateFlight(string flightNumber, [FromBody] Flight updatedFlight)
        {
            var flight = _flights.FirstOrDefault(f => f.FlightNumber == flightNumber);
            if (flight == null) return NotFound();

            flight.Destination = updatedFlight.Destination;
            flight.DepartureTime = updatedFlight.DepartureTime;
            flight.Gate = updatedFlight.Gate;
            flight.Status = updatedFlight.Status;

            await _messageProducer.SendFlightMessageAsync(flight);

            return Ok(flight);
        }

        // DELETE api/flights/{flightNumber}
        [HttpDelete("{flightNumber}")]
        public async Task<IActionResult> DeleteFlight(string flightNumber)
        {
            var flight = _flights.FirstOrDefault(f => f.FlightNumber == flightNumber);
            if (flight == null) return NotFound();

            _flights.Remove(flight);

            flight.Status = "Deleted";
            await _messageProducer.SendFlightMessageAsync(flight);

            return NoContent();
        }
    }
}
