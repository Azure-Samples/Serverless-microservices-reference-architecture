using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ServerlessMicroservices.Models;
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
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            TripItem trip = context.GetInput<TripItem>();

            if (!context.IsReplaying)
                log.LogInformation($"ManageTrip {trip.Code} starting.....");

            string driverAcceptCode = "Unknown";

            try
            {
                // TODO: Validate the trip's passenger
                var passenger = new PassengerItem();

                // Find and notify drivers
                List<DriverItem> availableDrivers = await context.CallActivityAsync<List<DriverItem>>("A_TM_FindNNotifyDrivers", trip);
                if (availableDrivers.Count == 0)
                    throw new Exception("No drivers available!!");

                // Wait for either a timer or an external event to signal that a driver accepted the trip
                using (var cts = new CancellationTokenSource())
                {
                    var timeoutAt = context.CurrentUtcDateTime.AddSeconds(120);
                    var timeoutTask = context.CreateTimer(timeoutAt, cts.Token);
                    var acknowledgeTask = context.WaitForExternalEvent<string>(Constants.TRIP_DRIVER_ACKNOWLEDGE);

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
                    throw new Exception($"Out of {availableDrivers.Count}, no driver responed on time!!");

                // Notify other drivers
                var acceptedDriver = availableDrivers.SingleOrDefault(d => d.Code == driverAcceptCode);
                if (acceptedDriver == null)
                    throw new Exception($"Data integrity - received an ack from an invalid driver {driverAcceptCode}!!");

                var otherDrivers = availableDrivers.Remove(acceptedDriver);
                await context.CallActivityAsync("A_TM_NotifyOtherDrivers", otherDrivers);

                // Update the trip
                await context.CallActivityAsync("A_TM_UpdateTrip", new TripInfo
                {
                    Trip = trip,
                    Passenger = passenger,
                    Driver = acceptedDriver
                });

                // Notify the passenger
                await context.CallActivityAsync("A_TM_NotifyPassenger", passenger);

                // Trigger to create a trip monitor orchestrator whose job is to monitor the driver updates
                await context.CallActivityAsync("A_TM_CreateTripMonitor", trip);
            }
            catch (Exception e)
            {
                if (!context.IsReplaying)
                    log.LogInformation($"Caught an error from an activity: {e.Message}");

                await context.CallActivityAsync<string>("A_TM_Cleanup", trip);

                return new
                {
                    Error = "Failed to process trip",
                    Message = e.Message
                };
            }

            return new
            {
                Trip = trip,
                AcceptCode = driverAcceptCode
            };
        }

        [FunctionName("A_TM_FindNNotifyDrivers")]
        public static async Task<List<DriverItem>> FindNNotifyDrivers([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"FindNNotifyDrivers for {trip.Code} starting....");
            // TODO: Query Cosmos
            List<DriverItem> availableDrivers = new List<DriverItem>();
            foreach (var driver in availableDrivers)
            {
                await Notify(driver);
            }

            return availableDrivers;
        }

        [FunctionName("A_TM_NotifyOtherDrivers")]
        public static async Task NotifyOtherDrivers([ActivityTrigger] List<DriverItem> otherDrivers,
            ILogger log)
        {
            log.LogInformation($"NotifyOtherDrivers starting....");
            foreach (var driver in otherDrivers)
            {
                await Notify(driver);
            }
        }

        [FunctionName("A_TM_UpdateTrip")]
        public static async Task UpdateTrip([ActivityTrigger] TripInfo info,
            ILogger log)
        {
            log.LogInformation($"UpdateTrip starting....");
            // TODO: Update Cosmos
            await Notify(info.Trip);
        }

        [FunctionName("A_TM_NotifyPassenger")]
        public static async Task NotifyPassenger([ActivityTrigger] PassengerItem passenger,
            ILogger log)
        {
            log.LogInformation($"NotifyPassenger starting....");
            await Notify(passenger);
        }

        // Out does does not work in Async methods!!!
        [FunctionName("A_TM_CreateTripMonitor")]
        public static void CreateTripMonitor([ActivityTrigger] TripItem trip,
            [Queue("trip-monitors", Connection = "AzureWebJobsStorage")] out TripItem queueTrip,
            ILogger log)
        {
            log.LogInformation($"CreateTripMonitor starting....");
            // Enqueue the trip to be monitored 
            queueTrip = trip;
        }

        [FunctionName("A_TM_Cleanup")]
        public static async Task Cleanup([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"Cleanup for {trip.Code} starting....");
            //TODO: 
        }

        // *** PRIVATE ***//
        private static async Task Notify(DriverItem driver)
        {
             // This will most likely enqueue an item to SignalR service via INotifyService           
        }

        private static async Task Notify(PassengerItem passenger)
        {
            // This will most likely enqueue an item to SignalR service via INotifyService           
        }

        private static async Task Notify(TripItem driver)
        {
            // This will most likely enqueue an item to SignalR service via INotifyService           
        }
    }
}

public class TripInfo
{
    public TripItem Trip { get; set; }
    public PassengerItem Passenger { get; set; }
    public DriverItem Driver { get; set; }
}
