// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ActivityFeedBroadcast.Helpers;
using ActivityFeedBroadcast.Model;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using TabActivityFeed.Helpers;

namespace ActivityFeedBroadcast.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ConcurrentDictionary<string, List<BroadcastInfo>> _taskList;

        const int recipientPartitionSize = 85;

        const int partitionCount = 5;

        public HomeController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ConcurrentDictionary<string, List<BroadcastInfo>> taskList)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _taskList = taskList;
        }

        // Get broadcast page.
        [Route("broadcast")]
        public ActionResult Broadcast()
        {
            return View("Index");
        }

        // Get broadcast message Id.
        [HttpGet]
        [Route("MessageDetails")]
        public ActionResult GetRequestByID(string taskId)
        {
            var currentTaskList = new List<BroadcastInfo>();
            _taskList.TryGetValue("taskList", out currentTaskList);

            if (currentTaskList == null)
            {
                ViewBag.Message = "No record found";
            }
            else
            {
                var request = currentTaskList.FirstOrDefault(p => p.taskId.ToString() == taskId);
                ViewBag.TaskDetails = request;
            }
            return View("Message");
        }

        // Send notification to organisation.
        [HttpPost]
        [Route("SendNotificationToOrganisation")]
        public async Task<ActionResult> SendNotificationToOrganisation(BroadcastInfo taskInfo)
        {
            try
            {
                var currentTaskList = new List<BroadcastInfo>();
                List<BroadcastInfo> taskList = new List<BroadcastInfo>();
                _taskList.TryGetValue("taskList", out currentTaskList);

                taskInfo.taskId = Guid.NewGuid();

                if (currentTaskList == null)
                {
                    taskList.Add(taskInfo);
                    _taskList.AddOrUpdate("taskList", taskList, (key, newValue) => taskList);
                    ViewBag.TaskList = taskList;
                }
                else
                {
                    currentTaskList.Add(taskInfo);
                    _taskList.AddOrUpdate("taskList", currentTaskList, (key, newValue) => currentTaskList);
                    ViewBag.TaskList = currentTaskList;
                }

                // Graph client for user.
                var graphClient = SimpleGraphClient.GetGraphClient(taskInfo.access_token);

                // Graph client for app.
                var graphClientApp = SimpleGraphClient.GetGraphClientforApp(_configuration["AzureAd:MicrosoftAppId"], _configuration["AzureAd:MicrosoftAppPassword"], _configuration["AzureAd:TenantId"]);
                var usersList = await graphClient.Users
                                 .Request()
                                    .GetAsync();

                var installedAppsForCurrentUser = await graphClient.Users[taskInfo.userId].Teamwork.InstalledApps
                                   .Request()
                                   .Expand("teamsAppDefinition")
                                   .GetAsync();

                // Get app id using app display name.
                var appId = installedAppsForCurrentUser.Where(id => id.TeamsAppDefinition.DisplayName == _configuration["AppName"]).Select(x => x.TeamsAppDefinition.TeamsAppId);
                var count = usersList.Count;
                var counter = 0;
                var recipientsList = new List<Dictionary<string, string>>();

                await UtilityHelper.ForEachAsync(usersList, partitionCount, async users => {
                    try
                    {
                        var installedApp = await graphClient.Users[users.Id].Teamwork.InstalledApps
                                             .Request()
                                             .Expand("teamsApp")
                                             .GetAsync();

                        var response = new HttpResponseMessage();
                        var installationId = installedApp.Where(id => id.TeamsApp.DisplayName == _configuration["AppName"]).Select(x => x.TeamsApp.Id);

                        if (installationId.ToList().Count == 0)
                        {
                            var userScopeTeamsAppInstallation = new UserScopeTeamsAppInstallation
                            {
                                AdditionalData = new Dictionary<string, object>()
                            {
                                {"teamsApp@odata.bind", "https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/"+appId.ToList()[0]}
                            }
                            };

                            await graphClient.Users[users.Id].Teamwork.InstalledApps
                                                .Request()
                                                .AddAsync(userScopeTeamsAppInstallation);
                        }
                        counter++;
                        recipientsList.Add(new Dictionary<string, string>
                        {
                            {"@odata.type", "microsoft.graph.aadUserNotificationRecipient"},
                            {"userId", users.Id}
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Installation failed for " + users.DisplayName);
                    }
                });

                var url = "https://teams.microsoft.com/l/entity/" + appId.ToList()[0] + "/broadcast?context={\"subEntityId\":\"" + taskInfo.taskId + "\"}";
                var client = new HttpClient();

                var recipientsChunks = UtilityHelper.SplitIntoChunks(recipientsList, recipientPartitionSize);

                foreach (var recipientList in recipientsChunks)
                {
                    var postData = new
                    {
                        topic = new TeamworkActivityTopic()
                        {
                            Source = TeamworkActivityTopicSource.Text,
                            Value = $"{taskInfo.title}",
                            WebUrl = url
                        },
                        activityType = "approvalRequired",
                        previewText = new ItemBody
                        {
                            Content = $"Message By:"
                        },
                        templateParameters = new List<Microsoft.Graph.KeyValuePair>()
                            {
                                new Microsoft.Graph.KeyValuePair
                                {
                                    Name = "approvalTaskId",
                                    Value = taskInfo.title
                                }
                            },
                        recipients = recipientList
                    };

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", taskInfo.access_token);
                    var data = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");

                    // Sending acitivity feed notification in bulk
                    await client.PostAsync($"https://graph.microsoft.com/beta/teamwork/sendActivityNotificationToRecipients", data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return View("Index");
        }

        // Get user access token.
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