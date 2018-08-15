using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Services;
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
            string code = context.GetInput<string>();
            TripItem trip = null;

            if (!context.IsReplaying)
                log.LogInformation($"MonitorTrip {code} starting.....");

            try
            {
                trip = await context.CallActivityAsync<TripItem>("A_TO_RetrieveTrip", code);
                if (trip == null)
                    throw new Exception($"Trip with code {code} is not found!");

                if (trip.Driver == null)
                    throw new Exception($"Trip with code {code} has no driver!");

                // Monitor every x seconds
                DateTime nextUpdate = context.CurrentUtcDateTime.AddSeconds(ServiceFactory.GetSettingService().GetTripMonitorIntervalInSeconds());
                await context.CreateTimer(nextUpdate, CancellationToken.None);
                trip.MonitorIterations++;
                trip = await context.CallActivityAsync<TripItem>("A_TO_UpdateTrip", trip);

                // We do not allow the trip to hang forever.... 
                if (trip.EndDate == null && trip.MonitorIterations < ServiceFactory.GetSettingService().GetTripMonitorMaxIterations())
                {
                    // Reload the instance with a new state
                    context.ContinueAsNew(trip.Code);
                }
                else
                {
                    if (trip.EndDate == null)
                        throw new Exception($"The trip did not complete in {ServiceFactory.GetSettingService().GetTripMonitorMaxIterations() * ServiceFactory.GetSettingService().GetTripMonitorIntervalInSeconds()} seconds!!");

                    // Let the driver be back in the available pool
                    await context.CallActivityAsync<TripItem>("A_TO_UpdateDriver", trip);

                    // Externalize the trip
                    await context.CallActivityAsync<TripItem>("A_TO_Externalize", trip);
                }
            }
            catch (Exception e)
            {
                if (!context.IsReplaying)
                    log.LogInformation($"Caught an error from an activity: {e.Message}");

                if (trip != null)
                {
                    trip.Error = e.Message;
                    await context.CallActivityAsync<string>("A_TO_Cleanup", trip);
                }

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

        [FunctionName("A_TO_RetrieveTrip")]
        public static async Task<TripItem> RetrieveTrip([ActivityTrigger] string code,
            ILogger log)
        {
            log.LogInformation($"RetrieveTrip for {code} starting....");
            TripItem trip = null;
            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                trip = await persistenceService.RetrieveTrip(code);
            }
            else
            {
                //TODO:
            }

            return trip;
        }

        [FunctionName("A_TO_UpdateTrip")]
        public static async Task<TripItem> UpdateTrip([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"UpdateTrip for {trip.Code} starting....");
            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                // NOTE: We need a way to determine when the trip is over
                // It is ridiculous, but for now, I am checking to see if the driver location equals the trip destination location :-)
                var driver = await persistenceService.RetrieveDriver(trip.Driver.Code);
                if (driver != null && driver.Latitude == trip.Destination.Latitude && driver.Longitude == trip.Destination.Longitude)
                    trip.EndDate = DateTime.Now;

                trip = await persistenceService.UpsertTrip(trip, true);
                await Notify(trip);
            }
            else
            {
                //TODO:
            }

            return trip;
        }

        [FunctionName("A_TO_UpdateDriver")]
        public static async Task UpdateDriver([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"UpdateDriver for {trip.Code} starting....");
            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                var driver = trip.Driver;
                if (driver != null)
                {
                    driver = await persistenceService.RetrieveDriver(trip.Driver.Code);
                    driver.IsAcceptingRides = true;
                    await persistenceService.UpsertDriver(driver, true);
                }
            }
            else
            {
                //TODO:
            }
        }

        [FunctionName("A_TO_Externalize")]
        public static async Task Externalize([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"Externalize for {trip.Code} starting....");
            await Externalize(trip);
        }

        [FunctionName("A_TO_Cleanup")]
        public static async Task Cleanup([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"Cleanup for {trip.Code} starting....");
            trip.EndDate = DateTime.Now;
            trip.IsAborted = true;

            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                await persistenceService.UpsertTrip(trip, true);
                var driver = trip.Driver;
                if (driver != null)
                {
                    driver.IsAcceptingRides = true;
                    await persistenceService.UpsertDriver(driver, true);
                }
            }
            else
            {
                //TODO:
            }

            await Externalize(trip);
        }

        // *** PRIVATE ***//
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
