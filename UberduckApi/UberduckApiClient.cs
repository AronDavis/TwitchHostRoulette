using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UberduckApi.ResponseModels;

namespace UberduckApi
{
    /// <summary>
    /// https://app.uberduck.ai/docs/api
    /// </summary>
    public class UberduckApiClient
    {
        private HttpClient _client = new HttpClient();

        private string _base64Auth;

        /// <summary>
        /// Auth needs to be set before API calls will work.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="apiSecret"></param>
        public void SetAuth(string apiKey, string apiSecret)
        {
            _base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiKey}:{apiSecret}"));
        }

        public async Task<SpeakResponse> Speak(string speech, string voice)
        {
            if(_base64Auth == null)
            {
                throw new Exception("Auth must be set first.");
            }

            string url = "https://api.uberduck.ai/speak";

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Add("Authorization", $"Basic {_base64Auth}");

                var body = new {
                    speech = speech,
                    voice = voice
                };

                request.Content = new StringContent(JsonConvert.SerializeObject(body)); //, System.Text.Encoding.UTF8, "application/json"

                HttpResponseMessage results = await _client.SendAsync(request);
                string jsonString = await results.Content.ReadAsStringAsync();
                SpeakResponse data = JsonConvert.DeserializeObject<SpeakResponse>(jsonString);
                return data;
            }
        }

        public async Task<SpeakStatusResponse> SpeakStatus(Guid uuid)
        {
            string url = "https://api.uberduck.ai/speak-status";

            url = QueryHelpers.AddQueryString(url, "uuid", uuid.ToString());

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Add("Authorization", $"Basic {_base64Auth}");

                HttpResponseMessage results = await _client.SendAsync(request);

                string jsonString = await results.Content.ReadAsStringAsync();
                SpeakStatusResponse data = JsonConvert.DeserializeObject<SpeakStatusResponse>(jsonString);
                return data;
            }
        }
    }
}
