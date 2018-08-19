using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Drivers
{
    public static class DriverFunctions
    {
        [FunctionName("GetDrivers")]
        public static async Task<IActionResult> GetDrivers([HttpTrigger(AuthorizationLevel.Function, "get", 
                Route = "drivers")] HttpRequest req, 
            ILogger log)
        {
            log.LogInformation("GetDrivers triggered....");
            var validationService = ServiceFactory.GetTokenValidationService();
            var user = await validationService.AuthenticateRequest(req);

            if (user == null)
            {
                return new StatusCodeResult(401);
            }

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDrivers());
            }
            catch (Exception e)
            {
                var error = $"GetDrivers failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetDriversWithinLocation")]
        public static async Task<IActionResult> GetDriversWithinLocation([HttpTrigger(AuthorizationLevel.Function, "get", Route = "drivers/{latitude}/{longitude}/{miles}")] HttpRequest req,
            double latitude,
            double longitude,
            double miles, 
            ILogger log)
        {
            log.LogInformation("GetDriversWithinLocation triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDrivers(latitude, longitude, miles));
            }
            catch (Exception e)
            {
                var error = $"GetDriversWithinLocation failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetActiveDrivers")]
        public static async Task<IActionResult> GetActiveDrivers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "activedrivers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetActiveDrivers triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveActiveDrivers());
            }
            catch (Exception e)
            {
                var error = $"GetActiveDrivers failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetDriver")]
        public static async Task<IActionResult> GetDriver([HttpTrigger(AuthorizationLevel.Function, "get", Route = "drivers/{code}")] HttpRequest req,
            string code,
            ILogger log)
        {
            log.LogInformation("GetDriver triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDriver(code));
            }
            catch (Exception e)
            {
                var error = $"GetDriver failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("CreateDriver")]
        public static async Task<IActionResult> CreateDriver([HttpTrigger(AuthorizationLevel.Function, "post", Route = "drivers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("CreateDriver triggered....");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                DriverItem driver = JsonConvert.DeserializeObject<DriverItem>(requestBody);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.UpsertDriver(driver));
            }
            catch (Exception e)
            {
                var error = $"CreateDriver failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("UpdateDriver")]
        public static async Task<IActionResult> UpdateDriver([HttpTrigger(AuthorizationLevel.Function, "put", Route = "drivers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("UpdateDriver triggered....");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                DriverItem driver = JsonConvert.DeserializeObject<DriverItem>(requestBody);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.UpsertDriver(driver, true));
            }
            catch (Exception e)
            {
                var error = $"UpdateDriver failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("UpdateDriverLocation")]
        public static async Task<IActionResult> UpdateDriverLocation([HttpTrigger(AuthorizationLevel.Function, "put", Route = "driverlocations")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("UpdateDriverLocation triggered....");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                DriverLocationItem driverLocation = JsonConvert.DeserializeObject<DriverLocationItem>(requestBody);
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.UpsertDriverLocation(driverLocation));
            }
            catch (Exception e)
            {
                var error = $"UpdateDriverLocation failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetDriverLocations")]
        public static async Task<IActionResult> GetDriverLocations([HttpTrigger(AuthorizationLevel.Function, "get", Route = "driverlocations/{code}")] HttpRequest req,
            string code,
            ILogger log)
        {
            log.LogInformation("GetDriverLocations triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDriverLocations(code));
            }
            catch (Exception e)
            {
                var error = $"GetDriverLocations failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("DeleteDriver")]
        public static async Task<IActionResult> DeleteDriver([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "drivers/{code}")] HttpRequest req,
            string code,
            ILogger log)
        {
            log.LogInformation("DeleteDriver triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService();
                await persistenceService.DeleteDriver(code);
                return (ActionResult)new OkObjectResult("Ok");
            }
            catch (Exception e)
            {
                var error = $"DeleteDriver failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }
    }
}
