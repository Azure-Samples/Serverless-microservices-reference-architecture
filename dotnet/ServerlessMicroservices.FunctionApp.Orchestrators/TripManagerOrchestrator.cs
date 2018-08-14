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

                // Update the trip
                trip.AvailableDrivers = availableDrivers;
                trip = await context.CallActivityAsync<TripItem>("A_TM_UpdateTrip", trip);

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
                    throw new Exception($"Did not receive an ack from any driver in {Constants.WAIT_FOR_DRIVERS_PERIOD_IN_SECONDS} seconds!!");

                var acceptedDriver = availableDrivers.SingleOrDefault(d => d.Code == driverAcceptCode);
                if (acceptedDriver == null)
                    throw new Exception($"Data integrity - received an ack from an invalid driver {driverAcceptCode}!!");

                // Update the trip
                trip.Driver = acceptedDriver;
                trip.AcceptDate = context.CurrentUtcDateTime;
                trip = await context.CallActivityAsync<TripItem>("A_TM_UpdateTrip", trip);

                // Notify the selected driver
                await context.CallActivityAsync("A_TM_NotifySelectedDriver", trip);

                // Remove selected and notify other drivers
                await context.CallActivityAsync("A_TM_NotifyOtherDrivers", trip);

                // Notify the passenger
                await context.CallActivityAsync("A_TM_NotifyPassenger", trip);

                // Externalize the trip
                await context.CallActivityAsync("A_TM_Externalize", trip);

                // Trigger to create a trip monitor orchestrator whose job is to monitor the driver location every x seconds and update the trip
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
        public static async Task<List<DriverItem>> FindNNotifyDrivers([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"FindNNotifyDrivers for {trip.Code} starting....");
            // TODO: Query Cosmos directly or call another function to get the result
            List<DriverItem> availableDrivers = new List<DriverItem>();
            foreach (var driver in availableDrivers)
            {
                await Notify(driver, trip);
            }

            return availableDrivers;
        }

        [FunctionName("A_TM_UpdateTrip")]
        public static async Task<TripItem> UpdateTrip([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"UpdateTrip starting....");
            // TODO: Update Cosmos directly or post to another function
            await Notify(trip);
            return trip;
        }

        [FunctionName("A_TM_NotifySelectedDriver")]
        public static async Task NotifySelectedDriver([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"NotifySelectedDriver starting....");
            await Notify(trip.Driver, trip, true);
        }

        [FunctionName("A_TM_NotifyOtherDrivers")]
        public static async Task NotifyOtherDrivers([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"NotifyOtherDrivers starting....");
            trip.AvailableDrivers.Remove(trip.Driver);
            foreach (var driver in trip.AvailableDrivers)
            {
                await Notify(driver, trip, false);
            }
        }

        [FunctionName("A_TM_NotifyPassenger")]
        public static async Task NotifyPassenger([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"NotifyPassenger starting....");
            await Notify(trip.Passenger, trip);
        }

        [FunctionName("A_TM_Externalize")]
        public static async Task NotifyExternal([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"Externalize starting....");
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
            trip.EndDate = DateTime.Now;
            trip.IsAborted = true;
            // TODO: Update Cosmos directly or post to another function
        }

        // *** PRIVATE ***//
        private static async Task Notify(DriverItem driver, TripItem trip)
        {
            //OUT-OF-SCOPE: Used to notify a driver that a trip is up for grabs
            //OUT-OF-SCOPE: The notification can be a SignalR push, push notification, SMS or Email
            //OUT-OF-SCOPE: An email with an Ok button to accept the ride is probably good for demo
        }

        private static async Task Notify(DriverItem driver, TripItem trip, bool isSelected)
        {
            //OUT-OF-SCOPE: Used to notify an available driver that either he/she is selected or not selected for a trip           
            //OUT-OF-SCOPE: If true, it is used to let the selected driver that he/she has been selected
            //OUT-OF-SCOPE: If false, it is used to let the other drivers that they were not selected
        }

        private static async Task Notify(PassengerItem passenger, TripItem trip)
        {
            //OUT-OF-SCOPE: This is used to let the passenger that the trip has been accepted
            //OUT-OF-SCOPE: The notification can be a SignalR push, push notification, SMS or Email
        }

        private static async Task Notify(TripItem trip)
        {
            //OUT-OF-SCOPE: This is used to update the trip info i.e. Driver location for all observers
        }

        private static async Task Externalize(TripItem trip)
        {
            //TODO: Trigger an Event Grid topic           
        }
    }
}
