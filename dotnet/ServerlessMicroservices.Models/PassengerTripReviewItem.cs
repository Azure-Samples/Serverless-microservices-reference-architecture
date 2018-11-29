using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Models
{
    public class PassengerTripReviewItem : BaseItem, IPassengerTripReview
    {
        public double Sentiment { get; set; }
        public string DriverCode { get; set; }
        public decimal DriverRating { get; set; }
        public string Review { get; set; }
        public List<string> KeyPhrases { get; set; } = new List<string>();
        // Reference
        [JsonProperty(PropertyName = "passengerCode")]
        public string PassengerCode { get; set; }
        [JsonProperty(PropertyName = "tripCode")]
        public string TripCode { get; set; }
    }

    public interface IPassengerTripReview
    {
        [JsonProperty(PropertyName = "sentiment")]
        double Sentiment { get; set; }
        [JsonProperty(PropertyName = "driverCode")]
        string DriverCode { get; set; }
        [JsonProperty(PropertyName = "driverRating")]
        decimal DriverRating { get; set; }
        [JsonProperty(PropertyName = "review")]
        string Review { get; set; }
        [JsonProperty(PropertyName = "keyPhrases")]
        List<string> KeyPhrases { get; set; }
    }

    public class PassengerTripReview : IPassengerTripReview
    {
        public double Sentiment { get; set; }
        public string DriverCode { get; set; }
        public decimal DriverRating { get; set; }
        public string Review { get; set; }
        public List<string> KeyPhrases { get; set; }
    }
}
