using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Models.Request
{
    public class DriverPickupPredictionResponse
    {
        [JsonProperty(PropertyName = "topPrediction")]
        public DriverPickupPredictionPayload TopPrediction { get; set; }
        [JsonProperty(PropertyName = "allPredictions")]
        public List<DriverPickupPredictionPayload> AllPredictions { get; set; }
    }

    public class DriverPickupPredictionPayload
    {
        [JsonProperty(PropertyName = "locationFriendlyName")]
        public string LocationFriendlyName { get; set; }
        [JsonProperty(PropertyName = "location")]
        public TripLocation Location { get; set; }
        [JsonProperty(PropertyName = "predictedPickupRequests")]
        public float PredictedPickupRequests { get; set; }
    }
}
