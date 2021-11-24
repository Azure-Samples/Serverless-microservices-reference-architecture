

using Newtonsoft.Json;

namespace ServerlessMicroservices.Models
{
    public class DriverItem : BaseItem
    {
        [JsonProperty("code")]
        public string Code { get; set; } = "";

        [JsonProperty("firstName")]
        public string FirstName { get; set; } = "";

        [JsonProperty("lastName")]
        public string LastName { get; set; } = "";

        [JsonProperty("latitude")]
        public double Latitude { get; set; } = 0;

        [JsonProperty("longitude")]
        public double Longitude { get; set; } = 0;

        [JsonProperty("car")]
        public CarItem Car { get; set; } = new CarItem();

        [JsonProperty("isAcceptingRides")]
        public bool IsAcceptingRides { get; set; } = true;
    }

    public class DriverLocationItem : BaseItem
    {
        [JsonProperty("driverCode")]
        public string DriverCode { get; set; } = "";

        [JsonProperty("latitude")]
        public double Latitude { get; set; } = 0;

        [JsonProperty("longitude")]
        public double Longitude { get; set; } = 0;
    }
}
