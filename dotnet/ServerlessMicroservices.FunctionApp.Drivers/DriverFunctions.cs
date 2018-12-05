using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;
using ServerlessMicroservices.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ServerlessMicroservices.Models.Request;

namespace ServerlessMicroservices.FunctionApp.Drivers
{
    public static class DriverFunctions
    {
        [FunctionName("GetDrivers")]
        public static async Task<IActionResult> GetDrivers([HttpTrigger(AuthorizationLevel.Anonymous, "get", 
                Route = "drivers")] HttpRequest req, 
            ILogger log)
        {
            log.LogInformation("GetDrivers triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDrivers());
            }
            catch (Exception e)
            {
                var error = $"GetDrivers failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetDriversWithinLocation")]
        public static async Task<IActionResult> GetDriversWithinLocation([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "drivers/{latitude}/{longitude}/{miles}")] HttpRequest req,
            double latitude,
            double longitude,
            double miles, 
            ILogger log)
        {
            log.LogInformation("GetDriversWithinLocation triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDrivers(latitude, longitude, miles));
            }
            catch (Exception e)
            {
                var error = $"GetDriversWithinLocation failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetActiveDrivers")]
        public static async Task<IActionResult> GetActiveDrivers([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "activedrivers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetActiveDrivers triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveActiveDrivers());
            }
            catch (Exception e)
            {
                var error = $"GetActiveDrivers failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetDriver")]
        public static async Task<IActionResult> GetDriver([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "drivers/{code}")] HttpRequest req,
            string code,
            ILogger log)
        {
            log.LogInformation("GetDriver triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDriver(code));
            }
            catch (Exception e)
            {
                var error = $"GetDriver failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("CreateDriver")]
        public static async Task<IActionResult> CreateDriver([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "drivers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("CreateDriver triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                DriverItem driver = JsonConvert.DeserializeObject<DriverItem>(requestBody);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.UpsertDriver(driver));
            }
            catch (Exception e)
            {
                var error = $"CreateDriver failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("UpdateDriver")]
        public static async Task<IActionResult> UpdateDriver([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "drivers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("UpdateDriver triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                DriverItem driver = JsonConvert.DeserializeObject<DriverItem>(requestBody);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.UpsertDriver(driver, true));
            }
            catch (Exception e)
            {
                var error = $"UpdateDriver failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("UpdateDriverLocation")]
        public static async Task<IActionResult> UpdateDriverLocation([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "driverlocations")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("UpdateDriverLocation triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                DriverLocationItem driverLocation = JsonConvert.DeserializeObject<DriverLocationItem>(requestBody);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.UpsertDriverLocation(driverLocation));
            }
            catch (Exception e)
            {
                var error = $"UpdateDriverLocation failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetDriverLocations")]
        public static async Task<IActionResult> GetDriverLocations([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "driverlocations/{code}")] HttpRequest req,
            string code,
            ILogger log)
        {
            log.LogInformation("GetDriverLocations triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDriverLocations(code));
            }
            catch (Exception e)
            {
                var error = $"GetDriverLocations failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("DeleteDriver")]
        public static async Task<IActionResult> DeleteDriver([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "drivers/{code}")] HttpRequest req,
            string code,
            ILogger log)
        {
            log.LogInformation("DeleteDriver triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var persistenceService = ServiceFactory.GetPersistenceService();
                await persistenceService.DeleteDriver(code);
                return (ActionResult)new OkObjectResult("Ok");
            }
            catch (Exception e)
            {
                var error = $"DeleteDriver failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("PredictBestPickupLocation")]
        public static async Task<IActionResult> PredictBestPickupLocation([HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "predict/{date}")] HttpRequest req,
            string date,
            [Blob("data/Samples/RidesharePredictions_%PredictionFileVersion%.json", FileAccess.Read,
                Connection = "PredictionStorageConnection")] string predictionsJson,
            ILogger log)
        {
            log.LogInformation("PredictBestPickupLocation triggered....");
            var returnPrediction = new DriverPickupPredictionResponse();

            try
            {
                await Utilities.ValidateToken(req);

                var predictions = JsonConvert.DeserializeObject<List<DriverPickupPrediction>>(predictionsJson);

                DateTime.TryParse(date, out var predictionDate);

                if (predictionDate == DateTime.MinValue || predictionDate > DateTime.UtcNow.AddDays(15))
                {
                    return new BadRequestObjectResult("You must pass in a valid date that is no greater than 15 days from today.");
                }

                var candidatePredictions = new List<DriverPickupPredictionPayload>();

                // Filter each prediction set by the requested date.
                foreach (var prediction in predictions)
                {
                    var pred = prediction.Predictions.FirstOrDefault(p => DateTime.Parse(p.D).Equals(predictionDate));
                    if (pred != null)
                    {
                        candidatePredictions.Add(new DriverPickupPredictionPayload
                        {
                            LocationFriendlyName = $"{prediction.Location.Latitude}, {prediction.Location.Longitude}",
                            Location = prediction.Location,
                            PredictedPickupRequests = pred.Ppr
                        });
                    }
                }

                // Order by top pickup requests.
                candidatePredictions = candidatePredictions.OrderByDescending(p => p.PredictedPickupRequests).ToList();

                // Return the highest predicted pickup value.
                returnPrediction.TopPrediction = candidatePredictions.Aggregate((i1, i2) => i1.PredictedPickupRequests > i2.PredictedPickupRequests ? i1 : i2);
                returnPrediction.AllPredictions = candidatePredictions;

                return (ActionResult)new OkObjectResult(returnPrediction);
            }
            catch (Exception e)
            {
                var error = $"PredictBestPickupLocation failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }
    }
}
