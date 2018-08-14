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
        public static async Task<IActionResult> GetDrivers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "drivers")] HttpRequest req, 
            ILogger log)
        {
            log.LogInformation("GetDrivers triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDrivers());
            }
            catch (Exception e)
            {
                var error = $"GetDrivers failed: {e.Message}";
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
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
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
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
                return (ActionResult)new OkObjectResult(await persistenceService.UpsertDriver(driver));
            }
            catch (Exception e)
            {
                var error = $"CreateDriver failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("UpdateDriverLocation")]
        public static async Task<IActionResult> UpdateDriverLocation([HttpTrigger(AuthorizationLevel.Function, "put", Route = "drivers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("UpdateDriverLocation triggered....");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                DriverLocationItem driverLocation = JsonConvert.DeserializeObject<DriverLocationItem>(requestBody);
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
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
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
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
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
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
