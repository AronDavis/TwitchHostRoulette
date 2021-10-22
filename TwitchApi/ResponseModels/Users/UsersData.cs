using Newtonsoft.Json;

namespace TwitchApi.ResponseModels.Users
{
    public class UsersData
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
