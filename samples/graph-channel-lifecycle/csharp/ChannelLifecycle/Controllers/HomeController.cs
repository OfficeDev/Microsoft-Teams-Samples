using ChannelLifecycle.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;
using ChannelLifecycle;

namespace ChannelLifecycle.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Index
        /// </summary>
        /// <returns></returns>
        [Route("Index")]
        public async Task<IActionResult> Index(string tenantId, string groupId, string channelId)
        {
            var result = await GetChannelsList(tenantId, groupId);

            ChannelViewModel viewModel = new ChannelViewModel();
            List<ChannelModel> list = new List<ChannelModel>();
            foreach (var res in result)
            {
                var channelModel = new ChannelModel();
                channelModel.ChannelId = res.Id;
                channelModel.ChannelName = res.DisplayName;
                channelModel.ChannelDesc = res.Description;
                list.Add(channelModel);
            }

            viewModel.Members = await GetMembers(tenantId, groupId);
            viewModel.TenantId = tenantId;
            viewModel.GroupId = groupId;
            viewModel.channelList = list;

            return View(viewModel);
        }

        [HttpGet]
        [Route("CreateTab")]
        public async Task CreateTab(string tenantId, string teamId, string channelId)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);

            var appsInstalled = await graphClient.Teams[teamId].InstalledApps.Request().Expand("teamsAppDefinition").GetAsync();
            var teamsAppId = appsInstalled.Where(o => o.TeamsAppDefinition.DisplayName == "Channel Lifecycle").Select(o => o.TeamsAppDefinition.TeamsAppId).FirstOrDefault();

            var teamsTab = new TeamsTab
            {
                DisplayName = "My Tab",
                Configuration = new TeamsTabConfiguration
                {
                    EntityId = "2DCA2E6C7A10415CAF6B8AB66123125",
                    ContentUrl = _configuration["BaseUri"] + "/TestTab",
                    WebsiteUrl = _configuration["BaseUri"] + "/TestTab"
                },
                AdditionalData = new Dictionary<string, object>()
                        {
                            {"teamsApp@odata.bind", "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/"+teamsAppId }
                        }
            };

            var result = await graphClient.Teams[teamId].Channels[channelId].Tabs
                  .Request()
                  .AddAsync(teamsTab);

        }

        /// <summary>
        /// Get Channels List
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetChannelsList")]
        public async Task<List<Channel>> GetChannelsList(string tenantId, string teamId)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);

            var result = await graphClient.Teams[teamId].Channels.Request()
                .GetAsync();

            return result.ToList();
        }

        [HttpGet]
        [Route("GetMembers")]
        public async Task<List<string>> GetMembers(string tenantId, string teamId)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);

            var members = await graphClient.Teams[teamId].Members
                 .Request()
                 .GetAsync();

            List<string> membersList = new List<string>();
            foreach (var member in members)
            {
                membersList.Add(member.DisplayName);
            }

            return membersList;
        }

        [HttpGet]
        [Route("GeteChannel")]
        public async Task<List<string>> GetChannel(string tenantId, string teamId, string channelId)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);

            var channel = await graphClient.Teams[teamId].Channels[channelId]
                .Request()
                .GetAsync();

            List<string> channelData = new List<string>();
            channelData.Add("Channel Name: " + channel.DisplayName);
            channelData.Add("Channel Desc: " + channel.Description);

            return channelData;
        }

        [HttpGet]
        [Route("CreateChannel")]
        public async Task<string> CreateChannel(string tenantId, string teamId, string channelName, string channelDesc)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);

            var channel = new Channel
            {
                DisplayName = channelName,
                Description = channelDesc,
                MembershipType = ChannelMembershipType.Standard
            };

            var result = await graphClient.Teams[teamId].Channels
                  .Request()
                  .AddAsync(channel);

            return "Channel created";
        }

        [HttpGet]
        [Route("UpdateChannel")]
        public async Task<string> UpdateChannel(string tenantId, string teamId, string channelId, string channelName, string channelDesc)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);

            var channel = new Channel
            {
                DisplayName = channelName,
                Description = channelDesc,
            };

            await graphClient.Teams[teamId].Channels[channelId]
                    .Request()
                    .UpdateAsync(channel);

            return "Channel updated";
        }

        [HttpGet]
        [Route("DeleteChannel")]
        public async Task<string> DeleteChannel(string tenantId, string teamId, string channelId)
        {
            string token = await GetToken(tenantId);
            GraphServiceClient graphClient = GetAuthenticatedClient(token);

            await graphClient.Teams[teamId].Channels[channelId]
                .Request()
                .DeleteAsync();

            return "Successfully deleted";
        }

        /// <summary>
        /// Configure Tab
        /// </summary>
        /// <returns></returns>
        [Route("ConfigureTab")]
        public IActionResult ConfigureTab()
        {
            return View();
        }

        [Route("TestTab")]
        public IActionResult TestTab()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        ///Get Authenticated Client
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public GraphServiceClient GetAuthenticatedClient(string token)
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

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
        [Route("GetToken")]
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

