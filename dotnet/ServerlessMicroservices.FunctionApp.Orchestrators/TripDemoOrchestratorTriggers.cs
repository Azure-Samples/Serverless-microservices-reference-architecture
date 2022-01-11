using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessMicroservices.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Orchestrators
{
    public static class TripDemoOrchestratorTriggers
    {
        [FunctionName("T_StartTripDemoViaQueueTrigger")]
        public static async Task StartTripDemoViaQueueTrigger(
            [DurableClient] IDurableClient context,
            [QueueTrigger("%TripDemosQueue%", Connection = "AzureWebJobsStorage")] TripDemoState demoState,
            ILogger log)
        {
            try
            {
                // The demo instance id is the trip code + -D. This is to make sure that a Trip Manager and a monitor can co-exist
                var instanceId = $"{demoState.Code}-D";
                await StartInstance(context, demoState, instanceId, log);
            }
            catch (Exception ex)
            {
                var error = $"StartTripDemoViaQueueTrigger failed: {ex.Message}";
                log.LogError(error);
            }
        }

        [FunctionName("T_StartTripDemo")]
        public static async Task<IActionResult> StartTripDemo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tripdemos")] HttpRequest req,
            [DurableClient] IDurableClient context,
            ILogger log)
        {
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                TripDemoState demoState = JsonConvert.DeserializeObject<TripDemoState>(requestBody);

                // The demo instance id is the trip code + -D. This is to make sure that a Trip Manager and a monitor can co-exist
                var instanceId = $"{demoState.Code}-D";
                await StartInstance(context, demoState, instanceId, log);

                //NOTE: Unfortunately this does not work the same way as before when it was using HttpMessageResponse
                //var reqMessage = req.ToHttpRequestMessage();
                //var res = context.CreateCheckStatusResponse(reqMessage, trip.Code);
                //res.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(10));
                //return (ActionResult)new OkObjectResult(res.Content);
                return (ActionResult)new OkObjectResult("NOTE: No status URLs are returned!");
            }
            catch (Exception ex)
            {
                var error = $"StartTripDemo failed: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("T_GetTripDemo")]
        public static async Task<IActionResult> GetTripDemo([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tripdemos/{code}")] HttpRequest req,
            [DurableClient] IDurableClient context,
            string code,
            ILogger log)
        {
            try
            {
                var status = await context.GetStatusAsync(code);
                if (status == null)
                    throw new Exception($"{code} does not exist!!");

                return (ActionResult)new OkObjectResult(status);
            }
            catch (Exception ex)
            {
                var error = $"GetTripDemo failed: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("T_TerminateTripDemo")]
        public static async Task<IActionResult> TerminateTripDemo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tripdemos/{code}/terminate")] HttpRequest req,
            [DurableClient] IDurableClient context,
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
                var error = $"TerminateTripDemo failed: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        //TODO: Implement Get Trip Demo Instances, Restart Trip Demo Instances and Terminate Trip Demo Instances if Persist to table storage if persist instances is activated

        /** PRIVATE **/
        private static async Task StartInstance(IDurableOrchestrationClient context, TripDemoState state, string instanceId, ILogger log)
        {
            try
            {
                var reportStatus = await context.GetStatusAsync(instanceId);
                string runningStatus = reportStatus == null ? "NULL" : reportStatus.RuntimeStatus.ToString();
                log.LogInformation($"Instance running status: '{runningStatus}'.");

                if (reportStatus == null || reportStatus.RuntimeStatus != OrchestrationRuntimeStatus.Running)
                {
                    await context.StartNewAsync("O_DemoTrip", instanceId, state);
                    log.LogInformation($"Started a new trip demo = '{instanceId}'.");

                    // TODO: Persist to table storage if persist instances is activated
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async Task TeminateInstance(IDurableClient context, string instanceId, ILogger log)
        {
            try
            {
                // TODO: Remove from table storage if persist instances is activated
                log.LogInformation($"Terminating trip demo '{instanceId}'.");
                await context.TerminateAsync(instanceId, "Via an API request");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
