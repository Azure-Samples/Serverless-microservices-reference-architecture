using Newtonsoft.Json;

namespace ServerlessMicroservices.Models
{
    public class DriverItem : BaseItem
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; } = "";

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; } = "";

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; } = "";

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; } = 0;

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; } = 0;

        [JsonProperty(PropertyName = "car")]
        public CarItem Car { get; set; } = new CarItem();

        [JsonProperty(PropertyName = "isAcceptingRides")]
        public bool IsAcceptingRides { get; set; } = true;
    }

    public class DriverLocationItem : BaseItem
    {
        [JsonProperty(PropertyName = "driverCode")]
        public string DriverCode { get; set; } = "";

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; } = 0;

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; } = 0;
    }
}
