using ServerlessMicroservices.Models;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public interface IChangeNotifierService
    {
        Task DriverChanged(DriverItem driver);
        Task TripCreated(TripItem trip);
        Task TripDeleted(TripItem trip);
        Task PassengerChanged(PassengerItem trip);
    }
}
