using Newtonsoft.Json;

namespace ServerlessMicroservices.Models
{
    public class PassengerItem : BaseItem
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; } = "";
    }
}
