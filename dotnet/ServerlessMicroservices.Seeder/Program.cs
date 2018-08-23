using ServerlessMicroservices.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Seeder
{
    class Program
    {
        private static string _tripTestParametersUrl = "";

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You need to pass the test parameters URL as an argument. Press any key to exit...");
            }
            else
            {
                _tripTestParametersUrl = args[0];
                int iterations = args[1] != null ? Int32.Parse(args[1]) : 1;
                int seconds = args[2] != null ? Int32.Parse(args[2]) : 60;

                for (int i = 0; i < iterations; i++)
                {
                    await TestTrips();
                    await Task.Delay(seconds * 1000);
                    Console.WriteLine($"Iteration {i} completed");
                }

                Console.WriteLine("Test is completed. Press any key to exit...");
            }

            Console.ReadLine();
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
                Console.WriteLine($"Trip - Simulate a little delay....");
                await Task.Delay(Utilities.GenerateRandomInteger(5000));

                var startTime = DateTime.Now;
                Console.WriteLine($"Trip - Passenger Code: {passengerCode} ....");
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTripRunner failed: {ex.Message}");
                result.Error = ex.Message;
            }

            return result;
        }
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
