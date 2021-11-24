using Newtonsoft.Json;
using System;

namespace ServerlessMicroservices.Models
{
    // The DigitalMemberships cosmos collection contains several documents of different types. This enumeration is a differatiator
    public enum ItemCollectionTypes
    {
        Driver,
        DriverLocation,
        Car,
        Trip,
        Passenger
    }

    public class BaseItem
    {
        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("self")]
        public string Self { get; set; } = "";

        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; } = "";

        [JsonProperty("upsertDate")]
        public DateTime UpsertDate { get; set; } = DateTime.Now;

        [JsonProperty("collectionType")]
        public ItemCollectionTypes CollectionType { get; set; }
    }
}
