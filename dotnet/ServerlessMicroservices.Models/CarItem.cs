using Newtonsoft.Json;

namespace ServerlessMicroservices.Models
{
    public class CarItem : BaseItem
    {
        [JsonProperty(PropertyName = "driverCode")]
        public string DriverCode { get; set; } = "";

        [JsonProperty(PropertyName = "make")]
        public string Make { get; set; } = "";

        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; } = "";

        [JsonProperty(PropertyName = "year")]
        public string Year { get; set; } = "";

        [JsonProperty(PropertyName = "color")]
        public string Color { get; set; } = "";

        [JsonProperty(PropertyName = "licensePlate")]
        public string LicensePlate { get; set; } = "";

        [JsonProperty(PropertyName = "dailyTripDuration")]
        public double DailyTripDuration { get; set; } = 0;

        [JsonProperty(PropertyName = "lifetimeBatteryCyclesUsed")]
        public double LifetimeBatteryCyclesUsed { get; set; } = 0;

        [JsonProperty(PropertyName = "batteryRatedCycles")]
        public int BatteryRatedCycles { get; set; } = 200;

        [JsonProperty(PropertyName = "batteryAgeDays")]
        public int BatteryAgeDays { get; set; } = 0;
    }
}
