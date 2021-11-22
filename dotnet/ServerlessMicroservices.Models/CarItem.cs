using Newtonsoft.Json;

namespace ServerlessMicroservices.Models
{
    public class CarItem : BaseItem
    {
        [JsonProperty("driverCode")]
        public string DriverCode { get; set; } = "";

        [JsonProperty("make")]
        public string Make { get; set; } = "";

        [JsonProperty("model")]
        public string Model { get; set; } = "";

        [JsonProperty("year")]
        public string Year { get; set; } = "";

        [JsonProperty("color")]
        public string Color { get; set; } = "";

        [JsonProperty("licensePlate")]
        public string LicensePlate { get; set; } = "";
    }
}
