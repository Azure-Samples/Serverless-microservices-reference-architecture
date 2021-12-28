using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;
using ServerlessMicroservices.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Orchestrators
{
    [StorageAccount("AzureWebJobsStorage")]
    public static class TripManagerOrchestrator
    {
        [FunctionName("O_ManageTrip")]
        public static async Task<object> ManageTrip(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            TripItem trip = context.GetInput<TripItem>();

            if (!context.IsReplaying)
                log.LogInformation($"ManageTrip {trip.Code} starting.....");

            string driverAcceptCode = "Unknown";

            try
            {
                // Find and notify drivers
                trip = await context.CallActivityAsync<TripItem>("A_TM_FindNNotifyDrivers", trip);
                if (trip.AvailableDrivers.Count == 0)
                    throw new Exception("No drivers available!!");

                // Wait for either a timer or an external event to signal that a driver accepted the trip
                using (var cts = new CancellationTokenSource())
                {
                    var timeoutAt = context.CurrentUtcDateTime.AddSeconds(ServiceFactory.GetSettingService().GetDriversAcknowledgeMaxWaitPeriodInSeconds());
                    var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                    var acknowledgeTask = context.WaitForExternalEvent<string>(Constants.TRIP_DRIVER_ACCEPT_EVENT);

                    var winner = await Task.WhenAny(acknowledgeTask, timeoutTask);
                    if (winner == acknowledgeTask)
                    {
                        driverAcceptCode = acknowledgeTask.Result;
                        cts.Cancel(); // we should cancel the timeout task
                    }
                    else
                    {
                        driverAcceptCode = "Timed Out";
                    }
                }

                if (driverAcceptCode == "Timed Out")
                    throw new Exception($"Did not receive an ack from any driver in {ServiceFactory.GetSettingService().GetDriversAcknowledgeMaxWaitPeriodInSeconds()} seconds!!");

                var acceptedDriver = trip.AvailableDrivers.SingleOrDefault(d => d.Code == driverAcceptCode);
                if (acceptedDriver == null)
                    throw new Exception($"Data integrity - received an ack from an invalid driver {driverAcceptCode}!!");

                // Assign trip driver
                trip = await context.CallActivityAsync<TripItem>("A_TM_AssignTripDriver",  new TripInfo
                {
                    Trip = trip,
                    Driver = acceptedDriver
                });

                // Notify other drivers that their service is no longer needed
                await context.CallActivityAsync("A_TM_NotifyOtherDrivers", trip);

                // Notify the passenger that the trip is starting
                await context.CallActivityAsync("A_TM_NotifyPassenger", trip);

                // Trigger to create a trip monitor orchestrator whose job is to monitor the trip for completion
                await context.CallActivityAsync("A_TM_CreateTripMonitor", trip);
            }
            catch (Exception e)
            {
                if (!context.IsReplaying)
                    log.LogInformation($"Caught an error from an activity: {e.Message}");

                trip.Error = e.Message;
                await context.CallActivityAsync<string>("A_TM_Cleanup", trip);

                return new
                {
                    Error = "Failed to process trip",
                    Message = e.Message
                };
            }

            return new
            {
                Trip = trip
            };
        }

        [FunctionName("A_TM_FindNNotifyDrivers")]
        public static async Task<TripItem> FindNNotifyDrivers([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"FindNNotifyDrivers for {trip.Code} starting....");
            List<DriverItem> availableDrivers = new List<DriverItem>();

            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                availableDrivers = await persistenceService.RetrieveDrivers(trip.Source.Latitude, trip.Source.Longitude, ServiceFactory.GetSettingService().GetDriversLocationRadiusInMiles());
                if (availableDrivers.Count > 0)
                    trip = await persistenceService.AssignTripAvailableDrivers(trip, availableDrivers);
            }
            else
            {
                //TODO: Go through the Driver APIs
            }

            foreach (var driver in availableDrivers)
            {
                //TODO: Out of scope
            }

            if (availableDrivers.Count > 0)
                await Externalize(trip, Constants.EVG_SUBJECT_TRIP_DRIVERS_NOTIFIED);

            return trip;
        }

        [FunctionName("A_TM_AssignTripDriver")]
        public static async Task<TripItem> AssignTripDriver([ActivityTrigger] TripInfo tripInfo,
            ILogger log)
        {
            log.LogInformation($"AssignTripDriver starting....");
            var trip = tripInfo.Trip;
            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                trip = await persistenceService.AssignTripDriver(trip, tripInfo.Driver.Code);
            }
            else
            {
                //TODO: Go through the Trips APIs
            }

            await Externalize(trip, Constants.EVG_SUBJECT_TRIP_DRIVER_PICKED);
            return trip;
        }

        [FunctionName("A_TM_NotifyOtherDrivers")]
        public static async Task NotifyOtherDrivers([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"NotifyOtherDrivers starting....");
            trip.AvailableDrivers.Remove(trip.Driver);
            foreach (var driver in trip.AvailableDrivers)
            {
                //TODO: Out of scope
            }
        }

        [FunctionName("A_TM_NotifyPassenger")]
        public static async Task NotifyPassenger([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"NotifyPassenger starting....");
            //TODO: Out of scope
        }

        // Out does does not work in Async methods!!!
        [FunctionName("A_TM_CreateTripMonitor")]
        public static void CreateTripMonitor([ActivityTrigger] TripItem trip,
            [Queue("%TripMonitorsQueue%", Connection = "AzureWebJobsStorage")] out string queueTripCode,
            ILogger log)
        {
            log.LogInformation($"CreateTripMonitor starting....");
            // Enqueue the trip code to be monitored 
            queueTripCode = trip.Code;

            // Send an event telemetry
            ServiceFactory.GetLoggerService().Log("Trip monitored", new Dictionary<string, string>
                {
                    {"Code", trip.Code },
                    {"Passenger", $"{trip.Passenger.FirstName} {trip.Passenger.LastName}" },
                    {"Destination", $"{trip.Destination.Latitude} - {trip.Destination.Longitude}" },
                    {"Mode", $"{trip.Type}" }
                });
        }

        [FunctionName("A_TM_Cleanup")]
        public static async Task Cleanup([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"Cleanup for {trip.Code} starting....");

            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                trip = await persistenceService.AbortTrip(trip);
            }
            else
            {
                //TODO: Go through the Trips APIs
            }

            await Externalize(trip, Constants.EVG_SUBJECT_TRIP_ABORTED);

            // Send an event telemetry
            ServiceFactory.GetLoggerService().Log("Trip aborted", new Dictionary<string, string>
                {
                    {"Code", trip.Code },
                    {"Passenger", $"{trip.Passenger.FirstName} {trip.Passenger.LastName}" },
                    {"Destination", $"{trip.Destination.Latitude} - {trip.Destination.Longitude}" },
                    {"Mode", $"{trip.Type}" }
                });
        }

        // *** PRIVATE ***//
        private static async Task Externalize(TripItem trip, string subject)
        {
            await Utilities.TriggerEventGridTopic<TripItem>(null, trip, Constants.EVG_EVENT_TYPE_MANAGER_TRIP, subject, ServiceFactory.GetSettingService().GetTripExternalizationsEventGridTopicUrl(), ServiceFactory.GetSettingService().GetTripExternalizationsEventGridTopicApiKey());
        }
    }

    public class TripInfo
    {
        public TripItem Trip { get; set; }
        public DriverItem Driver { get; set; }
    }
}
