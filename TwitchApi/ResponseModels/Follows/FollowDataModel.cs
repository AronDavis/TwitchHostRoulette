using Newtonsoft.Json;
using System;

namespace TwitchHostRoulette.Models.Follows
{
    public class FollowDataModel
    {
        [JsonProperty("followed_at")]
        public DateTime FollowedAt { get; set; }

        [JsonProperty("from_id")]
        public int FromId { get; set; }

        [JsonProperty("from_name")]
        public string FromName { get; set; }

        [JsonProperty("to_id")]
        public int ToId { get; set; }

        [JsonProperty("to_name")]
        public string ToName { get; set; }
    }
}
