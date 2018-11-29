using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Models.Request
{
    public class CognitiveRequest
    {
        [JsonProperty(PropertyName = "documents")]
        public List<RequestDocument> Documents { get; set; }
    }

    public class RequestDocument
    {
        [JsonProperty(PropertyName = "language")]
        public string Language { get; set; } = "en";
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = "1";
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }
    }
}
