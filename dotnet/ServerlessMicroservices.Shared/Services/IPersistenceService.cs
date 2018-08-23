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
        Task<List<DriverItem>> RetrieveDrivers(double latitude, double longitude, double miles, int max = Constants.MAX_RETRIEVE_DOCS);
        Task<List<DriverItem>> RetrieveActiveDrivers(int max = Constants.MAX_RETRIEVE_DOCS);
        Task<int> RetrieveDriversCount();
        Task<DriverItem> UpsertDriver(DriverItem driver, bool isIgnoreChangeFeed = false);
        Task<string> UpsertDriverLocation(DriverLocationItem driver, bool isIgnoreChangeFeed = false);
        Task<List<DriverLocationItem>> RetrieveDriverLocations(string code, int max = Constants.MAX_RETRIEVE_DOCS);
        Task DeleteDriver(string code);

        // Trips
        Task<TripItem> RetrieveTrip(string code);
        Task<List<TripItem>> RetrieveTrips(int max = Constants.MAX_RETRIEVE_DOCS);
        Task<List<TripItem>> RetrieveTrips(double latitude, double longitude, double miles, int max = Constants.MAX_RETRIEVE_DOCS);
        Task<List<TripItem>> RetrieveActiveTrips(int max = Constants.MAX_RETRIEVE_DOCS);
        Task<int> RetrieveTripsCount();
        Task<int> RetrieveActiveTripsCount();
        Task<TripItem> UpsertTrip(TripItem trip, bool isIgnoreChangeFeed = false);
        Task DeleteTrip(string code);

        // High-level methods
        Task<TripItem> AssignTripAvailableDrivers(TripItem trip, List<DriverItem> drivers);
        Task<TripItem> AssignTripDriver(TripItem trip, string driverCode);
        Task RecycleTripDriver(TripItem trip);
        Task<TripItem> CheckTripCompletion(TripItem trip);
        Task<TripItem> AbortTrip(TripItem trip);
    }
}
