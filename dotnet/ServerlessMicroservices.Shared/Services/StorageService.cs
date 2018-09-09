using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using ServerlessMicroservices.Models;
using System;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public class StorageService : IStorageService
    {
        public const string LOG_TAG = "StorageService";

        private bool _isStorageInitialized = false;

        private ISettingService _settingService;
        private ILoggerService _loggerService;

        private CloudQueue _tripManagersQueue;
        private CloudQueue _tripMonitorsQueue;
        private CloudQueue _tripDemosQueue;
        private CloudQueue _tripDriversQueue;

        public StorageService(ISettingService setting, ILoggerService logger)
        {
            _settingService = setting;
            _loggerService = logger;
        }

        public async Task Enqueue(TripItem trip)
        {
            await InitializeStorage();

            if (_tripManagersQueue != null)
            {
                var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(trip));
                await _tripManagersQueue.AddMessageAsync(queueMessage);
            }
        }

        public async Task Enqueue(TripDemoState tripDemoState)
        {
            await InitializeStorage();

            if (_tripDemosQueue != null)
            {
                var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(tripDemoState));
                await _tripDemosQueue.AddMessageAsync(queueMessage);
            }
        }

        public async Task Enqueue(string tripCode, string driverCode)
        {
            await InitializeStorage();

            if (_tripDriversQueue != null)
            {
                var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(new TripDriver
                {
                    TripCode = tripCode,
                    DriverCode = driverCode
                }));
                await _tripDriversQueue.AddMessageAsync(queueMessage);
            }
        }

        // PRIVATE//

        private async Task InitializeStorage()
        {
            if (_isStorageInitialized)
                return;

            var error = "";

            try
            {
                if (!string.IsNullOrEmpty(_settingService.GetStorageAccount()))
                {
                    // Queues Initialization
                    var queueStorageAccount = CloudStorageAccount.Parse(_settingService.GetStorageAccount());
                    // Get context object for working with queues, and set a default retry policy appropriate for a web user interface.
                    var queueClient = queueStorageAccount.CreateCloudQueueClient();
                    //queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

                    var tripManagersQueueName = _settingService.GetTripManagersQueueName();
                    if (!string.IsNullOrEmpty(tripManagersQueueName))
                    {
                        _tripManagersQueue = queueClient.GetQueueReference(tripManagersQueueName);
                        await _tripManagersQueue.CreateIfNotExistsAsync();
                    }
                    else
                        _loggerService.Log("tripManagersQueueName is empty");

                    var tripMonitorsQueueName = _settingService.GetTripMonitorsQueueName();
                    if (!string.IsNullOrEmpty(tripMonitorsQueueName))
                    {
                        _tripMonitorsQueue = queueClient.GetQueueReference(tripMonitorsQueueName);
                        await _tripMonitorsQueue.CreateIfNotExistsAsync();
                    }
                    else
                        _loggerService.Log("tripMonitorsQueueName is empty");

                    var tripDemosQueueName = _settingService.GetTripDemosQueueName();
                    if (!string.IsNullOrEmpty(tripDemosQueueName))
                    {
                        _tripDemosQueue = queueClient.GetQueueReference(tripDemosQueueName);
                        await _tripDemosQueue.CreateIfNotExistsAsync();
                    }
                    else
                        _loggerService.Log("tripDemosQueueName is empty");

                    var tripDriversQueueName = _settingService.GetTripDriversQueueName();
                    if (!string.IsNullOrEmpty(tripDriversQueueName))
                    {
                        _tripDriversQueue = queueClient.GetQueueReference(tripDriversQueueName);
                        await _tripDemosQueue.CreateIfNotExistsAsync();
                    }
                    else
                        _loggerService.Log("tripDriversQueueName is empty");

                    _isStorageInitialized = true;
                }
            }
            catch (Exception ex)
            {
                error = $"Error while initializing storage: {ex.Message}";
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - InitializeStorage - Error: {error}");
            }
        }
    }
}
