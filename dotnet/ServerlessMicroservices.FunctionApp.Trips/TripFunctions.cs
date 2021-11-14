using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;
using ServerlessMicroservices.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Trips
{
    public static class TripFunctions
    {
        [FunctionName("GetTrips")]
        public static async Task<IActionResult> GetTrips([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "trips")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetTrips triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveTrips());
            }
            catch (Exception e)
            {
                var error = $"GetTrips failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetActiveTrips")]
        public static async Task<IActionResult> GetActiveTrips([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "activetrips")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetActiveTrips triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveActiveTrips());
            }
            catch (Exception e)
            {
                var error = $"GetActiveTrips failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetTrip")]
        public static async Task<IActionResult> GetTrip([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "trips/{code}")] HttpRequest req,
            string code,
            ILogger log)
        {
            log.LogInformation("GetTrip triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveTrip(code));
            }
            catch (Exception e)
            {
                var error = $"GetTrip failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("CreateTrip")]
        public static async Task<IActionResult> CreateTrip([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trips")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("CreateTrip triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                TripItem trip = JsonConvert.DeserializeObject<TripItem>(requestBody);

                // validate
                if (trip.Passenger == null || string.IsNullOrEmpty(trip.Passenger.Code))
                    throw new Exception("A passenger with a valid code must be attached to the trip!!");

                trip.EndDate = null;
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.UpsertTrip(trip));
            }
            catch (Exception e)
            {
                var error = $"CreateTrip failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("AssignTripDriver")]
        public static async Task<IActionResult> AssignTripDriver([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "trips/{code}/drivers/{drivercode}")] HttpRequest req,
            string code,
            string drivercode,
            ILogger log)
        {
            log.LogInformation("AssignTripDriver triggered....");

            try
            {
                await Utilities.ValidateToken(req);

                var settingService = ServiceFactory.GetSettingService();
                if (!settingService.IsEnqueueToOrchestrators())
                {
                    // Send over to the trip manager 
                    var baseUrl = settingService.GetStartTripManagerOrchestratorBaseUrl();
                    var key = settingService.GetStartTripManagerOrchestratorApiKey();
                    if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(key))
                        throw new Exception("Trip manager orchestrator base URL and key must be both provided");

                    await Utilities.Post<dynamic, dynamic>(null, null, $"{baseUrl}/tripmanagers/{code}/acknowledge/drivers/{drivercode}?code={key}", new Dictionary<string, string>());
                }
                else
                {
                    await ServiceFactory.GetStorageService().Enqueue(code, drivercode);
                }

                return (ActionResult)new OkObjectResult("Ok");
            }
            catch (Exception e)
            {
                var error = $"AssignTripDriver failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        /*** SignalR Info or Negotiate Function ****/
        [FunctionName("GetSignalRInfo")]
        public static async Task<IActionResult> GetSignalRInfo([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "signalrinfo")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "trips", UserId = "{headers.x-ms-signalr-userid}")] SignalRConnectionInfo info,
            ILogger log)
        {
            log.LogInformation("GetSignalRInfo triggered....");

            try
            {
                if (info == null)
                    throw new Exception("SignalR Info is null!");

                await Utilities.ValidateToken(req);

                return (ActionResult)new OkObjectResult(info);
            }
            catch (Exception e)
            {
                var error = $"GetSignalRInfo failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        /*** Event Grid Listeners ****/
        [FunctionName("EVGH_TripExternalizations2SignalR")]
        public static async Task ProcessTripExternalizations2SignalR([EventGridTrigger] EventGridEvent eventGridEvent,
            [SignalR(HubName = "trips")] IAsyncCollector<SignalRMessage> signalRMessages, 
            ILogger log)
        {
            log.LogInformation($"ProcessTripExternalizations2SignalR triggered....EventGridEvent" +
                            $"\n\tId:{eventGridEvent.Id}" +
                            $"\n\tTopic:{eventGridEvent.Topic}" +
                            $"\n\tSubject:{eventGridEvent.Subject}" +
                            $"\n\tType:{eventGridEvent.EventType}" +
                            $"\n\tData:{eventGridEvent.Data}");

            try
            {
                TripItem trip = JsonConvert.DeserializeObject<TripItem>(eventGridEvent.Data.ToString());
                if (trip == null)
                    throw new Exception("Trip is null!");

                log.LogInformation($"ProcessTripExternalizations2SignalR trip code {trip.Code}");

                // Convert the `event subject` to a method to be called on clients
                var clientMethod = "tripUpdated";
                if (eventGridEvent.Subject == Constants.EVG_SUBJECT_TRIP_DRIVERS_NOTIFIED)
                    clientMethod = "tripDriversNotified";
                else if (eventGridEvent.Subject == Constants.EVG_SUBJECT_TRIP_DRIVER_PICKED)
                    clientMethod = "tripDriverPicked";
                else if (eventGridEvent.Subject == Constants.EVG_SUBJECT_TRIP_STARTING)
                    clientMethod = "tripStarting";
                else if (eventGridEvent.Subject == Constants.EVG_SUBJECT_TRIP_RUNNING)
                    clientMethod = "tripRunning";
                else if (eventGridEvent.Subject == Constants.EVG_SUBJECT_TRIP_COMPLETED)
                    clientMethod = "tripCompleted";
                else if (eventGridEvent.Subject == Constants.EVG_SUBJECT_TRIP_ABORTED)
                    clientMethod = "tripAborted";

                log.LogInformation($"ProcessTripExternalizations2SignalR firing SignalR `{clientMethod}` client method!");
                await signalRMessages.AddAsync(new SignalRMessage
                {
                    UserId = trip.Passenger.Code,
                    Target = clientMethod,
                    Arguments = new object[] { trip }
                });
            }
            catch (Exception e)
            {
                var error = $"ProcessTripExternalizations2SignalR failed: {e.Message}";
                log.LogError(error);
                throw e;
            }
        }

        [FunctionName("EVGH_TripExternalizations2PowerBI")]
        public static async Task ProcessTripExternalizations2PowerBI([EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation($"ProcessTripExternalizations2PowerBI triggered....EventGridEvent" +
                            $"\n\tId:{eventGridEvent.Id}" +
                            $"\n\tTopic:{eventGridEvent.Topic}" +
                            $"\n\tSubject:{eventGridEvent.Subject}" +
                            $"\n\tType:{eventGridEvent.EventType}" +
                            $"\n\tData:{eventGridEvent.Data}");

            try
            {
                TripItem trip = JsonConvert.DeserializeObject<TripItem>(eventGridEvent.Data.ToString());
                if (trip == null)
                    throw new Exception("Trip is null!");

                log.LogInformation($"ProcessTripExternalizations2PowerBI trip code {trip.Code}");

                if (eventGridEvent.Subject == Constants.EVG_SUBJECT_TRIP_ABORTED ||
                    eventGridEvent.Subject == Constants.EVG_SUBJECT_TRIP_COMPLETED)
                {
                    var archiveService = ServiceFactory.GetArchiveService();
                    await archiveService.UpsertTrip(trip);

                    var powerBIService = ServiceFactory.GetPowerBIService();
                    await powerBIService.UpsertTrip(trip);
                }
            }
            catch (Exception e)
            {
                var error = $"ProcessTripExternalizations2PowerBI failed: {e.Message}";
                log.LogError(error);
                throw e;
            }
        }

        /*** TEST SUPPORT ***/
        [FunctionName("StoreTripTestParameters")]
        public static async Task<IActionResult> StoreTripTestParameters([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "triptestparameters")] HttpRequest req,
            [Blob("trips/testparams.json", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream outBlob,
            ILogger log)
        {
            log.LogInformation("StoreTripTestParameters triggered....");

            try
            {
                //NOTE: No need for security check as this is used in testing only
                var requestBody = new StreamReader(req.Body).ReadToEnd();
                byte[] byteArray = Encoding.UTF8.GetBytes(requestBody);
                await outBlob.WriteAsync(byteArray, 0, byteArray.Length);
                return (ActionResult)new OkObjectResult("Ok");
            }
            catch (Exception e)
            {
                var error = $"StoreTripTestParameters failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("RetrieveTripTestParameters")]
        public static IActionResult RetrieveTripTestParameters([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "triptestparameters")] HttpRequest req,
            [Blob("trips/testparams.json", FileAccess.Read, Connection = "AzureWebJobsStorage")] Stream inBlob,
            ILogger log)
        {
            log.LogInformation("RetrieveTripTestParameters triggered....");

            try
            {
                //NOTE: No need for security check as this is used in testing only
                StreamReader reader = new StreamReader(inBlob);
                return (ActionResult)new OkObjectResult(JsonConvert.DeserializeObject<dynamic>(reader.ReadToEnd()));
            }
            catch (Exception e)
            {
                var error = $"RetrieveTripTestParameters failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }
    }
}
