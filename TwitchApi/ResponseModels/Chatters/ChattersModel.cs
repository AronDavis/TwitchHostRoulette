using Newtonsoft.Json;

namespace TwitchApi.ResponseModels.Chatters
{
    public class ChattersModel
    {
        [JsonProperty("chatters")]
        public ChattersData Chatters { get; set; }
    }
}
