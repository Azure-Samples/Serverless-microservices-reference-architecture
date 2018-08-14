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
                    var timeoutAt = context.CurrentUtcDateTime.AddSeconds(Constants.WAIT_FOR_DRIVERS_PERIOD_IN_SECONDS);
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
                trip = await context.CallActivityAsync<TripItem>("A_TM_UpdateTrip", new TripInfo
                {
                    Trip = trip,
                    Passenger = passenger,
                    Driver = acceptedDriver
                });

                // Notify the passenger
                await context.CallActivityAsync("A_TM_NotifyPassenger", new TripInfo
                {
                    Trip = trip,
                    Passenger = passenger,
                    Driver = acceptedDriver
                });

                // Externalize the trip
                await context.CallActivityAsync("A_TM_Externalize", trip);

                // Trigger to create a trip monitor orchestrator whose job is to monitor the driver location every x seconds and update the trip
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
                Trip = trip
            };
        }

        [FunctionName("A_TM_FindNNotifyDrivers")]
        public static async Task<List<DriverItem>> FindNNotifyDrivers([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"FindNNotifyDrivers for {trip.Code} starting....");
            // TODO: Query Cosmos directly or call another function to get the result
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
                await Notify(driver, true);
            }
        }

        [FunctionName("A_TM_UpdateTrip")]
        public static async Task<TripItem> UpdateTrip([ActivityTrigger] TripInfo info,
            ILogger log)
        {
            log.LogInformation($"UpdateTrip starting....");
            // TODO: Update Cosmos directly or post to another function
            await Notify(info.Trip);
            return info.Trip;
        }

        [FunctionName("A_TM_NotifyPassenger")]
        public static async Task NotifyPassenger([ActivityTrigger] TripInfo info,
            ILogger log)
        {
            log.LogInformation($"NotifyPassenger starting....");
            await Notify(info.Passenger, info.Trip);
        }

        [FunctionName("A_TM_NotifyExternal")]
        public static async Task NotifyExternal([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"NotifyExternal starting....");
            await Externalize(trip);
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
             //TODO: This will most likely enqueue an item to SignalR service via INotifyService  
             //TODO: This could be an email as well with an Ok button to accept the ride
        }

        private static async Task Notify(DriverItem driver, bool notSelected)
        {
            //TODO: This will most likely enqueue an item to SignalR service via INotifyService           
            //TODO: This is used to let the other drivers that they were not selected
        }

        private static async Task Notify(PassengerItem passenger, TripItem trip)
        {
            //TODO: This will most likely enqueue an item to SignalR service via INotifyService           
            //TODO: This is used to let the passenger that the trip has been accepted
        }

        private static async Task Notify(TripItem trip)
        {
            //TODO: This will most likely enqueue an item to SignalR service via INotifyService           
            //TODO: This is used to update the trip info i.e. Driver location for the passenger
        }

        private static async Task Externalize(TripItem trip)
        {
            //TODO: This will most likely trigger an Event Grid topic           
            //TODO: Most likely it will be processed by several listeners
        }
    }
}

public class TripInfo
{
    public TripItem Trip { get; set; }
    public PassengerItem Passenger { get; set; }
    public DriverItem Driver { get; set; }
}
