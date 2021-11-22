using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public class CosmosPersistenceService : IPersistenceService
    {
        public const string LOG_TAG = "CosmosPersistenceService";

        // Cosmos DocDB API database
        private string _docDbEndpointUri;
        private string _docDbPrimaryKey;
        private string _docDbDatabaseName;

        // Doc DB Collections
        private string _docDbDigitalMainCollectionName;

        private static CosmosClient _docDbSingletonClient;


        private ISettingService _settingService;
        private ILoggerService _loggerService;
        private IChangeNotifierService _changeNotifierService;
        private readonly Lazy<Task<Container>> _cosmosContainer;

        public CosmosPersistenceService(ISettingService setting, ILoggerService logger, IChangeNotifierService changeService)
        {
            _settingService = setting;
            _loggerService = logger;
            _changeNotifierService = changeService;

            _docDbEndpointUri = _settingService.GetDocDbEndpointUri();
            _docDbPrimaryKey = _settingService.GetDocDbApiKey();

            _docDbDatabaseName = _settingService.GetDocDbRideShareDatabaseName();
            _docDbDigitalMainCollectionName = _settingService.GetDocDbMainCollectionName();

            _cosmosContainer = new Lazy<Task<Container>>(async () =>
            {
                var cosmos = new CosmosClient(setting.GetDocDbEndpointUri(), setting.GetDocDbApiKey());
                var db = cosmos.GetDatabase(setting.GetDocDbRideShareDatabaseName());
                //TODO: Hardcoded partition key field here
                return await db.CreateContainerIfNotExistsAsync(setting.GetDocDbMainCollectionName(), "/code", throughput: setting.GetDocDbThroughput());
            });
        }

        // Drivers
        public async Task<DriverItem> RetrieveDriver(string code)
        {
            var error = "";
            //double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(code))
                    throw new Exception("Code is null or empty string!!");

                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                // NOTE: ReadDocumentAsync is really fast in Cosmos as it bypasses all indexing...but it requires the doc ID
                var docId = $"{code.ToUpper()}-{ItemCollectionTypes.Driver}";

                return await (await GetCosmosContainer()).ReadItemAsync<DriverItem>(docId, new PartitionKey(code.ToUpper()));
            }
            catch (Exception ex)
            {
                // Detect a `Resource Not Found` exception...do not treat it as error
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) && ex.InnerException.Message.IndexOf("Resource Not Found") != -1)
                {
                    return null;
                }
                else
                {
                    error = ex.Message;
                    throw new Exception(ex.Message, ex);
                }
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveDriver - Error: {error}");
            }
        }

        public async Task<List<DriverItem>> RetrieveDrivers(int max = Constants.MAX_RETRIEVE_DOCS)
        {
            var error = "";
            //double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                using (var query = (await GetCosmosContainer()).GetItemLinqQueryable<DriverItem>(requestOptions: new QueryRequestOptions { MaxItemCount = max })
                                .Where(e => e.CollectionType == ItemCollectionTypes.Driver)
                                .OrderByDescending(e => e.UpsertDate)
                                .Take(max)
                                .ToFeedIterator())
                {
                    List<DriverItem> allDocuments = new List<DriverItem>();
                    while (query.HasMoreResults)
                    {
                        var queryResult = await query.ReadNextAsync();
                        //cost += queryResult.RequestCharge;
                        allDocuments.AddRange(queryResult.ToList());
                    }

                    return allDocuments;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveDrivers - Error: {error}");
            }
        }

        public async Task<List<DriverItem>> RetrieveDrivers(double latitude, double longitude, double miles, int max = Constants.MAX_RETRIEVE_DOCS)
        {
            //TODO: Call the main `RetrieveDrivers` method
            return await RetrieveActiveDrivers(max);
        }

        public async Task<List<DriverItem>> RetrieveActiveDrivers(int max = Constants.MAX_RETRIEVE_DOCS)
        {
            var error = "";
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                using (var query = (await GetCosmosContainer()).GetItemLinqQueryable<DriverItem>(requestOptions: new QueryRequestOptions { MaxItemCount = max })
                                .Where(e => e.CollectionType == ItemCollectionTypes.Driver && e.IsAcceptingRides == true)
                                .OrderByDescending(e => e.UpsertDate)
                                .Take(max)
                                .ToFeedIterator())
                {
                    List<DriverItem> allDocuments = new List<DriverItem>();
                    while (query.HasMoreResults)
                    {
                        var queryResult = await query.ReadNextAsync();
                        //cost += queryResult.RequestCharge;
                        allDocuments.AddRange(queryResult.ToList());
                    }

                    return allDocuments;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveDrivers - Error: {error}");
            }
        }

        public async Task<int> RetrieveDriversCount()
        {
            var error = "";
            // NOTE: Unfortunately I could not find a way to get the cost when performing `count` against Cosmos
            //double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                return await (await GetCosmosContainer()).GetItemLinqQueryable<DriverItem>()
                                .Where(e => e.CollectionType == ItemCollectionTypes.Driver)
                                .CountAsync();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveDriversCount - Error: {error}");
            }
        }

        public async Task<DriverItem> UpsertDriver(DriverItem driver, bool isIgnoreChangeFeed = false)
        {
            var error = "";
            //double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                // Just making sure...
                driver.CollectionType = ItemCollectionTypes.Driver;
                driver.UpsertDate = DateTime.Now;

                if (driver.Id == "")
                {
                    if (string.IsNullOrEmpty(driver.Code))
                        driver.Code = Utilities.GenerateRandomAlphaNumeric(8);

                    driver.Id = $"{driver.Code}-{driver.CollectionType}";
                }

                var response = await (await GetCosmosContainer()).UpsertItemAsync(driver, new PartitionKey(driver.Code.ToUpper()));

                if (!isIgnoreChangeFeed)
                {
                    await _changeNotifierService.DriverChanged(driver);
                }

                return response.Resource;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - UpsertDriver - Error: {error}");
            }
        }

        public async Task<string> UpsertDriverLocation(DriverLocationItem location, bool isIgnoreChangeFeed = false)
        {
            var error = "";
            string resourceId = "";

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                DriverItem driver = await RetrieveDriver(location.DriverCode);
                if (driver == null)
                    throw new Exception($"Unable to locate a driver with code {location.DriverCode}");

                // Just making sure...
                location.CollectionType = ItemCollectionTypes.DriverLocation;
                location.UpsertDate = DateTime.Now;

                if (location.Id == "")
                    location.Id = $"{location.DriverCode}-{location.CollectionType}-{Guid.NewGuid().ToString()}";

                //TODO: DriverLocationItem has no Code property, which is the PK 😬
                var response = await (await GetCosmosContainer()).UpsertItemAsync(location);

                // Also update the driver latest location
                driver.Latitude = location.Latitude;
                driver.Longitude = location.Longitude;
                await UpsertDriver(driver, isIgnoreChangeFeed);

                if (!isIgnoreChangeFeed)
                {
                    //TODO: This is one way we can react to changes in Cosmos and perhaps enqueue a message or whatever
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - UpsertDriverLocation - Error: {error}");
            }

            //TODO: Always returns empty string...
            return resourceId;
        }

        public async Task<List<DriverLocationItem>> RetrieveDriverLocations(string code, int max = Constants.MAX_RETRIEVE_DOCS)
        {
            var error = "";
            //double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                using (var query = (await GetCosmosContainer()).GetItemLinqQueryable<DriverLocationItem>(requestOptions: new QueryRequestOptions { MaxItemCount = max })
                                .Where(e => e.CollectionType == ItemCollectionTypes.DriverLocation)
                                .OrderByDescending(e => e.UpsertDate)
                                .Take(max)
                                .ToFeedIterator())
                {
                    List<DriverLocationItem> allDocuments = new List<DriverLocationItem>();
                    while (query.HasMoreResults)
                    {
                        var queryResult = await query.ReadNextAsync();
                        //cost += queryResult.RequestCharge;
                        allDocuments.AddRange(queryResult.ToList());
                    }

                    return allDocuments;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveDriverLocations - Error: {error}");
            }
        }

        public async Task DeleteDriver(string code)
        {
            var error = "";

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                var driver = await RetrieveDriver(code);
                if (driver == null)
                    throw new Exception($"Unable to locate a driver with code {code}");

                var response = await (await GetCosmosContainer()).DeleteItemAsync<DriverItem>(driver.Id, new PartitionKey(code.ToUpper()));

                //TODO: Also delete the associated driver location items
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - DeleteDriver - Error: {error}");
            }
        }

        // Trips
        public async Task<TripItem> RetrieveTrip(string code)
        {
            var error = "";

            try
            {
                if (string.IsNullOrEmpty(code))
                    throw new Exception("Code is null or empty string!!");

                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                // NOTE: ReadDocumentAsync is really fast in Cosmos as it bypasses all indexing...but it requires the doc ID
                var docId = $"{code.ToUpper()}-{ItemCollectionTypes.Trip}";
                return await (await GetCosmosContainer()).ReadItemAsync<TripItem>(docId, new PartitionKey(code.ToUpper()));
            }
            catch (Exception ex)
            {
                // Detect a `Resource Not Found` exception...do not treat it as error
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) && ex.InnerException.Message.IndexOf("Resource Not Found") != -1)
                {
                    return null;
                }
                else
                {
                    error = ex.Message;
                    throw new Exception(ex.Message, ex);
                }
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveTrip - Error: {error}");
            }
        }

        public async Task<List<TripItem>> RetrieveTrips(int max = Constants.MAX_RETRIEVE_DOCS)
        {
            var error = "";

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                using (var query = (await GetCosmosContainer()).GetItemLinqQueryable<TripItem>(requestOptions: new QueryRequestOptions { MaxItemCount = max })
                                .Where(e => e.CollectionType == ItemCollectionTypes.Trip)
                                .OrderByDescending(e => e.UpsertDate)
                                .Take(max)
                                .ToFeedIterator())
                {
                    List<TripItem> allDocuments = new List<TripItem>();
                    while (query.HasMoreResults)
                    {
                        var queryResult = await query.ReadNextAsync();
                        allDocuments.AddRange(queryResult.ToList());
                    }

                    return allDocuments;
                }

            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveTrips - Error: {error}");
            }
        }

        public async Task<List<TripItem>> RetrieveTrips(double latitude, double longitude, double miles, int max = Constants.MAX_RETRIEVE_DOCS)
        {
            //TODO: call the main `RetrieveTrips` method
            return await RetrieveTrips(max);
        }

        public async Task<List<TripItem>> RetrieveActiveTrips(int max = Constants.MAX_RETRIEVE_DOCS)
        {
            var error = "";
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                using (var query = (await GetCosmosContainer()).GetItemLinqQueryable<TripItem>(requestOptions: new QueryRequestOptions { MaxItemCount = max })
                                .Where(e => e.CollectionType == ItemCollectionTypes.Trip && e.EndDate == null)
                                .OrderByDescending(e => e.UpsertDate)
                                .Take(max)
                                .ToFeedIterator())
                {
                    List<TripItem> allDocuments = new List<TripItem>();
                    while (query.HasMoreResults)
                    {
                        var queryResult = await query.ReadNextAsync();
                        allDocuments.AddRange(queryResult.ToList());
                    }

                    return allDocuments;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveTrips - Error: {error}");
            }
        }

        public async Task<int> RetrieveTripsCount()
        {
            var error = "";

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                return await (await GetCosmosContainer()).GetItemLinqQueryable<TripItem>()
                                .Where(e => e.CollectionType == ItemCollectionTypes.Trip)
                                .CountAsync();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveTripsCount - Error: {error}");
            }
        }

        public async Task<int> RetrieveActiveTripsCount()
        {
            var error = "";
            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                return await (await GetCosmosContainer()).GetItemLinqQueryable<TripItem>()
                    .Where(e => e.CollectionType == ItemCollectionTypes.Trip & e.EndDate == null)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveActiveTripsCount - Error: {error}");
            }
        }

        public async Task<TripItem> UpsertTrip(TripItem trip, bool isIgnoreChangeFeed = false)
        {
            var error = "";

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                // Just making sure...
                trip.CollectionType = ItemCollectionTypes.Trip;
                trip.UpsertDate = DateTime.Now;

                bool blInsert = false;
                if (trip.Id == "")
                {
                    trip.Code = Utilities.GenerateRandomAlphaNumeric(8);
                    trip.Id = $"{trip.Code}-{trip.CollectionType}";
                    blInsert = true;
                }

                if (trip.EndDate != null)
                    trip.Duration = ((DateTime)trip.EndDate - trip.StartDate).TotalSeconds;

                var response = await (await GetCosmosContainer()).UpsertItemAsync(trip, new PartitionKey(trip.Code.ToUpper()));

                if (!isIgnoreChangeFeed && blInsert)
                {
                    await _changeNotifierService.TripCreated(trip, await RetrieveActiveTripsCount());
                }

                return response.Resource;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - UpsertTrip - Error: {error}");
            }
        }

        public async Task DeleteTrip(string code)
        {
            var error = "";

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                var trip = await RetrieveTrip(code);
                if (trip == null)
                    throw new Exception($"Unable to locate a trip with code {code}");

                var response = await (await GetCosmosContainer()).DeleteItemAsync<TripItem>(trip.Id, new PartitionKey(code.ToUpper()));
                await _changeNotifierService.TripDeleted(trip);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - DeleteTrip - Error: {error}");
            }
        }


        // High-level methods
        public async Task<TripItem> AssignTripAvailableDrivers(TripItem trip, List<DriverItem> drivers)
        {
            var error = "";

            try
            {
                trip.AvailableDrivers = drivers;
                trip = await UpsertTrip(trip, true);
                return trip;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - AssignTripAvailableDrivers - Error: {error}");
            }
        }

        public async Task<TripItem> AssignTripDriver(TripItem trip, string driverCode)
        {
            var error = "";

            try
            {
                var driver = await RetrieveDriver(driverCode);
                driver.IsAcceptingRides = false;
                // NOTE: Make sure the driver is not at the destination already!!!
                driver.Latitude = trip.Source.Latitude;
                driver.Longitude = trip.Source.Longitude;
                await UpsertDriver(driver, true);

                trip.Driver = driver;
                trip.AcceptDate = DateTime.Now;
                trip = await UpsertTrip(trip, true);
                return trip;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - AssignTripDriver - Error: {error}");
            }
        }

        public async Task RecycleTripDriver(TripItem trip)
        {
            var error = "";

            try
            {
                var driver = trip.Driver;
                if (driver != null)
                {
                    driver = await RetrieveDriver(trip.Driver.Code);
                    driver.IsAcceptingRides = true;
                    await UpsertDriver(driver, true);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RecycleTripDriver - Error: {error}");
            }
        }

        public async Task<TripItem> CheckTripCompletion(TripItem trip)
        {
            var error = "";

            try
            {
                // TODO: We need a way to determine when the trip is over
                // TODO: It is ridiculous, but for now, I am checking to see if the driver location equals the trip destination location :-)
                var driver = await RetrieveDriver(trip.Driver.Code);
                if (driver != null && driver.Latitude == trip.Destination.Latitude && driver.Longitude == trip.Destination.Longitude)
                {
                    trip.EndDate = DateTime.Now;
                }

                trip.MonitorIterations++;
                trip = await UpsertTrip(trip, true);
                return trip;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - CheckTripCompletion - Error: {error}");
            }
        }

        public async Task<TripItem> AbortTrip(TripItem trip)
        {
            var error = "";

            try
            {
                trip.EndDate = DateTime.Now;
                trip.IsAborted = true;
                trip = await UpsertTrip(trip, true);

                var driver = trip.Driver;
                if (driver != null)
                {
                    driver = await RetrieveDriver(trip.Driver.Code);
                    driver.IsAcceptingRides = true;
                    await UpsertDriver(driver, true);
                }

                return trip;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - AbortTrip - Error: {error}");
            }
        }

        //**** PRIVATE METHODS ****//

        private Task<Container> GetCosmosContainer() => _cosmosContainer.Value;
    }
}
