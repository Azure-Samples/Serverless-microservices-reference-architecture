using System.Text.Json.Serialization;

namespace ServerlessMicroservices.Models
{
    public class DriverItem : BaseItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = "";

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = "";

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; } = 0;

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; } = 0;

        [JsonPropertyName("car")]
        public CarItem Car { get; set; } = new CarItem();

        [JsonPropertyName("isAcceptingRides")]
        public bool IsAcceptingRides { get; set; } = true;
    }

    public class DriverLocationItem : BaseItem
    {
        [JsonPropertyName("driverCode")]
        public string DriverCode { get; set; } = "";

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; } = 0;

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; } = 0;
    }
}
