using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ServerlessMicroservices.FunctionApp.Orchestrators
{
    public static class TripManagerOrchestratorTriggers
    {
        [FunctionName("T_StartTripManagerViaQueueTrigger")]
        public static async Task StartTripManagerViaQueueTrigger(
            [DurableClient] IDurableClient context,
            [QueueTrigger("%TripManagersQueue%", Connection = "AzureWebJobsStorage")] TripItem trip,
            ILogger log)
        {
            try
            {
                // The manager instance id is the trip code
                var instanceId = trip.Code;
                await StartInstance(context, trip, instanceId, log);
            }
            catch (Exception ex)
            {
                var error = $"StartTripManagerViaQueueTrigger failed: {ex.Message}";
                log.LogError(error);
            }
        }

        [FunctionName("T_StartTripManager")]
        public static async Task<IActionResult> StartTripManager([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tripmanagers")] HttpRequest req,
            [DurableClient] IDurableClient context,
            ILogger log)
        {
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                TripItem trip = JsonConvert.DeserializeObject<TripItem>(requestBody);

                // The manager instance id is the trip code
                var instanceId = trip.Code;
                await StartInstance(context, trip, instanceId, log);

                //NOTE: Unfortunately this does not work the same way as before when it was using HttpMessageResponse
                //var reqMessage = req.ToHttpRequestMessage();
                //var res = context.CreateCheckStatusResponse(reqMessage, trip.Code);
                //res.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(10));
                //return (ActionResult)new OkObjectResult(res.Content);
                return (ActionResult)new OkObjectResult("NOTE: No status URLs are returned!");
            }
            catch (Exception ex)
            {
                var error = $"StartTripManager failed: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("T_GetTripManager")]
        public static async Task<IActionResult> GetTripManager([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tripmanagers/{code}")] HttpRequest req,
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
                var error = $"GetTripManager failed: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("T_TerminateTripManager")]
        public static async Task<IActionResult> TerminateTripManager([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tripmanagers/{code}/terminate")] HttpRequest req,
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
                var error = $"TerminateTripManager failed: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("T_AcknowledgeTrip")]
        public static async Task<IActionResult> AcknowledgeTrip([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tripmanagers/{code}/acknowledge/drivers/{drivercode}")] HttpRequest req,
            [DurableClient] IDurableClient context,
            string code,
            string drivercode,
            ILogger log)
        {
            try
            {
                await context.RaiseEventAsync(code, Constants.TRIP_DRIVER_ACCEPT_EVENT, drivercode);
                return (ActionResult)new OkObjectResult("Ok");
            }
            catch (Exception ex)
            {
                var error = $"AcknowledgeTrip failed: {ex.Message}";
                log.LogError(error);
                return new BadRequestObjectResult(error);
            }
        }

        [FunctionName("T_AcknowledgeTripViaQueueTrigger")]
        public static async Task AcknowledgeTripViaQueueTrigger(
            [DurableClient] IDurableClient context,
            [QueueTrigger("%TripDriversQueue%", Connection = "AzureWebJobsStorage")] TripDriver info,
            ILogger log)
        {
            try
            {
                await context.RaiseEventAsync(info.TripCode, Constants.TRIP_DRIVER_ACCEPT_EVENT, info.DriverCode);
            }
            catch (Exception ex)
            {
                var error = $"AcknowledgeTripViaQueueTrigger failed: {ex.Message}";
                log.LogError(error);
            }
        }

        //TODO: Implement Get Trip Manager Instances, Restart Trip Manager Instances and Terminate Trip Manager Instances if Persist to table storage if persist instances is activated

        /** PRIVATE **/
        private static async Task StartInstance(IDurableClient context, TripItem trip, string instanceId, ILogger log)
        {
            try
            {
                var reportStatus = await context.GetStatusAsync(instanceId);
                string runningStatus = reportStatus == null ? "NULL" : reportStatus.RuntimeStatus.ToString();
                log.LogInformation($"Instance running status: '{runningStatus}'.");

                if (reportStatus == null || reportStatus.RuntimeStatus != OrchestrationRuntimeStatus.Running)
                {
                    await context.StartNewAsync("O_ManageTrip", instanceId, trip);
                    log.LogInformation($"Started a new trip manager = '{instanceId}'.");

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
                log.LogInformation($"Terminating trip manager '{instanceId}'.");
                await context.TerminateAsync(instanceId, "Via an API request");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
