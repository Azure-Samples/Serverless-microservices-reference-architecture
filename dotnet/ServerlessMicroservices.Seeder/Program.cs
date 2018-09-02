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
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.FullName = "RideShare Seed & Test Commands";
            app.HelpOption("--help");

            // `seed` Seed Command
            var seedUrlOption = app.Option("-t|--seedbaseurl", "Set seed base url", CommandOptionType.SingleValue, true);
            var seedGetDriversFunctionCodeOption = app.Option("-t|--seedgetdriverscode", "Set seed get drivers function cpde", CommandOptionType.SingleValue, true);
            var seedPostDriversFunctionCodeOption = app.Option("-t|--seedpostdriverscode", "Set seed post drivers function cpde", CommandOptionType.SingleValue, true);
            var seedGetPassengersFunctionCodeOption = app.Option("-t|--seedgetpassengerscode", "Set seed get passengers function cpde", CommandOptionType.SingleValue, true);
            var seedPostPassengersFunctionCodeOption = app.Option("-t|--seedpostpassengerscode", "Set seed post passengers function cpde", CommandOptionType.SingleValue, true);

            // `testTrips` Test Trips Command
            var testUrlOption = app.Option("-t|--testurl", "Set test url", CommandOptionType.SingleValue, true);
            var testIterationsOption = app.Option("-i|--testiterations", "Set test iterations", CommandOptionType.SingleValue, true);
            var testSecondsOption = app.Option("-s|--testseconds", "Set test seconds", CommandOptionType.SingleValue, true);

            // `testSignalr` Test SignalR Command
            var signalRInfoUrlOption = app.Option("-v|--signalrinfourl", "Set SignalR Info URL", CommandOptionType.SingleValue, true);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Build();

            app.Command("seed", cmd =>
            {
                cmd.Description = "Seed Drivers & Passengers";
                cmd.HelpOption("--help");

                cmd.OnExecute(async () =>
                {
                    var baseUrl = seedUrlOption.Value();
                    var getDriversCode = seedGetDriversFunctionCodeOption.Value();
                    var postDriversCode = seedPostDriversFunctionCodeOption.Value();
                    var getPassengersCode = seedGetPassengersFunctionCodeOption.Value();
                    var postPassengersCode = seedPostPassengersFunctionCodeOption.Value();

                    if (string.IsNullOrEmpty(baseUrl) || 
                        !seedUrlOption.HasValue() ||
                        string.IsNullOrEmpty(getDriversCode) ||
                        !seedGetDriversFunctionCodeOption.HasValue() ||
                        string.IsNullOrEmpty(postDriversCode) ||
                        !seedPostDriversFunctionCodeOption.HasValue() ||
                        string.IsNullOrEmpty(getPassengersCode) ||
                        !seedGetPassengersFunctionCodeOption.HasValue() ||
                        string.IsNullOrEmpty(postPassengersCode) ||
                        !seedPostPassengersFunctionCodeOption.HasValue()
                        )
                    {
                        MissingSeedOptions();
                        return 0;
                    }

                    await Seed(baseUrl, getDriversCode, postDriversCode, getPassengersCode, postPassengersCode);
                    return 0;
                });
            });

            app.Command("testTrips", cmd =>
            {
                cmd.Description = "Test Demo Trips";
                cmd.HelpOption("--help");

                cmd.OnExecute(async () =>
                {
                    var url = testUrlOption.Value();

                    if (string.IsNullOrEmpty(url) || !testUrlOption.HasValue())
                    {
                        MissingTestTripsOptions();
                        return 0;
                    }

                    int iterations = testIterationsOption.HasValue() ? Int32.Parse(testIterationsOption.Value()) : 1;
                    int seconds = testSecondsOption.HasValue() ? Int32.Parse(testSecondsOption.Value()) : 60;

                    for (int i = 0; i < iterations; i++)
                    {
                        if (i > 0)
                        {
                            Console.WriteLine($"Delaying for {seconds} seconds before starting iteration {i}....");
                            await Task.Delay(seconds * 1000);
                        }

                        Console.WriteLine($"Iteration {i} starting....");
                        await TestTrips(url);
                        Console.WriteLine($"Iteration {i} completed");
                    }

                    return 0;
                });
            });

            app.Command("testSignalr", cmd =>
            {
                cmd.Description = "Test SignalR .NET Client";
                cmd.HelpOption("--help");

                cmd.OnExecute(async () =>
                {
                    string url = signalRInfoUrlOption.HasValue() ? signalRInfoUrlOption.Value() : "Dev";
                    if (string.IsNullOrEmpty(url) || !testUrlOption.HasValue())
                    {
                        MissingTestSignalROptions();
                        return 0;
                    }

                    await TestSignalR(url);
                    return 0;
                });
            });

            app.Execute(args);
        }

        //*** Auxiliary Methods ***//

        /*
         * Seed 
         */
        static async Task Seed(string baseUrl, string getDriversFunctionCode, string postDriversFunctionCode, string getPassengersFunctionCodestring, string postPassengersFunctionCode)
        {
            // Read existing entities
            List<DriverItem> drivers = await Utilities.Get<List<DriverItem>>(null, $"{baseUrl}/api/drivers?code={getDriversFunctionCode}", new Dictionary<string, string>());
            List<PassengerItem> passengers = await Utilities.Get<List<PassengerItem>>(null, $"{baseUrl}/api/passengers?code={getPassengersFunctionCodestring}", new Dictionary<string, string>());

            if (drivers == null || drivers.Count == 0)
            {
                drivers = new List<DriverItem>()
                {
                    new DriverItem() { Code = "AA100", FirstName = "James", LastName = "Beaky", Latitude = 31.7157, Longitude = 117.1611, Car = new  CarItem () { DriverCode = "AA100", Make = "BMW", Model = "735", Year = "2015", Color = "Silver", LicensePlate = "CA-91099"} },
                    new DriverItem() { Code = "AA110", FirstName = "Rod", LastName = "Snow", Latitude = 34.0552, Longitude = 118.2437, Car = new  CarItem () { DriverCode = "AA110", Make = "Toyota", Model = "Camry", Year = "2013", Color = "Gray", LicensePlate = "CA-78209"} },
                    new DriverItem() { Code = "AA120", FirstName = "Sam", LastName = "Imada", Latitude = 37.7749, Longitude = 122.4194, Car = new  CarItem () { DriverCode = "AA120", Make = "Honda", Model = "Accord", Year = "2017", Color = "Black", LicensePlate = "CA-76215"} }
                };

                foreach(var driver in drivers)
                {
                    await Utilities.Post<dynamic, dynamic>(null, driver, $"{baseUrl}/api/drivers?code={postDriversFunctionCode}", new Dictionary<string, string>());
                }
            }
            else
                Console.WriteLine("No need to seed ...there are drivers in the solution!");

            if (passengers == null || passengers.Count == 0)
            {
                passengers = new List<PassengerItem>()
                {
                    new PassengerItem() { Code = "joe.kassini@gmail.com", FirstName = "Joe", LastName = "Kassini", MobileNumber =  "3105551212", Email = "joe.kassini@gmail.com" },
                    new PassengerItem() { Code = "rob.dart@gmail.com", FirstName = "Rob", LastName = "Dart", MobileNumber =  "7145551313", Email = "rob.dart@gmail.com" },
                    new PassengerItem() { Code = "sue.faming@gmail.com", FirstName = "Sue", LastName = "Faming", MobileNumber =  "7145551414", Email = "sue.faming@gmail.com" }
                };

                foreach (var passenger in passengers)
                {
                    await Utilities.Post<dynamic, dynamic>(null, passenger, $"{baseUrl}/api/passengers?code={postPassengersFunctionCode}", new Dictionary<string, string>());
                }
            }
            else
                Console.WriteLine("No need to seed ...there are passengers in the solution!");

            Console.WriteLine("Seed completed......press any key!");
            Console.ReadLine();
        }

        /*
         * This is end-to-end trip testing
         */
        static async Task TestTrips(string url)
        {
            // Read the test parameters
            var tripTasks = await Utilities.Get<List<TripTestParameters>> (null, url, new Dictionary<string, string>());

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
        static async Task TestSignalR(string url)
        {
            // Get the SingalR service url and access token by calling the `signalrinfo` API
            var singnalRInfo = await Utilities.Get<SignalRInfo>(null, url, new Dictionary<string, string>());
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

        private static void MissingSeedOptions()
        {
            Console.WriteLine("Required options: seedbaseurl, seedgetdriverscode, seedpostdriverscode, seedgetpassengerscode, seedpostpassengerscode");
        }

        private static void MissingTestTripsOptions()
        {
            Console.WriteLine("Required options: testUrl");
        }

        private static void MissingTestSignalROptions()
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
