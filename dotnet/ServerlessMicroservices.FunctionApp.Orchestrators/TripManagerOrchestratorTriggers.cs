using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessMicroservices.Models;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Orchestrators
{
    public static class TripManagerOrchestratorTriggers
    {
        [FunctionName("StartTripManager")]
        public static async Task<IActionResult> CreateTripManager([HttpTrigger(AuthorizationLevel.Function, "post", Route = "tripmanagers")] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient context,
            ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            TripItem trip = JsonConvert.DeserializeObject<TripItem>(requestBody);

            // The manager instance id is the trip code
            await StartInstance(context, trip, trip.Code, log);

            var reqMessage = req.ToHttpRequestMessage();
            var res = context.CreateCheckStatusResponse(reqMessage, trip.Code);
            res.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(10));
            // TODO: So no retry header in Core....
            return (ActionResult) new OkObjectResult(res.Content);
        }

        [FunctionName("GetTripManager")]
        public static async Task<IActionResult> GetTripManager([HttpTrigger(AuthorizationLevel.Function, "get", Route = "tripmanagers/{code}")] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient context,
            string code,
            ILogger log)
        {
            var status = await context.GetStatusAsync(code);
            if (status != null)
            {
                return (ActionResult)new OkObjectResult(status);
            }
            else
            {
                var error = $"GetTripManager failed: {code} does not exist!!";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("TerminateTripManager")]
        public static async Task<IActionResult> TerminateTripManager([HttpTrigger(AuthorizationLevel.Function, "post", Route = "tripmanagers/{code}/terminate")] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient context,
            string code,
            ILogger log)
        {
            try
            {
                await TeminateInstance(context, code, log);
                return (ActionResult)new OkObjectResult("Ok");
            }
            catch (Exception ex)
            {
                var error = $"TerminateTripManager failed: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("AcknowledgeTrip")]
        public static async Task<IActionResult> AcknowledgeTrip([HttpTrigger(AuthorizationLevel.Function, "post", Route = "tripmanagers/{code}/acknowledge/drivers/{drivercode}")] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient context,
            string code,
            string drivercode,
            ILogger log)
        {
            try
            {
                await context.RaiseEventAsync(code, Constants.TRIP_DRIVER_ACKNOWLEDGE, drivercode);
                return (ActionResult)new OkObjectResult("Ok");
            }
            catch (Exception ex)
            {
                var error = $"AcknowledgeTrip failed: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        //TODO: Implement Get Trip Manager Instances, Restart Trip Manager Instances and Terminate Trip Manager Instances if Persist to table storage if persist instances is activated

        /** PRIVATE **/
        private static async Task StartInstance(DurableOrchestrationClient context, TripItem trip, string code, ILogger log)
        {
            try
            {
                var reportStatus = await context.GetStatusAsync(code);
                string runningStatus = reportStatus == null ? "NULL" : reportStatus.RuntimeStatus.ToString();
                log.LogInformation($"Instance running status: '{runningStatus}'.");

                if (reportStatus == null || reportStatus.RuntimeStatus != OrchestrationRuntimeStatus.Running)
                {
                    await context.StartNewAsync("O_ActorTripManager", code, trip);
                    log.LogInformation($"Started a new trip manager = '{code}'.");

                    // TODO: Persist to table storage if persist instances is activated
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async Task TeminateInstance(DurableOrchestrationClient context, string code, ILogger log)
        {
            try
            {
                // TODO: Remove from table storage if persist instances is activated
                log.LogInformation($"Terminating trip manager '{code}'.");
                await context.TerminateAsync(code, "Via an API request");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
