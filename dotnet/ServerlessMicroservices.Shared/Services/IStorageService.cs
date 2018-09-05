using ServerlessMicroservices.Models;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public interface IStorageService
    {
        Task Enqueue(TripItem trip);
        Task Enqueue(TripDemoState tripDemoState);
    }
}
