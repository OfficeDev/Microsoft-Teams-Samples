using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TabActivityFeed.Helpers;
using TabActivityFeed.Model;
using TabActivityFeed.Repository;

namespace TabActivityFeed.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        [Route("request")]
        public ActionResult Hello()
        {
            return View("Index");
        }

        [HttpPost]
        [Route("SendNotificationToUser")]
        public async Task<ActionResult> SendNotificationToUser(TaskInfo taskInfo)
        {
            TaskHelper.AddTaskToFeed(taskInfo);
            var graphClient = SimpleGraphClient.GetGraphClient(taskInfo.access_token);
            var graphClientApp = SimpleGraphClient.GetGraphClientforApp(_configuration["AzureAd:MicrosoftAppId"], _configuration["AzureAd:MicrosoftAppPassword"], _configuration["AzureAd:TenantId"]);
            var user = await graphClient.Users[taskInfo.userName]
                      .Request()
                      .GetAsync();
            var installedApps = await graphClient.Users[user.Id].Teamwork.InstalledApps
                               .Request()
                               .Expand("teamsAppDefinition")
                               .GetAsync();
            var installationId = installedApps.Where(id => id.TeamsAppDefinition.DisplayName == "Tab Approval").Select(x => x.Id);
            var userName = user.UserPrincipalName;

                ViewBag.taskID = new Guid();
                var topic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.EntityUrl,
                    Value = "https://graph.microsoft.com/beta/users/" + user.Id + "/teamwork/installedApps/" + installationId.ToList()[0]
                };

            var activityType = "approvalRequired";

            var previewText = new ItemBody
            {
                Content = "Deployment requires your approval"
            };
            var customRecipient = new AadUserNotificationRecipient
            {
                UserId = user.Id
            };
            var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
                 {
                 new Microsoft.Graph.KeyValuePair
                 {
                   Name = "approvalTaskId",
                   Value ="2020AAGGTAPP"
                  }
                };
            try
                {
                    await graphClientApp.Users[user.Id].Teamwork
                        .SendActivityNotification(topic, activityType, null, previewText, templateParameters)
                        .Request()
                        .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

            return View("Index");
        }

        [Authorize]
        [HttpGet("/GetUserAccessToken")]
        public async Task<ActionResult<string>> GetUserAccessToken()
        {
            try
            {
                var accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(_configuration, _httpClientFactory, _httpContextAccessor);
                return accessToken;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}