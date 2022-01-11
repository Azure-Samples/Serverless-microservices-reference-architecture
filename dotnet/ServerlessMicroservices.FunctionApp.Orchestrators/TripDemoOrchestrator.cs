using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Orchestrators
{
    [StorageAccount("AzureWebJobsStorage")]
    public static class TripDemoOrchestrator
    {
        private static Random _rnd = new Random();

        [FunctionName("O_DemoTrip")]
        public static async Task<object> DemoTrip(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            TripDemoState state = context.GetInput<TripDemoState>();

            if (!context.IsReplaying)
                log.LogInformation($"DemoTrip {state.Code} starting.....");

            try
            {
                var trip = await context.CallActivityAsync<TripItem>("A_TD_RetrieveTrip", state.Code);
                if (trip == null)
                    throw new Exception($"Trip with code {trip.Code} is not found!");

                if (trip.EndDate != null)
                    throw new Exception($"Trip with code {trip.Code} already ended!");

                if (trip.Type == TripTypes.Normal)
                    throw new Exception($"Trip with code {trip.Code} is not a demo!");

                // Retrieve time settings
                var settings = await context.CallActivityAsync<TripTimeSettings>("A_TD_RetrieveSettings", trip.Code);

                // Run every x seconds
                DateTime nextUpdate = context.CurrentUtcDateTime.AddSeconds(settings.IntervalInSeconds);
                await context.CreateTimer(nextUpdate, CancellationToken.None);

                // Retrieve trip route locations if needed
                if (state.RouteLocations.Count == 0)
                    state = await context.CallActivityAsync<TripDemoState>("A_TD_RetrieveRouteItems", state);

                if (state.RouteLocations.Count == 0)
                    throw new Exception($"Trip with code {trip.Code} has no routes!");

                // Assign a driver
                if (trip.Driver == null && trip.AvailableDrivers.Count > 0)
                    await context.CallActivityAsync("A_TD_AssignDriver", trip);

                // Navigate to a new route location
                if (trip.Driver != null)
                {
                    state = await context.CallActivityAsync<TripDemoState>("A_TD_Navigate", new TripDemoInfo()
                    {
                        State = state,
                        Trip = trip
                    });
                }

                // Check for completion
                if (state.CurrentRouteIndex < state.RouteLocations.Count)
                {
                    // Reload the instance with a new state
                    context.ContinueAsNew(state);
                }
            }
            catch (Exception e)
            {
                if (!context.IsReplaying)
                    log.LogInformation($"Caught an error from an activity: {e.Message}");

                await context.CallActivityAsync<string>("A_TO_Cleanup", state);

                return new
                {
                    Error = "Failed to process trip",
                    Message = e.Message
                };
            }

            return new
            {
                State = state
            };
        }

        [FunctionName("A_TD_RetrieveTrip")]
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
                //TODO: Gor through the Trip APIs
            }

            return trip;
        }

        [FunctionName("A_TD_RetrieveSettings")]
        public static async Task<TripTimeSettings> RetrieveSettings([ActivityTrigger] string ignore,
            ILogger log)
        {
            log.LogInformation($"RetrieveSettings starting....");
            TripTimeSettings settings = new TripTimeSettings();
            settings.IntervalInSeconds = ServiceFactory.GetSettingService().GetTripMonitorIntervalInSeconds();
            settings.MaxIterations = ServiceFactory.GetSettingService().GetTripMonitorMaxIterations();
            return settings;
        }

        [FunctionName("A_TD_RetrieveRouteItems")]
        public static async Task<TripDemoState> RetrieveRouteItems([ActivityTrigger] TripDemoState state,
            ILogger log)
        {
            log.LogInformation($"RetrieveRouteItems starting....");
            // Supply the trip source & destination points
            state.RouteLocations = await ServiceFactory.GetRoutesService().RetrieveRouteItems(state.Source, state.Destination);
            return state;
        }

        // Out does does not work in Async methods!!!
        [FunctionName("A_TD_AssignDriver")]
        public static void AssignDriver([ActivityTrigger] TripItem trip,
            [Queue("trip-drivers", Connection = "AzureWebJobsStorage")] out TripDriver queueInfo,
            ILogger log)
        {
            log.LogInformation($"AssignDriver for {trip.Code} starting....");

            // Find a random driver
            var driver = trip.AvailableDrivers[_rnd.Next(trip.AvailableDrivers.Count)];
            if (driver == null)
                throw new Exception("Driver index is out of range!!!");

            // Enqueue the trip code to be monitored 
            queueInfo = new TripDriver()
            {
                TripCode = trip.Code,
                DriverCode = driver.Code
            };
        }

        [FunctionName("A_TD_Navigate")]
        public static async Task<TripDemoState> Navigate([ActivityTrigger] TripDemoInfo info,
            ILogger log)
        {
            log.LogInformation($"Navigate for {info.State.Code} starting....");
            if (info.State.CurrentRouteIndex >= info.State.RouteLocations.Count)
                return info.State;

            var route = info.State.RouteLocations[info.State.CurrentRouteIndex];

            // Persist the driver location
            if (ServiceFactory.GetSettingService().IsPersistDirectly())
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                await persistenceService.UpsertDriverLocation(new DriverLocationItem()
                {
                    DriverCode = info.Trip.Driver.Code,
                    Latitude = route.Latitude,
                    Longitude = route.Longitude
                }, true);
            }
            else
            {
                //TODO: Gor through the Trip APIs
            }

            info.State.CurrentRouteIndex++;
            return info.State;
        }

        [FunctionName("A_TD_Cleanup")]
        public static async Task Cleanup([ActivityTrigger] TripDemoState state,
            ILogger log)
        {
            log.LogInformation($"Cleanup for {state.Code} starting....");
            // TODO: Really nothing to do 
        }
    }

    public class TripDemoInfo
    {
        public TripDemoState State { get; set; }
        public TripItem Trip { get; set; }
    }
}
