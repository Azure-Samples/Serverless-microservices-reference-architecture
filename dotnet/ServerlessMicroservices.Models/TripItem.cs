using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Models
{
    public enum TripTypes
    {
        Normal,
        Demo
    }

    public class TripItem : BaseItem
    {
        [JsonProperty("code")]
        public string Code { get; set; } = "";

        // Included here ...just in case the passenger state changed ...this captures the passenger state at the time of the trip
        [JsonProperty("passenger")]
        public PassengerItem Passenger { get; set; } = new PassengerItem();

        // Included here ...just in case the driver state changed ...this captures the driver state at the time of the trip
        [JsonProperty("driver")]
        public DriverItem Driver { get; set; } = null;

        // Included here ...just in case the driver state changed ...this captures the available drivers state at the time of the trip
        [JsonProperty("availableDrivers")]
        public List<DriverItem> AvailableDrivers { get; set; } = new List<DriverItem>();
        
        [JsonProperty("source")]
        public TripLocation Source { get; set; } = new TripLocation();

        [JsonProperty("destination")]
        public TripLocation Destination { get; set; } = new TripLocation();

        [JsonProperty("acceptDate")]
        public DateTime? AcceptDate { get; set; } = null;

        [JsonProperty("startDate")]
        public DateTime StartDate  { get; set; } = DateTime.Now;

        [JsonProperty("endDate")]
        public DateTime? EndDate { get; set; } = null;

        // Computed values
        [JsonProperty("duration")]
        public double Duration { get; set; } = 0;

        [JsonProperty("monitorIterations")]
        public int MonitorIterations { get; set; } = 0;

        [JsonProperty("isAborted")]
        public bool IsAborted { get; set; } = false;

        [JsonProperty("error")]
        public string Error { get; set; } = "";

        [JsonProperty("type")]
        public TripTypes Type { get; set; } = TripTypes.Normal;
    }

    public class TripLocation
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; } = 0;

        [JsonProperty("longitude")]
        public double Longitude { get; set; } = 0;
    }

    public class TripDemoState
    {
        [JsonProperty("code")]
        public string Code { get; set; } = "";

        [JsonProperty("source")]
        public TripLocation Source { get; set; } = new TripLocation();

        [JsonProperty("destination")]
        public TripLocation Destination { get; set; } = new TripLocation();

        [JsonProperty("routeLocations")]
        public List<TripLocation> RouteLocations { get; set; } = new List<TripLocation>();

        [JsonProperty("currentRouteIndex")]
        public int CurrentRouteIndex { get; set; } = 0;
    }

    public class TripDriver
    {
        public string TripCode { get; set; } = "";
        public string DriverCode { get; set; } = "";
    }
}
