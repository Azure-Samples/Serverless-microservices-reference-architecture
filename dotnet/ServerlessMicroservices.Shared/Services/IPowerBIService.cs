using ServerlessMicroservices.Models;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public interface IPowerBIService
    {
        Task UpsertTrip(TripItem trip);
    }
}
