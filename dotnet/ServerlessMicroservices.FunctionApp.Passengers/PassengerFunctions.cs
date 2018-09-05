using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using ServerlessMicroservices.Shared.Helpers;
using ServerlessMicroservices.Shared.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Passengers
{
    public static class PassengerFunctions
    {
        [FunctionName("GetPassengers")]
        public static async Task<IActionResult> GetPassengers([HttpTrigger(AuthorizationLevel.Anonymous, "get",
                Route = "passengers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetPassengers triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var passengers = ServiceFactory.GetUserService();
                var (users, error) = await passengers.GetUsers();
                if (!string.IsNullOrWhiteSpace(error))
                    throw new Exception(error);

                return (ActionResult)new OkObjectResult(users.ToList());
            }
            catch (Exception e)
            {
                var error = $"GetPassengers failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetPassenger")]
        public static async Task<IActionResult> GetPassenger([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "passengers/{userid}")] HttpRequest req,
            string userid,
            ILogger log)
        {
            log.LogInformation("GetPassenger triggered....");

            try
            {
                await Utilities.ValidateToken(req);
                var passengers = ServiceFactory.GetUserService();
                var (passenger, error) = await passengers.GetUserById(userid);
                if (!string.IsNullOrWhiteSpace(error))
                    throw new Exception(error);

                return (ActionResult)new OkObjectResult(passenger);
            }
            catch (Exception e)
            {
                var error = $"GetPassenger failed: {e.Message}";
                log.LogError(error);
                if (error.Contains(Constants.SECURITY_VALITION_ERROR))
                    return new StatusCodeResult(401);
                else
                    return new BadRequestObjectResult(error);
            }
        }
    }
}
