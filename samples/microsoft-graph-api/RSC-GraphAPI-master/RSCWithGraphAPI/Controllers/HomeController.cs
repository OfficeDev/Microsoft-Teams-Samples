using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RSCWithGraphAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;

namespace RSCWithGraphAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger,IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Route("ConfigureTab")]
        public IActionResult ConfigureTab()
        {
            return View();
        }

        [Route("AuthStart")]
        public ActionResult AuthStart()
        {
            ViewBag.ClientId = _configuration["ClientId"].ToString();
            ViewBag.ResponseType = _configuration["ResponseType"].ToString();
            ViewBag.ResponseMode = _configuration["ResponseMode"].ToString();
            ViewBag.Resource = _configuration["Resource"].ToString();
            return View();
        }

        [Route("AuthEnd")]
        public ActionResult AuthEnd()
        {
            return View();
        }

        [Route("Setup")]
        public ActionResult Setup()
        {
            return View();
        }

        [HttpGet]
        public async Task<List<string>> GetToken()
        {
            List<string> result = new List<string>();
            string tenantId = _configuration["TenantId"].ToString();

            var url = $"https://login.microsoftonline.com/" + tenantId + "/oauth2/v2.0/token";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("content-type", "application/x-www-form-urlencoded");

            var values = new Dictionary<string, string>
                {
                    { "client_id",_configuration["ClientId"].ToString()},
                    { "client_secret",_configuration["ClientSecret"].ToString()},
                    { "grant_type", "client_credentials"},
                    { "scope", "https://graph.microsoft.com/.default"},
                };

            var formContent = new FormUrlEncodedContent(values);
            var response = await httpClient.PostAsync(url, formContent);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJsonObject = JsonConvert.DeserializeObject(responseString);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.Add((string)responseJsonObject.access_token);
                result.Add(_configuration["TeamId"].ToString());

                return result;
            }
            else
            {
                return null;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
