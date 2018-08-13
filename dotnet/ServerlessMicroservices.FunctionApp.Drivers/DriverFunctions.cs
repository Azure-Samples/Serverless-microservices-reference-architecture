using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using ServerlessMicroservices.Shared.Services;
using System.Threading.Tasks;
using ServerlessMicroservices.Models;

namespace ServerlessMicroservices.FunctionApp.Drivers
{
    public static class DriverFunctions
    {
        [FunctionName("G_Drivers")]
        public static async Task<IActionResult> GetDrivers([HttpTrigger(AuthorizationLevel.Function, "get", Route = "drivers")] HttpRequest req, 
            TraceWriter log)
        {
            log.Info("G_Drivers triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDrivers());
            }
            catch (Exception e)
            {
                log.Error($"G_Drivers failed: {e.Message}");
                return new BadRequestObjectResult($"G_Drivers failed: {e.Message}");
            }
        }

        [FunctionName("G_Driver")]
        public static async Task<IActionResult> GetDriver([HttpTrigger(AuthorizationLevel.Function, "get", Route = "drivers/{code}")] HttpRequest req,
            string code,
            TraceWriter log)
        {
            log.Info("G_Driver triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDriver(code));
            }
            catch (Exception e)
            {
                log.Error($"G_Driver failed: {e.Message}");
                return new BadRequestObjectResult($"G_Driver failed: {e.Message}");
            }
        }

        [FunctionName("P_Driver")]
        public static async Task<IActionResult> PostDriver([HttpTrigger(AuthorizationLevel.Function, "post", Route = "drivers")] HttpRequest req,
            TraceWriter log)
        {
            log.Info("P_Driver triggered....");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                DriverItem driver = JsonConvert.DeserializeObject<DriverItem>(requestBody);
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
                return (ActionResult)new OkObjectResult(await persistenceService.UpsertDriver(driver));
            }
            catch (Exception e)
            {
                log.Error($"P_Driver failed: {e.Message}");
                return new BadRequestObjectResult($"P_Driver failed: {e.Message}");
            }
        }

        [FunctionName("U_Driver")]
        public static async Task<IActionResult> UpdateDriverLocation([HttpTrigger(AuthorizationLevel.Function, "put", Route = "drivers")] HttpRequest req,
            TraceWriter log)
        {
            log.Info("U_Driver triggered....");

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                DriverLocationItem driverLocation = JsonConvert.DeserializeObject<DriverLocationItem>(requestBody);
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
                return (ActionResult)new OkObjectResult(await persistenceService.UpsertDriverLocation(driverLocation));
            }
            catch (Exception e)
            {
                log.Error($"U_Driver failed: {e.Message}");
                return new BadRequestObjectResult($"U_Driver failed: {e.Message}");
            }
        }

        [FunctionName("G_DriverLocations")]
        public static async Task<IActionResult> GetDriverLocations([HttpTrigger(AuthorizationLevel.Function, "get", Route = "driverlocations/{code}")] HttpRequest req,
            string code,
            TraceWriter log)
        {
            log.Info("G_DriverLocations triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
                return (ActionResult)new OkObjectResult(await persistenceService.RetrieveDriverLocations(code));
            }
            catch (Exception e)
            {
                log.Error($"G_DriverLocations failed: {e.Message}");
                return new BadRequestObjectResult($"G_DriverLocations failed: {e.Message}");
            }
        }

        [FunctionName("D_Driver")]
        public static async Task<IActionResult> DeleteDriver([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "drivers/{code}")] HttpRequest req,
            string code,
            TraceWriter log)
        {
            log.Info("D_Driver triggered....");

            try
            {
                var persistenceService = ServiceFactory.GetPersistenceService(ServiceFactory.GetSettingService(), ServiceFactory.GetLoggerService(), ServiceFactory.GetAnalyticService());
                await persistenceService.DeleteDriver(code);
                return (ActionResult)new OkObjectResult("Ok");
            }
            catch (Exception e)
            {
                log.Error($"D_Driver failed: {e.Message}");
                return new BadRequestObjectResult($"D_Driver failed: {e.Message}");
            }
        }
    }
}
