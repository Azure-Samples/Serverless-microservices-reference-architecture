using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Models.Request
{
    public class CognitiveSentimentResponse
    {
        [JsonProperty(PropertyName = "documents")]
        public List<SentimentResponseDocument> Documents { get; set; }
        [JsonProperty(PropertyName = "errors")]
        public List<CognitiveError> Errors { get; set; } = new List<CognitiveError>();
    }

    public class SentimentResponseDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = "1";
        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }
    }
}
