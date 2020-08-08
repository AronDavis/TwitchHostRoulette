using Newtonsoft.Json;

namespace TwitchApi.ResponseModels.Chatters
{
    public class ChattersData
    {
        [JsonProperty("vips")]
        public string[] VIPs { get; set; }

        [JsonProperty("moderators")]
        public string[] Moderators { get; set; }

        [JsonProperty("staff")]
        public string[] Staff { get; set; }

        [JsonProperty("admins")]
        public string[] Admins { get; set; }

        [JsonProperty("global_mods")]
        public string[] GlobalMods { get; set; }

        [JsonProperty("viewers")]
        public string[] Viewers { get; set; }
    }
}
