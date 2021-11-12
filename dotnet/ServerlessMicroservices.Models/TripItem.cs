using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ServerlessMicroservices.Models
{
    public enum TripTypes
    {
        Normal,
        Demo
    }

    public class TripItem : BaseItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        // Included here ...just in case the passenger state changed ...this captures the passenger state at the time of the trip
        [JsonPropertyName("passenger")]
        public PassengerItem Passenger { get; set; } = new PassengerItem();

        // Included here ...just in case the driver state changed ...this captures the driver state at the time of the trip
        [JsonPropertyName("driver")]
        public DriverItem Driver { get; set; } = null;

        // Included here ...just in case the driver state changed ...this captures the available drivers state at the time of the trip
        [JsonPropertyName("availableDrivers")]
        public List<DriverItem> AvailableDrivers { get; set; } = new List<DriverItem>();
        
        [JsonPropertyName("source")]
        public TripLocation Source { get; set; } = new TripLocation();

        [JsonPropertyName("destination")]
        public TripLocation Destination { get; set; } = new TripLocation();

        [JsonPropertyName("acceptDate")]
        public DateTime? AcceptDate { get; set; } = null;

        [JsonPropertyName("startDate")]
        public DateTime StartDate  { get; set; } = DateTime.Now;

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; } = null;

        // Computed values
        [JsonPropertyName("duration")]
        public double Duration { get; set; } = 0;

        [JsonPropertyName("monitorIterations")]
        public int MonitorIterations { get; set; } = 0;

        [JsonPropertyName("isAborted")]
        public bool IsAborted { get; set; } = false;

        [JsonPropertyName("error")]
        public string Error { get; set; } = "";

        [JsonPropertyName("type")]
        public TripTypes Type { get; set; } = TripTypes.Normal;
    }

    public class TripLocation
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; } = 0;

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; } = 0;
    }

    public class TripDemoState
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("source")]
        public TripLocation Source { get; set; } = new TripLocation();

        [JsonPropertyName("destination")]
        public TripLocation Destination { get; set; } = new TripLocation();

        [JsonPropertyName("routeLocations")]
        public List<TripLocation> RouteLocations { get; set; } = new List<TripLocation>();

        [JsonPropertyName("currentRouteIndex")]
        public int CurrentRouteIndex { get; set; } = 0;
    }

    public class TripDriver
    {
        public string TripCode { get; set; } = "";
        public string DriverCode { get; set; } = "";
    }
}
