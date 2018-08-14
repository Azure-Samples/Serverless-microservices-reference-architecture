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
    public static class TripMonitorOrchestratorTriggers
    {
        [FunctionName("StartTripMonitor")]
        public static async Task<IActionResult> CreateTripManager([HttpTrigger(AuthorizationLevel.Function, "post", Route = "tripmonitors")] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClient context,
            ILogger log)
        {
            string requestBody = new StreamReader(req.Body).ReadToEnd();
            TripItem trip = JsonConvert.DeserializeObject<TripItem>(requestBody);

            // The monitor instance id is the trip code + -M. This is to make sure that a Trip Manager and a monitor can co-exist
            var code = $"{trip.Code}-M";
            await StartInstance(context, trip, code, log);

            var reqMessage = req.ToHttpRequestMessage();
            var res = context.CreateCheckStatusResponse(reqMessage, code);
            res.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(10));
            // TODO: So no retry header in Core....
            return (ActionResult)new OkObjectResult(res.Content);
        }

        [FunctionName("GetTripMonitor")]
        public static async Task<IActionResult> GetTripMonitor([HttpTrigger(AuthorizationLevel.Function, "get", Route = "tripmonitors/{code}")] HttpRequest req,
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
                var error = $"GetTripMonitor failed: {code} does not exist!!";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("TerminateTripMonitor")]
        public static async Task<IActionResult> TerminateTripMonitor([HttpTrigger(AuthorizationLevel.Function, "post", Route = "tripmonitors/{code}/terminate")] HttpRequest req,
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
                var error = $"TerminateTripMonitor failed: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        //TODO: Implement Get Trip Monitor Instances, Restart Trip Monitor Instances and Terminate Trip Monitor Instances if Persist to table storage if persist instances is activated

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
                    await context.StartNewAsync("O_ActorTripMonitor", code, trip);
                    log.LogInformation($"Started a new trip monitor = '{code}'.");

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
                log.LogInformation($"Terminating trip monitor '{code}'.");
                await context.TerminateAsync(code, "Via an API request");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
