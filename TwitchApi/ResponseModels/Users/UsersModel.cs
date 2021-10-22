using Newtonsoft.Json;

namespace TwitchApi.ResponseModels.Users
{
    public class UsersModel
    {
        [JsonProperty("data")]
        public UsersData[] Data { get; set; }
    }
}
