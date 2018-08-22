using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using ServerlessMicroservices.Shared.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Passengers
{
    public static class PassengerFunctions
    {
        [FunctionName("GetPassengers")]
        public static async Task<IActionResult> GetPassengers([HttpTrigger(AuthorizationLevel.Function, "get",
                Route = "passengers")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetPassengers triggered....");

            try
            {
                var passengers = ServiceFactory.GetUserService();
                var (users, error) = await passengers.GetUsers();
                if (!string.IsNullOrWhiteSpace(error))
                {
                    var returnError = $"GetPassengers failed: {error}";
                    log.LogError(returnError);
                    return new BadRequestObjectResult(returnError);
                }
                return (ActionResult)new OkObjectResult(users.ToList());
            }
            catch (Exception e)
            {
                var error = $"GetPassengers failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("GetPassenger")]
        public static async Task<IActionResult> GetPassenger([HttpTrigger(AuthorizationLevel.Function, "get", Route = "passengers/{userid}")] HttpRequest req,
            string userid,
            ILogger log)
        {
            log.LogInformation("GetPassenger triggered....");

            try
            {
                var passengers = ServiceFactory.GetUserService();
                var (passenger, error) = await passengers.GetUserById(userid);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    var returnError = $"GetPassenger failed: {error}";
                    log.LogError(returnError);
                    return new BadRequestObjectResult(returnError);
                }
                return (ActionResult)new OkObjectResult(passenger);
            }
            catch (Exception e)
            {
                var error = $"GetPassenger failed: {e.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }
    }
}
