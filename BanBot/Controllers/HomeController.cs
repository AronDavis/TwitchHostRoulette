using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Threading;
using TwitchApi;
using TwitchApi.ResponseModels.Auth;

namespace BanBot.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Index([FromQuery(Name = "code")] string authorizationCode)
        {
            string clientSecret = ConfigurationManager.AppSettings["clientSecret"];
            TwitchApiClient twitchApiClient = new TwitchApiClient();
            UserAccessTokenModel userAccessToken = twitchApiClient.GetUserAccessToken(
                clientId: "p4y1qamoqkv2o2gm8fnw642yhfdec8",
                clientSecret: clientSecret,
                authorizationCode: authorizationCode,
                redirectUri: "https://localhost:44331"
                ).Result;

            Interlocked.Exchange(ref Program.UserAccessToken, userAccessToken);

            return userAccessToken.UserAccessToken;
        }
    }
}
