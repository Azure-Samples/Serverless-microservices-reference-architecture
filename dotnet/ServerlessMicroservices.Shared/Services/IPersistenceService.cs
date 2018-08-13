using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public interface IPersistenceService
    {
        // Drivers
        Task<DriverItem> RetrieveDriver(string code);
        Task<List<DriverItem>> RetrieveDrivers(int max = Constants.MAX_RETRIEVE_DOCS);
        Task<int> RetrieveDriversCount();
        Task<string> UpsertDriver(DriverItem driver, bool isIgnoreChangeFeed = false);
        Task<string> UpsertDriverLocation(DriverLocationItem driver, bool isIgnoreChangeFeed = false);
        Task<List<DriverLocationItem>> RetrieveDriverLocations(string code, int max = Constants.MAX_RETRIEVE_DOCS);
        Task DeleteDriver(string code);
    }
}
