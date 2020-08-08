using Newtonsoft.Json;

namespace TwitchApi.ResponseModels.Games
{
    class GamesModel
    {
        [JsonProperty("data")]
        public GameData[] Data { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
}
