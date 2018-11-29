using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ServerlessMicroservices.Models.Request
{
    public class CognitiveKeyPhrasesResponse
    {
        [JsonProperty(PropertyName = "documents")]
        public List<KeyPhrasesResponseDocument> Documents { get; set; }
        [JsonProperty(PropertyName = "errors")]
        public List<CognitiveError> Errors { get; set; } = new List<CognitiveError>();
    }

    public class KeyPhrasesResponseDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = "1";
        [JsonProperty(PropertyName = "keyPhrases")]
        public string[] KeyPhrases { get; set; }
    }
}
