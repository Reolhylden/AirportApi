using System.Threading.Tasks;

namespace AirportApi.Services
{
    public interface IMessageProducer
    {
        Task SendFlightMessageAsync<T>(T message);
    }
}
