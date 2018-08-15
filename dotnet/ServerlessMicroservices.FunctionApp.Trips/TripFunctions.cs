
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Services;

namespace ServerlessMicroservices.FunctionApp.Trips
{
    public static class TripFunctions
    {
        [FunctionName("GetTrips")]
        public static async Task<IActionResult> GetTrips([HttpTrigger(AuthorizationLevel.Function, "get", Route = "trips")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetTrips triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveTrips());
            }
            catch (Exception e)
            {
                var error = $"GetTrips failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetActiveTrips")]
        public static async Task<IActionResult> GetActiveTrips([HttpTrigger(AuthorizationLevel.Function, "get", Route = "activetrips")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetActiveTrips triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveActiveTrips());
            }
            catch (Exception e)
            {
                var error = $"GetActiveTrips failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetTrip")]
        public static async Task<IActionResult> GetTrip([HttpTrigger(AuthorizationLevel.Function, "get", Route = "trips/{code}")] HttpRequest req,
            string code,
            ILogger log)
        {
            log.LogInformation("GetTrip triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveTrip(code));
            }
            catch (Exception e)
            {
                var error = $"GetTrip failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("CreateTrip")]
        public static async Task<IActionResult> CreateTrip([HttpTrigger(AuthorizationLevel.Function, "post", Route = "trips")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("CreateTrip triggered....");

            try
            {
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
                return new BadRequestObjectResult(error);
            }
        }
    }
}
