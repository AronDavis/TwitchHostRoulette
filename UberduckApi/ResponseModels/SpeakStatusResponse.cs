using Newtonsoft.Json;
using System;

namespace UberduckApi.ResponseModels
{
    public class SpeakStatusResponse
    {
        [JsonProperty("started_at")]
        public DateTime? StartedAt { get; set; }

        [JsonProperty("failed_at")]
        public DateTime? FailedAt { get; set; }

        [JsonProperty("finished_at")]
        public DateTime? FinishedAt { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
