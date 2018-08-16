using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;
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

                // Start a trip...
                await context.CallActivityAsync<TripItem>("A_TO_StartTrip", trip);

                // Monitor every x seconds
                DateTime nextUpdate = context.CurrentUtcDateTime.AddSeconds(ServiceFactory.GetSettingService().GetTripMonitorIntervalInSeconds());
                await context.CreateTimer(nextUpdate, CancellationToken.None);
                trip.MonitorIterations++;
                trip = await context.CallActivityAsync<TripItem>("A_TO_CheckTripCompletion", trip);

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

                    // Recycle the driver back in the available pool
                    await context.CallActivityAsync<TripItem>("A_TO_RecycleTripDriver", trip);

                    // Externalize the trip
                    await context.CallActivityAsync<TripItem>("A_TO_CompleteTrip", trip);
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

        [FunctionName("A_TO_StartTrip")]
        public static async Task StartTrip([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"StartTrip starting....");
            await Externalize(trip, Constants.EVG_SUBJECT_TRIP_STARTING);
        }

        [FunctionName("A_TO_CheckTripCompletion")]
        public static async Task<TripItem> CheckTripCompletion([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"CheckTripCompletion for {trip.Code} starting....");
            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                await persistenceService.CheckTripCompletion(trip);
                await Externalize(trip, Constants.EVG_SUBJECT_TRIP_RUNNING);
            }
            else
            {
                //TODO:
            }

            return trip;
        }

        [FunctionName("A_TO_RecycleTripDriver")]
        public static async Task RecycleTripDriver([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"RecycleTripDriver for {trip.Code} starting....");
            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                await persistenceService.RecycleTripDriver(trip);
            }
            else
            {
                //TODO:
            }
        }

        [FunctionName("A_TO_CompleteTrip")]
        public static async Task CompleteTrip([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"CompleteTrip for {trip.Code} starting....");
            await Externalize(trip, Constants.EVG_SUBJECT_TRIP_COMPLETED);
        }

        [FunctionName("A_TO_Cleanup")]
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
                //TODO:
            }

            await Externalize(trip, Constants.EVG_SUBJECT_TRIP_ABORTED);
        }

        // *** PRIVATE ***//
        private static async Task Externalize(TripItem trip, string subject)
        {
            await Utilities.TriggerEventGridTopic<TripItem>(null, trip, Constants.EVG_EVENT_TYPE_MONITOR_TRIP, subject, ServiceFactory.GetSettingService().GetTripExternalizationsEventGridTopicUrl(), ServiceFactory.GetSettingService().GetTripExternalizationsEventGridTopicApiKey());
        }
    }
}
