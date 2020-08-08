using Newtonsoft.Json;
using TwitchApi.ResponseModels;

namespace TwitchHostRoulette.Models.Follows
{
    public class FollowsModel
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("data")]
        public FollowDataModel[] Data { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
}
