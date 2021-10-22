using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchApi.ResponseModels.Chatters;
using TwitchApi.ResponseModels.Streams;
using TwitchApi.ResponseModels.Users;
using TwitchHostRoulette.Models.Follows;

namespace TwitchApi
{
    public class TwitchApiClient
    {
        private HttpClient _client = new HttpClient();

        public async Task<int> GetUserId(string username, string oauthToken, string clientId)
        {
            string url = "https://api.twitch.tv/helix/users";

            url = QueryHelpers.AddQueryString(url, "login", username);

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);
            message.Headers.Add("Authorization", $"Bearer { oauthToken }");
            message.Headers.Add("client-id", clientId);

            HttpResponseMessage results = await _client.SendAsync(message);
            string jsonString = await results.Content.ReadAsStringAsync();
            UsersModel data = JsonConvert.DeserializeObject<UsersModel>(jsonString);

            return data.Data[0].Id;
        }

        public async Task<ChattersModel> GetChatters(string user)
        {
            var url = $"https://tmi.twitch.tv/group/user/{user}/chatters";

            HttpResponseMessage results = await _client.GetAsync(url);
            string jsonString = await results.Content.ReadAsStringAsync();
            ChattersModel model = JsonConvert.DeserializeObject<ChattersModel>(jsonString);

            return model;
        }

        public async Task<FollowsModel> GetFollows(int toId, string oauthToken, string clientId, string cursor = null)
        {
            var url = "https://api.twitch.tv/helix/users/follows";

            url = QueryHelpers.AddQueryString(url, "first", 100.ToString()); //returns 100 followers in the data object

            url = QueryHelpers.AddQueryString(url, "to_id", toId.ToString());

            if (cursor != null)
                url = QueryHelpers.AddQueryString(url, "after", cursor);

            HttpResponseMessage response = await _client.SendAsync(new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
                Headers = {
                    { "Authorization", $"Bearer {oauthToken}" },
                    { "client-id", clientId },
                },
            });
            string jsonString = await response.Content.ReadAsStringAsync();
            FollowsModel data = JsonConvert.DeserializeObject<FollowsModel>(jsonString);

            return data;
        }

        public async Task<FollowsModel> GetPeopleUserFollows(int fromId, string oauthToken, string clientId, string cursor = null)
        {
            var url = "https://api.twitch.tv/helix/users/follows";

            url = QueryHelpers.AddQueryString(url, "first", 100.ToString()); //returns 100 followers in the data object

            url = QueryHelpers.AddQueryString(url, "from_id", fromId.ToString());

            if (cursor != null)
                url = QueryHelpers.AddQueryString(url, "after", cursor);

            HttpResponseMessage response = await _client.SendAsync(new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get,
                Headers = {
                    { "Authorization", $"Bearer {oauthToken}" },
                    { "client-id", clientId },
                },
            });
            string jsonString = await response.Content.ReadAsStringAsync();
            FollowsModel data = JsonConvert.DeserializeObject<FollowsModel>(jsonString);

            return data;
        }

        public async Task<List<StreamData>> GetStreams(int numberOfStreamsToRetrieve, Func<StreamData, bool> filter, string userLogin, string gameId = null)
        {
            List<StreamData> streamData = await _getStreamData(numberOfStreamsToRetrieve, filter, userLogin, gameId);

            return streamData;
        }

        private async Task<List<StreamData>> _getStreamData(int numberOfStreamsToRetrieve, Func<StreamData, bool> filter = null, string userLogin = null, string gameId = null)
        {
            List<StreamData> streamData = new List<StreamData>();
            HashSet<string> streamIds = new HashSet<string>();
            StreamsModel current = null;
            while (streamData.Count < numberOfStreamsToRetrieve)
            {
                current = await _getMoreData(current?.Pagination.Cursor, userLogin, gameId);

                foreach (var d in current.Data)
                {
                    if (!streamIds.Contains(d.Id) && d.GameId != string.Empty)
                    {
                        if (filter != null && !filter(d))
                            continue;

                        streamData.Add(d);
                        streamIds.Add(d.Id);

                        if (streamData.Count == numberOfStreamsToRetrieve)
                            break;
                    }
                }

                //if we couldn't find as many as we hoped to find, bail out of the loop
                if (current.Pagination.Cursor == null)
                    break;
            }

            return streamData;
        }

        private async Task<StreamsModel> _getMoreData(string cursor = null, string userLogin = null, string gameId = null)
        {
            var url = "https://api.twitch.tv/helix/streams";

            url = QueryHelpers.AddQueryString(url, "language", "en");

            if (cursor != null)
                url = QueryHelpers.AddQueryString(url, "after", cursor);

            if (gameId != null)
                url = QueryHelpers.AddQueryString(url, "game_id", gameId);

            if (userLogin != null)
                url = QueryHelpers.AddQueryString(url, "user_login", userLogin);

            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);
            message.Headers.Add("Authorization", $"Bearer { ConfigurationManager.AppSettings["api-oauth-token"] }");
            message.Headers.Add("client-id", ConfigurationManager.AppSettings["api-client-id"]);

            HttpResponseMessage results = await _client.SendAsync(message);
            string jsonString = await results.Content.ReadAsStringAsync();
            StreamsModel data = JsonConvert.DeserializeObject<StreamsModel>(jsonString);

            return data;
        }
    }
}
