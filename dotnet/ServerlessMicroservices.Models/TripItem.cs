using Newtonsoft.Json;
using System;

namespace ServerlessMicroservices.Models
{
    public class TripItem : BaseItem
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; } = "";

        // Included here ...just in case the driver state changed ...this captures the driver state at the time of the trip
        [JsonProperty(PropertyName = "driver")]
        public DriverItem Driver { get; set; } = new DriverItem();

        // Included here ...just in case the passenger state changed ...this captures the passenger state at the time of the trip
        [JsonProperty(PropertyName = "passenger")]
        public PassengerItem  Passenger { get; set; } = new PassengerItem();

        [JsonProperty(PropertyName = "source")]
        public TripLocation SourceLocation { get; set; } = new TripLocation();

        [JsonProperty(PropertyName = "destination")]
        public TripLocation Destination { get; set; } = new TripLocation();

        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate  { get; set; } = DateTime.Now;

        [JsonProperty(PropertyName = "endDate")]
        public DateTime? EndDate { get; set; } = null;

        // Computed value
        [JsonProperty(PropertyName = "duration")]
        public double Duration { get; set; } = 0;
    }

    public class TripLocation
    {
        public double Latitude { get; set; } = 0;
        public double Longitude { get; set; } = 0;
    }
}
