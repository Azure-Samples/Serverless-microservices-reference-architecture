using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Models.Request
{
    /// <summary>
    /// Data comes from custom-trained deep learning model (Neural Net with LSTM cells (Long Short-Term Memory))
    /// that predicts the number of fares per pickup location, accurately within 15 days out from historical
    /// data used to train the model. This emphasises the need to re-train the model for accuracy on a weekly basis.
    /// </summary>
    public class DriverPickupPrediction
    {
        [JsonProperty(PropertyName = "predictions")]
        public Prediction[] Predictions { get; set; }
        [JsonProperty(PropertyName = "location")]
        public TripLocation Location { get; set; }
    }

    public class Prediction
    {
        /// <summary>
        /// Predicted pickup requests (ppr) for the location/date.
        /// </summary>
        [JsonProperty(PropertyName = "ppr")]
        public float Ppr { get; set; }
        /// <summary>
        /// Date (d)
        /// </summary>
        [JsonProperty(PropertyName = "d")]
        public string D { get; set; }
        /// <summary>
        /// Pickup requests (pr) are actual values from historical data when available.
        /// For comparison of real values in training data vs. predicted. If future date,
        /// this will always be zero.
        /// </summary>
        [JsonProperty(PropertyName = "pr")]
        public float Pr { get; set; }
    }

}
