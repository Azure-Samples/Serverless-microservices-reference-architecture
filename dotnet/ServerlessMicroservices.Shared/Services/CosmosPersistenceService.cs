using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
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

        private static DocumentClient _docDbSingletonClient;

        private ISettingService _settingService;
        private ILoggerService _loggerService;
        private IChangeNotifierService _changeNotifierService;

        public CosmosPersistenceService(ISettingService setting, ILoggerService logger, IChangeNotifierService changeService)
        {
            _settingService = setting;
            _loggerService = logger;
            _changeNotifierService = changeService;

            _docDbEndpointUri = _settingService.GetDocDbEndpointUri();
            _docDbPrimaryKey = _settingService.GetDocDbApiKey();

            _docDbDatabaseName = _settingService.GetDocDbRideShareDatabaseName();
            _docDbDigitalMainCollectionName = _settingService.GetDocDbMainCollectionName();
        }

        // Drivers
        public async Task<DriverItem> RetrieveDriver(string code)
        {
            var error = "";
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(code))
                    throw new Exception("Code is null or empty string!!");

                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                // NOTE: ReadDocumentAsync is really fast in Cosmos as it bypasses all indexing...but it requires the doc ID
                var docId = $"{code.ToUpper()}-{ItemCollectionTypes.Driver}";
                RequestOptions options = new RequestOptions() { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                var request = (await GetDocDBClient(_settingService)).ReadDocumentAsync<DriverItem>(UriFactory.CreateDocumentUri(_docDbDatabaseName, _docDbDigitalMainCollectionName, docId), options);
                cost = request.Result.RequestCharge;
                return request.Result;
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
                    throw ex;
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
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                //FeedOptions queryOptions = new FeedOptions { MaxItemCount = max, PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = max, EnableCrossPartitionQuery = true };

                var query = (await GetDocDBClient(_settingService)).CreateDocumentQuery<DriverItem>(
                                UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), queryOptions)
                                .Where(e => e.CollectionType == ItemCollectionTypes.Driver)
                                .OrderByDescending(e => e.UpsertDate)
                                .Take(max)
                                .AsDocumentQuery();

                List<DriverItem> allDocuments = new List<DriverItem>();
                while (query.HasMoreResults)
                {
                    var queryResult = await query.ExecuteNextAsync<DriverItem>();
                    cost += queryResult.RequestCharge;
                    allDocuments.AddRange(queryResult.ToList());
                }

                return allDocuments;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
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

                //FeedOptions queryOptions = new FeedOptions { MaxItemCount = max, PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = max, EnableCrossPartitionQuery = true };

                var query = (await GetDocDBClient(_settingService)).CreateDocumentQuery<DriverItem>(
                                UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), queryOptions)
                                .Where(e => e.CollectionType == ItemCollectionTypes.Driver && e.IsAcceptingRides == true)
                                .OrderByDescending(e => e.UpsertDate)
                                .Take(max)
                                .AsDocumentQuery();

                List<DriverItem> allDocuments = new List<DriverItem>();
                while (query.HasMoreResults)
                {
                    var queryResult = await query.ExecuteNextAsync<DriverItem>();
                    cost += queryResult.RequestCharge;
                    allDocuments.AddRange(queryResult.ToList());
                }

                return allDocuments;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
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
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                //FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1, PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };

                return await(await GetDocDBClient(_settingService)).CreateDocumentQuery<DriverItem>(
                            UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), queryOptions)
                            .Where(e => e.CollectionType == ItemCollectionTypes.Driver)
                            .CountAsync();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveDriversCount - Error: {error}");
            }
        }

        public async Task<DriverItem> UpsertDriver(DriverItem driver, bool isIgnoreChangeFeed = false)
        {
            var error = "";
            double cost = 0;

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

                var response = await(await GetDocDBClient(_settingService)).UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), driver);
                cost = response.RequestCharge;

                if (!isIgnoreChangeFeed)
                {
                    await _changeNotifierService.DriverChanged(driver);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - UpsertDriver - Error: {error}");
            }

            return driver;
        }

        public async Task<string> UpsertDriverLocation(DriverLocationItem location, bool isIgnoreChangeFeed = false)
        {
            var error = "";
            double cost = 0;
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

                var response = await (await GetDocDBClient(_settingService)).UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), location);
                cost = response.RequestCharge;

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
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - UpsertDriverLocation - Error: {error}");
            }

            return resourceId;
        }

        public async Task<List<DriverLocationItem>> RetrieveDriverLocations(string code, int max = Constants.MAX_RETRIEVE_DOCS)
        {
            var error = "";
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                //FeedOptions queryOptions = new FeedOptions { MaxItemCount = max, PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = max, EnableCrossPartitionQuery = true };

                var query = (await GetDocDBClient(_settingService)).CreateDocumentQuery<DriverItem>(
                                UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), queryOptions)
                                .Where(e => e.CollectionType == ItemCollectionTypes.DriverLocation)
                                .OrderByDescending(e => e.UpsertDate)
                                .Take(max)
                                .AsDocumentQuery();

                List<DriverLocationItem> allDocuments = new List<DriverLocationItem>();
                while (query.HasMoreResults)
                {
                    var queryResult = await query.ExecuteNextAsync<DriverLocationItem>();
                    cost += queryResult.RequestCharge;
                    allDocuments.AddRange(queryResult.ToList());
                }

                return allDocuments;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveDriverLocations - Error: {error}");
            }
        }

        public async Task DeleteDriver(string code)
        {
            var error = "";
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                var driver = await RetrieveDriver(code);
                if (driver == null)
                    throw new Exception($"Unable to locate a driver with code {code}");

                var link = (string)driver.Self;
                RequestOptions requestOptions = new RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                var response = await(await GetDocDBClient(_settingService)).DeleteDocumentAsync(link, requestOptions);
                cost += response.RequestCharge;

                //TODO: Also delete the associated driver location items
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
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
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(code))
                    throw new Exception("Code is null or empty string!!");

                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                // NOTE: ReadDocumentAsync is really fast in Cosmos as it bypasses all indexing...but it requires the doc ID
                var docId = $"{code.ToUpper()}-{ItemCollectionTypes.Trip}";
                RequestOptions options = new RequestOptions() { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                var request = (await GetDocDBClient(_settingService)).ReadDocumentAsync<TripItem>(UriFactory.CreateDocumentUri(_docDbDatabaseName, _docDbDigitalMainCollectionName, docId), options);
                cost = request.Result.RequestCharge;
                return request.Result;
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
                    throw ex;
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
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                //FeedOptions queryOptions = new FeedOptions { MaxItemCount = max, PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = max, EnableCrossPartitionQuery = true };

                var query = (await GetDocDBClient(_settingService)).CreateDocumentQuery<TripItem>(
                                UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), queryOptions)
                                .Where(e => e.CollectionType == ItemCollectionTypes.Trip)
                                .OrderByDescending(e => e.UpsertDate)
                                .Take(max)
                                .AsDocumentQuery();

                List<TripItem> allDocuments = new List<TripItem>();
                while (query.HasMoreResults)
                {
                    var queryResult = await query.ExecuteNextAsync<TripItem>();
                    cost += queryResult.RequestCharge;
                    allDocuments.AddRange(queryResult.ToList());
                }

                return allDocuments;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
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

                //FeedOptions queryOptions = new FeedOptions { MaxItemCount = max, PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = max, EnableCrossPartitionQuery = true };

                var query = (await GetDocDBClient(_settingService)).CreateDocumentQuery<TripItem>(
                                UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), queryOptions)
                                .Where(e => e.CollectionType == ItemCollectionTypes.Trip && e.EndDate == null)
                                .OrderByDescending(e => e.UpsertDate)
                                .Take(max)
                                .AsDocumentQuery();

                List<TripItem> allDocuments = new List<TripItem>();
                while (query.HasMoreResults)
                {
                    var queryResult = await query.ExecuteNextAsync<TripItem>();
                    cost += queryResult.RequestCharge;
                    allDocuments.AddRange(queryResult.ToList());
                }

                return allDocuments;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveTrips - Error: {error}");
            }
        }

        public async Task<int> RetrieveTripsCount()
        {
            var error = "";
            // NOTE: Unfortunately I could not find a way to get the cost when performing `count` against Cosmos
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                //FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1, PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };

                return await (await GetDocDBClient(_settingService)).CreateDocumentQuery<TripItem>(
                            UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), queryOptions)
                            .Where(e => e.CollectionType == ItemCollectionTypes.Trip)
                            .CountAsync();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveTripsCount - Error: {error}");
            }
        }

        public async Task<int> RetrieveActiveTripsCount()
        {
            var error = "";
            // NOTE: Unfortunately I could not find a way to get the cost when performing `count` against Cosmos
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                //FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1, PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };

                return await (await GetDocDBClient(_settingService)).CreateDocumentQuery<TripItem>(
                            UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), queryOptions)
                            .Where(e => e.CollectionType == ItemCollectionTypes.Trip & e.EndDate == null)
                            .CountAsync();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - RetrieveActiveTripsCount - Error: {error}");
            }
        }

        public async Task<TripItem> UpsertTrip(TripItem trip, bool isIgnoreChangeFeed = false)
        {
            var error = "";
            double cost = 0;

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

                var response = await (await GetDocDBClient(_settingService)).UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), trip);
                cost = response.RequestCharge;

                if (!isIgnoreChangeFeed && blInsert)
                {
                    await _changeNotifierService.TripCreated(trip, await RetrieveActiveTripsCount());
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - UpsertTrip - Error: {error}");
            }

            return trip;
        }

        public async Task DeleteTrip(string code)
        {
            var error = "";
            double cost = 0;

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                var trip = await RetrieveTrip(code);
                if (trip == null)
                    throw new Exception($"Unable to locate a trip with code {code}");

                var link = (string)trip.Self;
                RequestOptions requestOptions = new RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(code.ToUpper()) };
                var response = await (await GetDocDBClient(_settingService)).DeleteDocumentAsync(link, requestOptions);
                cost += response.RequestCharge;

                await _changeNotifierService.TripDeleted(trip);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
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
                throw new Exception(error);
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
                throw new Exception(error);
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
                throw new Exception(error);
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
                throw new Exception(error);
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
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - AbortTrip - Error: {error}");
            }
        }

        //**** PRIVATE METHODS ****//
        private static async Task<DocumentClient> GetDocDBClient(ISettingService settingService)
        {
            if (_docDbSingletonClient == null)
            {
                var docDbName = settingService.GetDocDbRideShareDatabaseName();
                var docDbEndpointUrl = settingService.GetDocDbEndpointUri();
                var docDbApiKey = settingService.GetDocDbApiKey();

                // TODO: DocumentClient using direct TCP protocol does not work in Linux!!! 
                //_docDbSingletonClient = new DocumentClient(new Uri(docDbEndpointUrl), docDbApiKey, new ConnectionPolicy()
                //{
                //    ConnectionMode = ConnectionMode.Direct,
                //    ConnectionProtocol = Protocol.Tcp,
                //    MaxConnectionLimit = 200
                //});
                _docDbSingletonClient = new DocumentClient(new Uri(docDbEndpointUrl), docDbApiKey);


                // Do each collection seperately as we may need to set different create options for each
                {
                    DocumentCollection collectionDefinition = new DocumentCollection();
                    collectionDefinition.Id = settingService.GetDocDbMainCollectionName();
                    //collectionDefinition.PartitionKey.Paths.Add("/something");
                    collectionDefinition.DefaultTimeToLive = -1;

                    // TIP: If queries are known upfront, index just the properties you need
                    // Can be a mute point since we are using a fixed size and no partition!!!!
                    // But the below commented-out code shows hw this can be accomplished if we change to a partition-based collection
                    collectionDefinition.IndexingPolicy.Automatic = true;
                    collectionDefinition.IndexingPolicy.IndexingMode = IndexingMode.Consistent;
                    collectionDefinition.IndexingPolicy.IncludedPaths.Clear();

                    //IncludedPath path = new IncludedPath();
                    //path.Path = "/upsertDate/?";
                    //path.Indexes.Add(new RangeIndex(DataType.String) { Precision = -1 });
                    //collectionDefinition.IndexingPolicy.IncludedPaths.Add(path);

                    //path = new IncludedPath();
                    //path.Path = "/collectionType/?";
                    //path.Indexes.Add(new RangeIndex(DataType.Number) { Precision = -1 });
                    //collectionDefinition.IndexingPolicy.IncludedPaths.Add(path);

                    //collectionDefinition.IndexingPolicy.ExcludedPaths.Clear();
                    //collectionDefinition.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/*" });

                    DocumentCollection ttlEnabledCollection = await _docDbSingletonClient.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(docDbName), collectionDefinition, new RequestOptions { OfferThroughput = settingService.GetDocDbThroughput() });
                }

            }

            return _docDbSingletonClient;
        }
    }
}
