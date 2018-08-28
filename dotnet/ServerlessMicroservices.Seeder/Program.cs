using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessMicroservices.Models;
using ServerlessMicroservices.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Seeder
{
    class Program
    {
        private static string _tripTestParametersUrl = "";

        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "RideShare Test Sampels";
            app.HelpOption("--help");

            var testUrlOption = app.Option("-t|--testurl", "Set test url", CommandOptionType.SingleValue, true);
            var iterationsOption = app.Option("-i|--iterations", "Set iterations", CommandOptionType.SingleValue, true);
            var secondsOption = app.Option("-s|--seconds", "Set seconds", CommandOptionType.SingleValue, true);
            var envOption = app.Option("-v|--env", "Set Env", CommandOptionType.SingleValue, true);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                //.AddUserSecrets<Program>()
                .Build();

            app.Command("testTrips", cmd =>
            {
                cmd.Description = "Test Demo Trips";
                cmd.HelpOption("--help");

                cmd.OnExecute(async () =>
                {
                    var url = testUrlOption.Value();

                    if (string.IsNullOrEmpty(url) || !testUrlOption.HasValue())
                    {
                        MissingTestDemoOptions();
                        return 0;
                    }

                    int iterations = iterationsOption.HasValue() ? Int32.Parse(iterationsOption.Value()) : 1;
                    int seconds = secondsOption.HasValue() ? Int32.Parse(secondsOption.Value()) : 60;

                    for (int i = 0; i < iterations; i++)
                    {
                        if (i > 0)
                        {
                            Console.WriteLine($"Delaying for {seconds} seconds before starting iteration {i}....");
                            await Task.Delay(seconds * 1000);
                        }

                        Console.WriteLine($"Iteration {i} starting....");
                        await TestTrips();
                        Console.WriteLine($"Iteration {i} completed");
                    }

                    return 0;
                });
            });

            app.Command("signalr", cmd =>
            {
                cmd.Description = "Test SignalR .NET Client";
                cmd.HelpOption("--help");

                cmd.OnExecute(async () =>
                {
                    string env = envOption.HasValue() ? envOption.Value() : "Dev";
                    await TestSignalR(env);
                    return 0;
                });
            });

            app.Execute(args);
        }

        /*
         * Test Routines
         */

        /*
         * This is end-to-end trip testing
         */
        static async Task TestTrips()
        {
            // Read the test parameters
            var tripTasks = await Utilities.Get<List<TripTestParameters>> (null, _tripTestParametersUrl, new Dictionary<string, string>());

            // Launch the test tasks
            List<Task<TripTestResult>> taskRuns = new List<Task<TripTestResult>>();
            foreach (var task in tripTasks)
            {
                taskRuns.Add(TestTripRunner(task.Url, task.PassengerCode, task.PassengerFirstName, task.PassengerLastName, task.PassengerEmail, task.PassengerMobile, task.SourceLatitude, task.SourceLongitude, task.DestinationLatitude, task.DestinationLongitude));
            }

            await Task.WhenAll(taskRuns.ToArray());

            // Display the result for each test thread
            int index = 0;
            foreach (var taskRun in taskRuns)
            {
                var taskResult = taskRuns[index].Result;
                //var taskResult = await tasks[index]; -- this would also work
                Console.WriteLine($"Thread {index} => Duration: {taskResult.Duration} - Error: {taskResult.Error}");
                index++;
            }

            Console.WriteLine("All tasks are finished.");
        }

        static async Task<TripTestResult> TestTripRunner(string url,
                                                         string passengerCode,
                                                         string passengerFirstName,
                                                         string passengerLastName,
                                                         string passengerEmail,
                                                         string passengerMobile,
                                                         double sourceLatitude,
                                                         double sourceLongitude,
                                                         double destinationLatitude,
                                                         double destinationLongitude
                                                         )
        {
            Console.WriteLine($"TestTripRunner - Url {url} started....");
            TripTestResult result = new TripTestResult();

            try
            {
                // Wait a little to avoid thread contention
                Console.WriteLine($"TestTripRunner - Simulate a little delay....");
                await Task.Delay(Utilities.GenerateRandomInteger(10000));

                var startTime = DateTime.Now;
                Console.WriteLine($"TestTripRunner - Passenger Code: {passengerCode} ....");
                var response = await Utilities.Post<dynamic, dynamic>(null, new
                {
                    passenger = new
                    {
                        code = passengerCode,
                        firstName = passengerFirstName,
                        lastName = passengerLastName,
                        mobileNumber = passengerMobile,
                        email = passengerEmail
                    },
                    source = new 
                    {
                        latitude = sourceLatitude,
                        longitude = sourceLongitude
                    },
                    destination = new
                    {
                        latitude = destinationLatitude,
                        longitude = destinationLongitude
                    },
                    type = 1
                }, url, new Dictionary<string, string>());

                var endTime = DateTime.Now;
                result.Duration = (endTime - startTime).TotalSeconds;
                Console.WriteLine($"TestTripRunner - submitted in {result.Duration} seconds.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTripRunner - failed: {ex.Message}");
                result.Error = ex.Message;
            }

            return result;
        }

        /*
         * SignalR testing
         */
        static async Task TestSignalR(string env)
        {
            // Get the SingalR service url and access token by calling the `singnalrinfo` API
            var singnalRInfo = await GetSignalRInfo(env);
            if (singnalRInfo == null)
                throw new Exception("SignalR info is NULL!");

            var connection = new HubConnectionBuilder()
            .WithUrl(singnalRInfo.Endpoint, option =>
            {
                option.AccessTokenProvider = () =>
                {
                    return Task.FromResult(singnalRInfo.AccessKey);
                };
            })
            .ConfigureLogging( logging =>
            {
                logging.AddConsole();
            })
            .Build();

            connection.On<TripItem>("tripUpdated", (trip) =>
            {
                Console.WriteLine($"tripUpdated - {trip.Code}");
            });

            connection.On<TripItem>("tripDriversNotified", (trip) =>
            {
                Console.WriteLine($"tripDriversNotified - {trip.Code}");
            });

            connection.On<TripItem>("tripDriverPicked", (trip) =>
            {
                Console.WriteLine($"tripDriverPicked - {trip.Code}");
            });

            connection.On<TripItem>("tripStarting", (trip) =>
            {
                Console.WriteLine($"tripStarting - {trip.Code}");
            });

            connection.On<TripItem>("tripRunning", (trip) =>
            {
                Console.WriteLine($"tripRunning - {trip.Code}");
            });

            connection.On<TripItem>("tripCompleted", (trip) =>
            {
                Console.WriteLine($"tripCompleted - {trip.Code}");
            });

            connection.On<TripItem>("tripAborted", (trip) =>
            {
                Console.WriteLine($"tripAborted - {trip.Code}");
            });

            await connection.StartAsync();

            Console.WriteLine("SignalR client started....waiting for messages from server. To cancel......press any key!");
            Console.ReadLine();
        }

        private static async Task<SignalRInfo> GetSignalRInfo(string env)
        {
            if (env.ToLower() == "dev")
                return await Utilities.Get<SignalRInfo>(null, "https://ridesharetripsfunctionappdev.azurewebsites.net/api/signalrinfo", new Dictionary<string, string>());
            else 
                return await Utilities.Get<SignalRInfo>(null, "https://ridesharetripsfunctionapp.azurewebsites.net/api/signalrinfo", new Dictionary<string, string>());

        }

        private static void MissingTestDemoOptions()
        {
            Console.WriteLine("Required options: testUrl");
        }
    }

    class SignalRInfo
    {
        public string Endpoint { get; set; }
        public string AccessKey { get; set; }
    }

    class TripTestParameters
    {
        public string Url { get; set; } = "";
        public string PassengerCode { get; set; } = "";
        public string PassengerFirstName { get; set; } = "";
        public string PassengerLastName { get; set; } = "";
        public string PassengerMobile { get; set; } = "";
        public string PassengerEmail { get; set; } = "";
        public double SourceLatitude { get; set; } = 0;
        public double SourceLongitude { get; set; } = 0;
        public double DestinationLatitude { get; set; } = 0;
        public double DestinationLongitude { get; set; } = 0;
    }

    class TripTestResult
    {
        public double Duration { get; set; } = 0;
        public string Error { get; set; } = "";
    }
}
