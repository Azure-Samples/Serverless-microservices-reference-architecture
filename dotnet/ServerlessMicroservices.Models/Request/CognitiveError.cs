using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Models.Request
{
    public class CognitiveError
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "innerError")]
        public InnerError InnerError { get; set; }
    }

    public class InnerError
    {
        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }
    }
}
