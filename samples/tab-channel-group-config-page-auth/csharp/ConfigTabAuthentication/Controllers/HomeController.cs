using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TabAuthentication.Models;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

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
            ViewBag.ClientId = Configuration["ClientId"].ToString();
            return View();
        }

        [Route("SimpleConfigureTab")]
        public IActionResult SimpleConfigureTab()
        {
            ViewBag.ClientId = Configuration["ClientId"].ToString();
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
            string decode = "";
            string value = "";

            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtInput = validToken;

            //Check if readable token (string is in a JWT format)
            var readableToken = jwtHandler.CanReadToken(jwtInput);

            if (readableToken != true)
            {
                decode = "The token doesn't seem to be in a proper JWT format.";
            }

            if (readableToken == true)
            {
                var token = jwtHandler.ReadJwtToken(jwtInput);

                // Extract the payload of the JWT.
                var claims = token.Claims;
                var jwtPayload = "{";
                foreach (Claim c in claims)
                {
                    jwtPayload += '"' + c.Type + "\":\"" + c.Value + "\",";
                }
                jwtPayload += "}";
                decode += jwtPayload;

                value = JToken.Parse(jwtPayload).ToString(Formatting.Indented);
            }

            return value;
        }
    }
}
