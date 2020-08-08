using Newtonsoft.Json;

namespace TwitchApi.ResponseModels.Streams
{
    public class StreamsModel
    {
        [JsonProperty("data")]
        public StreamData[] Data { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
}
