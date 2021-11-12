using System.Text.Json.Serialization;

namespace ServerlessMicroservices.Models
{
    public class PassengerItem : BaseItem
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = "";

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = "";

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = "";

        [JsonPropertyName("mobileNumber")]
        public string MobileNumber { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

    }
}
