using Newtonsoft.Json;

namespace ServerlessMicroservices.Models
{
    public class PassengerItem : BaseItem
    {
        [JsonProperty("code")]
        public string Code { get; set; } = "";

        [JsonProperty("firstName")]
        public string FirstName { get; set; } = "";

        [JsonProperty("lastName")]
        public string LastName { get; set; } = "";

        [JsonProperty("mobileNumber")]
        public string MobileNumber { get; set; } = "";

        [JsonProperty("email")]
        public string Email { get; set; } = "";

    }
}
