using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ServerlessMicroservices.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Orchestrators
{
    [StorageAccount("AzureWebJobsStorage")]
    public static class TripMonitorOrchestrator
    {
        [FunctionName("O_MonitorTrip")]
        public static async Task<object> MonitorTrip(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            TripItem trip = context.GetInput<TripItem>();

            if (!context.IsReplaying)
                log.LogInformation($"MonitorTrip {trip.Code} starting.....");

            try
            {
                // Monitor every x seconds
                DateTime nextUpdate = context.CurrentUtcDateTime.AddSeconds(Constants.TRIP_UPDATE_INTERVAL_IN_SECONDS);
                await context.CreateTimer(nextUpdate, CancellationToken.None);
                trip.MonitorIterations++;
                trip = await context.CallActivityAsync<TripItem>("A_TO_UpdateTrip", trip);

                // We do not allow the trip to hang forever.... 
                if (trip.EndDate == null && trip.MonitorIterations < Constants.TRIP_MONITOR_MAX_ITERATIONS)
                {
                    // Reload the instance with a new state
                    context.ContinueAsNew(trip);
                }

                if (trip.EndDate == null)
                    throw new Exception($"The trip did not complete in {Constants.TRIP_MONITOR_MAX_ITERATIONS} {Constants.TRIP_UPDATE_INTERVAL_IN_SECONDS} second terations!!");
            }
            catch (Exception e)
            {
                if (!context.IsReplaying)
                    log.LogInformation($"Caught an error from an activity: {e.Message}");

                trip.Error = e.Message;
                await context.CallActivityAsync<string>("A_TO_Cleanup", trip);

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

        [FunctionName("A_TO_UpdateTrip")]
        public static async Task<TripItem> UpdateTrip([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"UpdateTrip for {trip.Code} starting....");
            //TODO: Update Cosmos directly or post to another function
            await Notify(trip);
            return trip;
        }

        [FunctionName("A_TO_Cleanup")]
        public static async Task Cleanup([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"Cleanup for {trip.Code} starting....");
            trip.EndDate = DateTime.Now;
            trip.IsAborted = true;
            // TODO: Update Cosmos directly or post to another function
        }

        // *** PRIVATE ***//
        private static async Task Notify(TripItem trip)
        {
            //TODO: This will most likely enqueue an item to SignalR service via INotifyService to update the passenger          
        }
    }
}
