using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ProactiveBot.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private ProactiveBot.Bots.ProactiveHelper Helper = new ProactiveBot.Bots.ProactiveHelper();
        private const string WelcomeMessage = "Welcome to the Proactive Bot sample.";

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Route("ConfigureTab")]
        public IActionResult ConfigureTab()
        {
            return View();
        }

        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index(string TenantId, string TeamId, string chatId)
        {
            if (chatId != null)
            {
                await Helper.AppInstallationforChat(chatId, TenantId, _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TeamAppCatalogid"]);
            }
            else
            {
                await Helper.AppInstallationforChannel(TeamId, TenantId, _configuration["MicrosoftAppId"], _configuration["MicrosoftAppPassword"], _configuration["TeamAppCatalogid"]);
            }
            return Content(WelcomeMessage);
        }
    }
}