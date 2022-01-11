using Azure.Core;
using Azure.Storage.Queues;
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

        private QueueClient _tripManagersQueue;
        private QueueClient _tripMonitorsQueue;
        private QueueClient _tripDemosQueue;
        private QueueClient _tripDriversQueue;

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
                await _tripManagersQueue.SendMessageAsync(JsonConvert.SerializeObject(trip)); 
            }
        }

        public async Task Enqueue(TripDemoState tripDemoState)
        {
            await InitializeStorage();

            if (_tripDemosQueue != null)
            {
                await _tripDemosQueue.SendMessageAsync(JsonConvert.SerializeObject(tripDemoState));
            }
        }

        public async Task Enqueue(string tripCode, string driverCode)
        {
            await InitializeStorage();

            if (_tripDriversQueue != null)
            {
                await _tripDriversQueue.SendMessageAsync(JsonConvert.SerializeObject(JsonConvert.SerializeObject(new TripDriver
                {
                    TripCode = tripCode,
                    DriverCode = driverCode
                })));
            }
        }

        // PRIVATE//

        private async Task InitializeStorage()
        {
            //TODO: #45 Not thread safe
            if (_isStorageInitialized)
                return;

            var error = "";

            try
            {

                //TODO: #45 Swallowing errors
                if (!string.IsNullOrEmpty(_settingService.GetStorageAccount()))
                {
                    // Queues Initialization

                    // Retry policy appropriate for a web user interface.
                    var queueClientOptions = new QueueClientOptions();
                    queueClientOptions.Retry.Mode = RetryMode.Fixed;
                    queueClientOptions.Retry.Delay = TimeSpan.FromSeconds(3);
                    queueClientOptions.Retry.MaxRetries = 3;


                    var tripManagersQueueName = _settingService.GetTripManagersQueueName();
                    if (!string.IsNullOrEmpty(tripManagersQueueName))
                        _tripManagersQueue = await InitializeQueueClient(tripManagersQueueName, queueClientOptions);
                    else
                        //TODO: #45 Swallowing errors
                        _loggerService.Log("tripManagersQueueName is empty");

                    var tripMonitorsQueueName = _settingService.GetTripMonitorsQueueName();
                    if (!string.IsNullOrEmpty(tripMonitorsQueueName))
                    {
                        _tripMonitorsQueue = await InitializeQueueClient(tripMonitorsQueueName, queueClientOptions);
                    }
                    else
                        _loggerService.Log("tripMonitorsQueueName is empty");

                    var tripDemosQueueName = _settingService.GetTripDemosQueueName();
                    if (!string.IsNullOrEmpty(tripDemosQueueName))
                    {
                        _tripDemosQueue = await InitializeQueueClient(tripDemosQueueName, queueClientOptions);
                    }
                    else
                        _loggerService.Log("tripDemosQueueName is empty");

                    var tripDriversQueueName = _settingService.GetTripDriversQueueName();
                    if (!string.IsNullOrEmpty(tripDriversQueueName))
                    {
                        _tripDriversQueue = await InitializeQueueClient(tripDriversQueueName, queueClientOptions);
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

        private async Task<QueueClient> InitializeQueueClient(string queueName, QueueClientOptions options)
        {
            var queueClient = new QueueClient(_settingService.GetStorageAccount(), queueName, options);
            await queueClient.CreateIfNotExistsAsync();
            return queueClient;
        }
    }
}
