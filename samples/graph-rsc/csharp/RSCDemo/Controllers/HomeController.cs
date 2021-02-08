using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using RSCWithGraphAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RSCWithGraphAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// RSC Setup
        /// </summary>
        [Route("Demo")]
        public async Task<ActionResult> Demo(string tenantId, string groupId)
        {
            GraphServiceClient graphClient = await GetAuthenticatedClient(tenantId);
            var viewModel = new DemoViewModel()
            {
                Channels = await GetChannelsList(graphClient, tenantId, groupId),
                Permissions = await GetPermissionGrants(graphClient, tenantId, groupId)
            };
            return View(viewModel);
        }

        /// <summary>
        /// Configure Tab
        /// </summary>
        [Route("ConfigureTab")]
        public IActionResult ConfigureTab()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        private async Task<List<string>> GetChannelsList(GraphServiceClient graphClient, string tenantId, string groupId)
        {
            var result = await graphClient.Teams[groupId].Channels.Request()
                .GetAsync();

            return result.Select(r => r.DisplayName).ToList();
        }

        private async Task<List<string>> GetPermissionGrants(GraphServiceClient graphClient, string tenantId, string groupId)
        {
            var result = await graphClient.Groups[groupId].PermissionGrants.Request()
                .GetAsync();

            return result.Select(r => r.Permission).ToList();
        }


        /// <summary>
        ///Get Authenticated Client
        /// </summary>
        private async Task<GraphServiceClient> GetAuthenticatedClient(string tenantId)
        {
            var accessToken = await GetToken(tenantId);
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));

            return graphClient;
        }

        /// <summary>
        /// Get Token for given tenant.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> GetToken(string tenantId)
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_configuration["ClientId"])
                                                  .WithClientSecret(_configuration["ClientSecret"])
                                                  .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                                                  .WithRedirectUri("https://daemon")
                                                  .Build();

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

    }
}
