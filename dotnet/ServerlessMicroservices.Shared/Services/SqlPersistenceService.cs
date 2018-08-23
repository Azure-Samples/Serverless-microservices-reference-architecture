using Dapper;
using ServerlessMicroservices.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ServerlessMicroservices.Shared.Services
{
    public class SqlPersistenceService : IPersistenceService
    {
        public const string LOG_TAG = "SqlPersistenceService";

        private ISettingService _settingService;
        private ILoggerService _loggerService;

        public SqlPersistenceService(ISettingService setting, ILoggerService logger)
        {
            _settingService = setting;
            _loggerService = logger;
        }

        public Task<DriverItem> RetrieveDriver(string code)
        {
            throw new NotImplementedException();
        }

        public Task<List<DriverItem>> RetrieveDrivers(int max = 20)
        {
            throw new NotImplementedException();
        }

        public Task<int> RetrieveDriversCount()
        {
            throw new NotImplementedException();
        }

        public Task<DriverItem> UpsertDriver(DriverItem driver, bool isIgnoreChangeFeed = false)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpsertDriverLocation(DriverLocationItem driver, bool isIgnoreChangeFeed = false)
        {
            throw new NotImplementedException();
        }

        public Task<List<DriverLocationItem>> RetrieveDriverLocations(string code, int max = 20)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDriver(string code)
        {
            throw new NotImplementedException();
        }

        public Task<List<DriverItem>> RetrieveDrivers(double latitude, double longitude, double miles, int max = 20)
        {
            throw new NotImplementedException();
        }

        public Task<List<DriverItem>> RetrieveActiveDrivers(int max = 20)
        {
            throw new NotImplementedException();
        }

        public Task<TripItem> RetrieveTrip(string code)
        {
            throw new NotImplementedException();
        }

        public Task<List<TripItem>> RetrieveTrips(int max = 20)
        {
            throw new NotImplementedException();
        }

        public Task<List<TripItem>> RetrieveTrips(double latitude, double longitude, double miles, int max = 20)
        {
            throw new NotImplementedException();
        }

        public Task<List<TripItem>> RetrieveActiveTrips(int max = 20)
        {
            throw new NotImplementedException();
        }

        public Task<int> RetrieveTripsCount()
        {
            throw new NotImplementedException();
        }

        public async Task<TripItem> UpsertTrip(TripItem trip, bool isIgnoreChangeFeed = false)
        {
            var error = "";

            try
            {
                var connectionString = _settingService.GetSqlConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                    throw new Exception("No SQL connection string!");

                //This is the table generation script
                /*

                USE [RideShare]
                GO

                SET ANSI_NULLS ON
                GO

                SET QUOTED_IDENTIFIER ON
                GO

                CREATE TABLE[dbo].TripFact (
                    [Id][int] IDENTITY(1, 1) NOT NULL,
                    [StartDate][datetime] NOT NULL,
                    [EndDate][datetime] NULL,
                    [AcceptDate][datetime] NULL,
                    [TripCode] [nvarchar] (20) NOT NULL,
	                [PassengerCode] [nvarchar] (20) NULL,
	                [PassengerName] [nvarchar] (100) NULL,
	                [PassengerEmail] [nvarchar] (100) NULL,
	                [AvailableDrivers] [int] NULL,
	                [DriverCode] [nvarchar] (20) NULL,
	                [DriverName] [nvarchar] (100) NULL,
	                [DriverLatitude] [float] NULL,
	                [DriverLongitude] [float] NULL,
	                [DriverCarMake] [nvarchar] (100) NULL,
	                [DriverCarModel] [nvarchar] (100) NULL,
	                [DriverCarYear] [nvarchar] (4) NULL,
	                [DriverCarColor] [nvarchar] (20) NULL,
	                [DriverCarLicensePlate] [nvarchar] (20) NULL,
	                [SourceLatitude] [float] NULL,
	                [SourceLongitude] [float] NULL,
	                [DestinationLatitude] [float] NULL,
	                [DestinationLongitude] [float] NULL,
	                [Duration] [float] NULL,
                    [MonitorIterations] [int] NULL,
	                [Status] [nvarchar] (20) NULL,
	                [Error] [nvarchar] (200) NULL,
	                [Mode] [nvarchar] (20) NULL
                 CONSTRAINT[PK_dbo.TripFact] PRIMARY KEY CLUSTERED
                (
                   [Id] ASC
                )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
                )

                GO

                CREATE INDEX IX_TRIP_START_DATE ON dbo.TripFact(StartDate);
                CREATE INDEX IX_TRIP_CODE ON dbo.TripFact(TripCode);
                CREATE INDEX IX_TRIP_PASSENGER_CODE ON dbo.TripFact(PassengerCode);
                CREATE INDEX IX_TRIP_DRIVER_CODE ON dbo.TripFact(DriverCode);
                 
                 */

                // Write a flatten trip record in TripFact table so it can be used in PowerBI, for example:
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    var sql = @"
                        INSERT INTO dbo.TripFact
                        (
                        StartDate,
                        EndDate,
                        AcceptDate,
                        TripCode,
                        PassengerCode,
                        PassengerName,
                        PassengerEmail,
                        AvailableDrivers,
                        DriverCode,
                        DriverName,
                        DriverLatitude,
                        DriverLongitude,
                        DriverCarMake,
                        DriverCarModel,
                        DriverCarYear,
                        DriverCarColor,
                        DriverCarLicensePlate,
                        SourceLatitude,
                        SourceLongitude,
                        DestinationLatitude,
                        DestinationLongitude,
                        Duration,
                        MonitorIterations,
                        Status,
                        Error,
                        Mode
                        )
                        VALUES
                        (
                        @startDate,
                        @endDate,
                        @acceptDate,
                        @tripCode,
                        @passengerCode,
                        @passengerName,
                        @passengerEmail,
                        @availableDrivers,
                        @driverCode,
                        @driverName,
                        @driverLatitude,
                        @driverLongitude,
                        @driverCarMake,
                        @driverCarModel,
                        @driverCarYear,
                        @driverCarColor,
                        @driverCarLicensePlate,
                        @sourceLatitude,
                        @sourceLongitude,
                        @destinationLatitude,
                        @destinationLongitude,
                        @duration,
                        @monitorIterations,
                        @status,
                        @error,
                        @mode
                        )
                        ";
                    _loggerService.Log($"{LOG_TAG} - UpsertTrip - SQL: {sql}");
                    conn.Execute(sql, new
                    {
                        startDate = trip.StartDate,
                        endDate = trip.EndDate,
                        acceptDate = trip.AcceptDate,
                        tripCode = trip.Code,
                        passengerCode = trip.Passenger != null ? trip.Passenger.Code : "",
                        passengerName = trip.Passenger != null ? $"{trip.Passenger.FirstName} {trip.Passenger.LastName}" : "",
                        passengerEmail = trip.Passenger != null ? trip.Passenger.Email : "",
                        availableDrivers = trip.AvailableDrivers.Count,
                        driverCode = trip.Driver != null ? trip.Driver.Code : "",
                        driverName = trip.Driver != null ? $"{trip.Driver.FirstName} {trip.Driver.LastName}" : "",
                        driverLatitude = trip.Driver != null ? trip.Driver.Latitude : 0,
                        driverLongitude = trip.Driver != null ? trip.Driver.Longitude : 0,
                        driverCarMake = trip.Driver != null ? trip.Driver.Car.Make : "",
                        driverCarModel = trip.Driver != null ? trip.Driver.Car.Model : "",
                        driverCarYear = trip.Driver != null ? trip.Driver.Car.Year : "",
                        driverCarColor = trip.Driver != null ? trip.Driver.Car.Color : "",
                        driverCarLicensePlate = trip.Driver != null ? trip.Driver.Car.LicensePlate : "",
                        sourceLatitude = trip.Source != null ? trip.Source.Latitude: 0,
                        sourceLongitude = trip.Source != null ? trip.Source.Longitude : 0,
                        destinationLatitude = trip.Destination != null ? trip.Destination.Latitude : 0,
                        destinationLongitude = trip.Destination != null ? trip.Destination.Longitude : 0,
                        duration = trip.Duration,
                        monitorIterations = trip.MonitorIterations,
                        status = trip.IsAborted ? "Aborted" : "Completed",
                        error = trip.Error,
                        mode = trip.Type == TripTypes.Normal ? "Normal" : "Demo" 
                    });
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw new Exception(error);
            }
            finally
            {
                _loggerService.Log($"{LOG_TAG} - UpsertTrip - Error: {error}");
            }

            return trip;
        }

        public Task DeleteTrip(string code)
        {
            throw new NotImplementedException();
        }

        public Task<TripItem> AssignTripAvailableDrivers(TripItem trip, List<DriverItem> drivers)
        {
            throw new NotImplementedException();
        }

        public Task<TripItem> AssignTripDriver(TripItem trip, string driverCode)
        {
            throw new NotImplementedException();
        }

        public Task RecycleTripDriver(TripItem trip)
        {
            throw new NotImplementedException();
        }

        public Task<TripItem> CheckTripCompletion(TripItem trip)
        {
            throw new NotImplementedException();
        }

        public Task<TripItem> AbortTrip(TripItem trip)
        {
            throw new NotImplementedException();
        }

        public Task<int> RetrieveActiveTripsCount()
        {
            throw new NotImplementedException();
        }
    }
}
