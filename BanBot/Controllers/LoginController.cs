using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BanBot.Controllers
{
    public class LoginController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public LoginController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public void Index()
        {
            //redirect to twitch auth page
            Response.Redirect("https://id.twitch.tv/oauth2/authorize?client_id=p4y1qamoqkv2o2gm8fnw642yhfdec8&redirect_uri=https://localhost:44331&response_type=code&scope=moderator:manage:banned_users");
        }
    }
}
