using Newtonsoft.Json;
using System;

namespace UberduckApi.ResponseModels
{
    public class SpeakResponse
    {
        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        [JsonProperty("detail")]
        public string ErrorMessage { get; set; }
    }
}
