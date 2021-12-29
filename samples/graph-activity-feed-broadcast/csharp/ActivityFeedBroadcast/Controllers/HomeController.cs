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

namespace ActivityFeedBroadcast.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ConcurrentDictionary<string, List<RequestInfo>> _taskList;

        public HomeController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ConcurrentDictionary<string, List<RequestInfo>> taskList)
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
            var currentTaskList = new List<RequestInfo>();
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

        // Send notification toorganisation.
        [HttpPost]
        [Route("SendNotificationToOrganisation")]
        public async Task<ActionResult> SendNotificationToManager(RequestInfo taskInfo)
        {
            try
            {
                var currentTaskList = new List<RequestInfo>();
                List<RequestInfo> taskList = new List<RequestInfo>();
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

                var graphClient = SimpleGraphClient.GetGraphClient(taskInfo.access_token);
                var graphClientApp = SimpleGraphClient.GetGraphClientforApp(_configuration["AzureAd:MicrosoftAppId"], _configuration["AzureAd:MicrosoftAppPassword"], _configuration["AzureAd:TenantId"]);
                var usersList = await graphClient.Users
                                 .Request()
                                    .GetAsync();

                var installedAppsForCurrentUser = await graphClient.Users[taskInfo.userId].Teamwork.InstalledApps
                                   .Request()
                                   .Expand("teamsAppDefinition")
                                   .GetAsync();
                var appId = installedAppsForCurrentUser.Where(id => id.TeamsAppDefinition.DisplayName == "Activity feed broadcast").Select(x => x.TeamsAppDefinition.TeamsAppId);

                foreach (var users in usersList)
                {
                    var installedApp = await graphClient.Users[users.Id].Teamwork.InstalledApps
                                   .Request()
                                   .Expand("teamsApp")
                                   .GetAsync();
                    var installationId = installedApp.Where(id => id.TeamsApp.DisplayName == "Activity feed broadcast").Select(x => x.TeamsApp.Id);
                    var appid = installationId.ToList();

                    if (appid.Count == 0)
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

                        var installedAppId = await graphClient.Users[users.Id].Teamwork.InstalledApps
                                   .Request()
                                   .Expand("teamsApp")
                                   .GetAsync();
                        var installationAppId = installedAppId.Where(id => id.TeamsApp.DisplayName == "Activity feed broadcast").Select(x => x.TeamsApp.Id);
                        var url = "https://teams.microsoft.com/l/entity/" + installationAppId.ToList()[0] + "/broadcast?context={\"subEntityId\":\"" + taskInfo.taskId + "\"}";
                        var topic = new TeamworkActivityTopic
                        {
                            Source = TeamworkActivityTopicSource.Text,
                            Value = $"{taskInfo.title}",
                            WebUrl = url
                        };

                        var previewText = new ItemBody
                        {
                            Content = $"Request By:"
                        };

                        var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
                        {
                            new Microsoft.Graph.KeyValuePair
                            {
                                Name = "approvalTaskId",
                                Value = taskInfo.title
                            }
                        };

                        await graphClientApp.Users[users.Id].Teamwork
                            .SendActivityNotification(topic, "approvalRequired", null, previewText, templateParameters)
                            .Request()
                            .PostAsync();

                        await Task.Delay(2000);
                    }
                    else
                    {
                        var url = "https://teams.microsoft.com/l/entity/" + installationId.ToList()[0] + "/broadcast?context={\"subEntityId\":\"" + taskInfo.taskId + "\"}";
                        var topic = new TeamworkActivityTopic
                        {
                            Source = TeamworkActivityTopicSource.Text,
                            Value = $"{taskInfo.title}",
                            WebUrl = url
                        };

                        var previewText = new ItemBody
                        {
                            Content = $"Request By:"
                        };

                        var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
                        {
                            new Microsoft.Graph.KeyValuePair
                            {
                                Name = "approvalTaskId",
                                Value = taskInfo.title
                            }
                        };

                        await graphClientApp.Users[users.Id].Teamwork
                            .SendActivityNotification(topic, "approvalRequired", null, previewText, templateParameters)
                            .Request()
                            .PostAsync();
                        await Task.Delay(2000);
                    } 
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