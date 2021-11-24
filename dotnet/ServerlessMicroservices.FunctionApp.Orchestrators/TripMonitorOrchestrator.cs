using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;
using ServerlessMicroservices.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Orchestrators
{
    [StorageAccount("AzureWebJobsStorage")]
    public static class TripMonitorOrchestrator
    {
        [FunctionName("O_MonitorTrip")]
        public static async Task<object> MonitorTrip(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
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

                if (trip.MonitorIterations == 0)
                    await context.CallActivityAsync<TripItem>("A_TO_StartTrip", trip);

                // Retrieve time settings
                var settings = await context.CallActivityAsync<TripTimeSettings>("A_TO_RetrieveSettings", code);

                // Monitor every x seconds
                DateTime nextUpdate = context.CurrentUtcDateTime.AddSeconds(settings.IntervalInSeconds);
                await context.CreateTimer(nextUpdate, CancellationToken.None);
                trip = await context.CallActivityAsync<TripItem>("A_TO_CheckTripCompletion", trip);

                // We do not allow the trip to hang forever.... 
                if (trip.EndDate == null && trip.MonitorIterations < settings.MaxIterations)
                {
                    // Reload the instance with a new state
                    context.ContinueAsNew(trip.Code);
                }
                else
                {
                    if (trip.EndDate == null)
                        throw new Exception($"The trip did not complete in {settings.MaxIterations * settings.IntervalInSeconds} seconds!!");

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
                //TODO: Go through the Trips APIs
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

        [FunctionName("A_TO_RetrieveSettings")]
        public static async Task<TripTimeSettings> RetrieveSettings([ActivityTrigger] string ignore,
            ILogger log)
        {
            log.LogInformation($"RetrieveSettings starting....");
            TripTimeSettings settings = new TripTimeSettings();
            settings.IntervalInSeconds = ServiceFactory.GetSettingService().GetTripMonitorIntervalInSeconds();
            settings.MaxIterations = ServiceFactory.GetSettingService().GetTripMonitorMaxIterations();
            return settings;
        }

        [FunctionName("A_TO_CheckTripCompletion")]
        public static async Task<TripItem> CheckTripCompletion([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"CheckTripCompletion for {trip.Code} starting....");
            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                trip = await persistenceService.CheckTripCompletion(trip);
                await Externalize(trip, Constants.EVG_SUBJECT_TRIP_RUNNING);
            }
            else
            {
                //TODO: Go through the Trips APIs
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
                //TODO: Go through the Trips APIs
            }
        }

        [FunctionName("A_TO_CompleteTrip")]
        public static async Task CompleteTrip([ActivityTrigger] TripItem trip,
            ILogger log)
        {
            log.LogInformation($"CompleteTrip for {trip.Code} starting....");
            await Externalize(trip, Constants.EVG_SUBJECT_TRIP_COMPLETED);

            // Send an event telemetry
            ServiceFactory.GetLoggerService().Log("Trip completed", new Dictionary<string, string>
                {
                    {"Code", trip.Code },
                    {"Passenger", $"{trip.Passenger.FirstName} {trip.Passenger.LastName}" },
                    {"Destination", $"{trip.Destination.Latitude} - {trip.Destination.Longitude}" },
                    {"Mode", $"{trip.Type}" }
                });
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
            await Utilities.TriggerEventGridTopic<TripItem>(null, trip, Constants.EVG_EVENT_TYPE_MONITOR_TRIP, subject, ServiceFactory.GetSettingService().GetTripExternalizationsEventGridTopicUrl(), ServiceFactory.GetSettingService().GetTripExternalizationsEventGridTopicApiKey());
        }
    }

    public class TripTimeSettings
    {
        public int IntervalInSeconds { get; set; } = 10;
        public int MaxIterations { get; set; } = 20;
    }
}
