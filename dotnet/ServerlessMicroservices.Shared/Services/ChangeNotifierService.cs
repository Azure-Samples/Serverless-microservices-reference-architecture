using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;

namespace ServerlessMicroservices.Shared.Services
{
    public class ChangeNotifierService : IChangeNotifierService
    {
        public const string LOG_TAG = "ChangeNotifierService";

        private ISettingService _settingService;
        private ILoggerService _loggerService;

        public ChangeNotifierService(ISettingService setting, ILoggerService logger)
        {
            _settingService = setting;
            _loggerService = logger;
        }

        public async Task DriverChanged(DriverItem driver)
        {
            //TODO: React to `Driver` changes 
        }

        public async Task TripCreated(TripItem trip, int activeTrips)
        {
            var error = "";

            try
            {
                // Start a trip manager 
                var baseUrl = _settingService.GetStartTripManagerOrchestratorBaseUrl();
                var key = _settingService.GetStartTripManagerOrchestratorApiKey();
                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(key))
                    throw new Exception("Trip manager orchestrator base URL and key must be both provided");

                await Utilities.Post<dynamic, dynamic>(null, trip, $"{baseUrl}/tripmanagers?code={key}", new Dictionary<string, string>());

                // Send an event telemetry
                _loggerService.Log("Trip created", new Dictionary<string, string>
                {
                    {"Code", trip.Code },
                    {"Passenger", $"{trip.Passenger.FirstName} {trip.Passenger.LastName}" },
                    {"Destination", $"{trip.Destination.Latitude} - {trip.Destination.Longitude}" },
                    {"Mode", $"{trip.Type}" }
                });

                // Send a metric telemetry
                _loggerService.Log("Active trips", activeTrips);

                if (trip.Type == TripTypes.Demo)
                {
                    baseUrl = _settingService.GetStartTripDemoOrchestratorBaseUrl();
                    key = _settingService.GetStartTripDemoOrchestratorApiKey();
                    if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(key))
                        throw new Exception("Trip demo orchestrator base URL and key must be both provided");

                    var tripDemoState = new TripDemoState();
                    tripDemoState.Code = trip.Code;
                    tripDemoState.Source = new TripLocation() { Latitude = trip.Source.Latitude, Longitude = trip.Source.Longitude };
                    tripDemoState.Destination = new TripLocation() { Latitude = trip.Destination.Latitude, Longitude = trip.Destination.Longitude };
                    await Utilities.Post<dynamic, dynamic>(null, tripDemoState, $"{baseUrl}/tripdemos?code={key}", new Dictionary<string, string>());
                }
            }
            catch (Exception ex)
            {
                error = $"Error while starting the trip manager: {ex.Message}";
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - TripCreated - Error: {error}");
            }
        }

        public async Task TripDeleted(TripItem trip)
        {
            var error = "";

            try
            {
                try
                {
                    // Terminate a trip manager 
                    var baseUrl = _settingService.GetTerminateTripManagerOrchestratorBaseUrl();
                    var key = _settingService.GetTerminateTripManagerOrchestratorApiKey();
                    if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(key))
                        throw new Exception("Trip manager orchestrator base URL and key must be both provided");

                    await Utilities.Post<dynamic, dynamic>(null, trip, $"{baseUrl}/tripmanagers/{trip.Code}/terminate?code={key}", new Dictionary<string, string>());
                }
                catch (Exception)
                {
                    // Report ...but do not re-throw as it is possible not to have a trip manager running
                    //throw new Exception(error);
                }

                try
                {
                    // Terminate a trip monitor 
                    var baseUrl = _settingService.GetTerminateTripMonitorOrchestratorBaseUrl();
                    var key = _settingService.GetTerminateTripMonitorOrchestratorApiKey();
                    if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(key))
                        throw new Exception("Trip monitor orchestrator base URL and key must be both provided");

                    await Utilities.Post<dynamic, dynamic>(null, trip, $"{baseUrl}/tripmonitors/{trip.Code}-M/terminate?code={key}", new Dictionary<string, string>());

                }
                catch (Exception)
                {
                    // Report ...but do not re-throw as it is possible not to have a trip monitor running
                    //throw new Exception(error);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                // Report ...but do not re-throw as it is possible not to have trip manager or monitor running
                //throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - TripDeleted - Error: {error}");
            }
        }

        public async Task PassengerChanged(PassengerItem trip)
        {
            //TODO: React to `Passenger` changes 
        }
    }
}
