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
        private IAnalyticService _analyticService;

        public CosmosPersistenceService(ISettingService setting, ILoggerService logger, IAnalyticService anayltic)
        {
            _settingService = setting;
            _loggerService = logger;
            _analyticService = anayltic;

            _docDbEndpointUri = _settingService.GetDocDbEndpointUri();
            _docDbPrimaryKey = _settingService.GetDocDbApiKey();

            _docDbDatabaseName = _settingService.GetDocDbRideShareDatabaseName();
            _docDbDigitalMainCollectionName = _settingService.GetDocDbMainCollectionName();
        }

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
                var request = (await GetDocDBClient(_settingService)).ReadDocumentAsync<DriverItem>(UriFactory.CreateDocumentUri(_docDbDatabaseName, _docDbDigitalMainCollectionName, docId)/*, options*/);
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
                // TODO: Do something with the cost and error 
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
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = max };

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
                // TODO: Do something with the cost and error 
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
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };

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
                // TODO: Do something with the cost and error 
            }
        }

        public async Task<string> UpsertDriver(DriverItem driver, bool isIgnoreChangeFeed = false)
        {
            var error = "";
            double cost = 0;
            string resourceId = "";

            try
            {
                if (string.IsNullOrEmpty(_docDbDigitalMainCollectionName))
                    throw new Exception("No Digital Main collection defined!");

                // Just making sure...
                driver.CollectionType = ItemCollectionTypes.Driver;
                driver.UpsertDate = DateTime.Now;

                if (driver.Id == "")
                    driver.Id = $"{driver.Code}-{driver.CollectionType}";

                var response = await(await GetDocDBClient(_settingService)).UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_docDbDatabaseName, _docDbDigitalMainCollectionName), driver);
                cost = response.RequestCharge;

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
                // TODO: Do something with the cost and error 
            }

            return resourceId;
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
                // TODO: Do something with the cost and error 
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
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = max };

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
                // TODO: Do something with the cost and error 
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
                //RequestOptions requestOptions = new RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(driver.Code.ToUpper()) };
                var response = await(await GetDocDBClient(_settingService)).DeleteDocumentAsync(link /*,requestOptions*/);
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
                // TODO: Do something with the cost and error 
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

                _docDbSingletonClient = new DocumentClient(new Uri(docDbEndpointUrl), docDbApiKey, new ConnectionPolicy()
                {
                    ConnectionMode = ConnectionMode.Direct,
                    ConnectionProtocol = Protocol.Tcp,
                    MaxConnectionLimit = 200
                });


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
