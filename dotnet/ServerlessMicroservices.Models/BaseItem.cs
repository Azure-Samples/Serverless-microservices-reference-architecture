using System;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("_self")]
        public string Self { get; set; } = "";

        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = "";

        [JsonPropertyName("upsertDate")]
        public DateTime UpsertDate { get; set; } = DateTime.Now;

        [JsonPropertyName("collectionType")]
        public ItemCollectionTypes CollectionType { get; set; }
    }
}
