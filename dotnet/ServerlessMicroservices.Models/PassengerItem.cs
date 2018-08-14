using Newtonsoft.Json;

namespace ServerlessMicroservices.Models
{
    public class PassengerItem : BaseItem
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; } = "";

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; } = "";

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; } = "";

        [JsonProperty(PropertyName = "mobileNumber")]
        public string MobileNumber { get; set; } = "";

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; } = "";

    }
}
