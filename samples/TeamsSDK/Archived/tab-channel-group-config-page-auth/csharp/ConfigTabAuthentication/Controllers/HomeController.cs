using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using TabAuthentication.Models;

namespace TabAuthentication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IConfiguration Configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }


        [Route("SimpleSetUp")]
        public IActionResult SimpleSetUp()
        {
            return View();
        }

        [Route("ChooseAuth")]
        public IActionResult ChooseAuth()
        {
            return View();
        }

        [Route("SilentConfigureTab")]
        public IActionResult SilentConfigureTab()
        {
            ViewBag.ClientId = Configuration["ClientId"];
            ViewBag.TenantId = Configuration["TenantId"];
            return View();
        }

        [Route("SimpleConfigureTab")]
        public IActionResult SimpleConfigureTab()
        {
            ViewBag.ClientId = Configuration["ClientId"];
            return View();
        }   

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public string ValidateToken(string validToken)
        {
            string value = "";

            var jwtHandler = new JwtSecurityTokenHandler();

            //Check if readable token (string is in a JWT format)
            if (!jwtHandler.CanReadToken(validToken))
            {
                return "The token doesn't seem to be in a proper JWT format.";
            }

            var token = jwtHandler.ReadJwtToken(validToken);

            // Extract the payload of the JWT.
            var jwtPayload = "{";
            foreach (Claim c in token.Claims)
            {
                jwtPayload += '"' + c.Type + "\":\"" + c.Value + "\",";
            }
            jwtPayload += "}";

            value = JsonSerializer.Serialize(
                JsonDocument.Parse(jwtPayload, new JsonDocumentOptions { AllowTrailingCommas = true }).RootElement,
                new JsonSerializerOptions { WriteIndented = true });

            return value;
        }
    }
}
