using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ServerlessMicroservices.Models
{
    public class TripItem : BaseItem
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; } = "";

        // Included here ...just in case the passenger state changed ...this captures the passenger state at the time of the trip
        [JsonProperty(PropertyName = "passenger")]
        public PassengerItem Passenger { get; set; } = new PassengerItem();

        // Included here ...just in case the driver state changed ...this captures the driver state at the time of the trip
        [JsonProperty(PropertyName = "driver")]
        public DriverItem Driver { get; set; } = null;

        // Included here ...just in case the driver state changed ...this captures the available drivers state at the time of the trip
        [JsonProperty(PropertyName = "availableDrivers")]
        public List<DriverItem> AvailableDrivers { get; set; } = new List<DriverItem>();
        
        [JsonProperty(PropertyName = "source")]
        public TripLocation SourceLocation { get; set; } = new TripLocation();

        [JsonProperty(PropertyName = "destination")]
        public TripLocation Destination { get; set; } = new TripLocation();

        [JsonProperty(PropertyName = "acceptDate")]
        public DateTime? AcceptDate { get; set; } = null;

        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate  { get; set; } = DateTime.Now;

        [JsonProperty(PropertyName = "endDate")]
        public DateTime? EndDate { get; set; } = null;

        // Computed values
        [JsonProperty(PropertyName = "duration")]
        public double Duration { get; set; } = 0;

        [JsonProperty(PropertyName = "monitorIterations")]
        public int MonitorIterations { get; set; } = 0;

        [JsonProperty(PropertyName = "isAborted")]
        public bool IsAborted { get; set; } = false;

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; } = "";
    }

    public class TripLocation
    {
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
    }
}
