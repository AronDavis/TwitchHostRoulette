using Newtonsoft.Json;

namespace TwitchApi.ResponseModels.Auth
{
    public class UserAccessTokenModel
    {
        [JsonProperty("access_token")]
        public string UserAccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public uint ExpiresInSeconds { get; set; }

        [JsonProperty("scope")]
        public string[] Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
