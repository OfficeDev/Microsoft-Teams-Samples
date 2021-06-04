using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ProactiveBot.Models;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Builder;
using System.Threading;
using Microsoft.Bot.Schema.Teams;
using System.Net;
using Attachment = Microsoft.Bot.Schema.Attachment;
using System.IO;
using Newtonsoft.Json;

namespace ProactiveBot.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        ProactiveBot.Bots.ProactiveHelper Helper = new ProactiveBot.Bots.ProactiveHelper();
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
