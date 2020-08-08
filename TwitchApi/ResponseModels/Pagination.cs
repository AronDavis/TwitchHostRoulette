using Newtonsoft.Json;

namespace TwitchApi.ResponseModels
{
    public class Pagination
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }
}
