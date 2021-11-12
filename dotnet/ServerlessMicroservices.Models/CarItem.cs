using System.Text.Json.Serialization;

namespace ServerlessMicroservices.Models
{
    public class CarItem : BaseItem
    {
        [JsonPropertyName("driverCode")]
        public string DriverCode { get; set; } = "";

        [JsonPropertyName("make")]
        public string Make { get; set; } = "";

        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        [JsonPropertyName("year")]
        public string Year { get; set; } = "";

        [JsonPropertyName("color")]
        public string Color { get; set; } = "";

        [JsonPropertyName("licensePlate")]
        public string LicensePlate { get; set; } = "";
    }
}
