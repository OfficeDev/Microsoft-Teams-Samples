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
using Polly;
using System.Net;
using Newtonsoft.Json;
using System.Text;

namespace ActivityFeedBroadcast.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ConcurrentDictionary<string, List<BroadcastInfo>> _taskList;

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
                var appId = installedAppsForCurrentUser.Where(id => id.TeamsAppDefinition.DisplayName == "Activity feed broadcast").Select(x => x.TeamsAppDefinition.TeamsAppId);

                Parallel.ForEach (usersList, async users =>
                {
                    var installedApp = await graphClient.Users[users.Id].Teamwork.InstalledApps
                                             .Request()
                                             .Expand("teamsApp")
                                             .GetAsync();

                    var response = new HttpResponseMessage();
                    var installationId = installedApp.Where(id => id.TeamsApp.DisplayName == "Activity feed broadcast").Select(x => x.TeamsApp.Id);
                    var client = new HttpClient();
                    var url = "https://teams.microsoft.com/l/entity/" + appId.ToList()[0] + "/broadcast?context={\"subEntityId\":\"" + taskInfo.taskId + "\"}";
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
                        }
                    };

                    var data = new StringContent(JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");

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

                        client.DefaultRequestHeaders.Authorization =
                           new AuthenticationHeaderValue("Bearer", taskInfo.access_token);
                        await client.PostAsync($"https://graph.microsoft.com/v1.0/users/{users.Id}/teamwork/sendActivityNotification", data);
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", taskInfo.access_token);
                        response = await client.PostAsync($"https://graph.microsoft.com/v1.0/users/{users.Id}/teamwork/sendActivityNotification", data);
                    }

                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        await Policy
                        .Handle<HttpRequestException>()
                        .OrResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.TooManyRequests ||
                                                            response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        .WaitAndRetryAsync(3,
                            sleepDurationProvider: (retryCount, response, context) =>
                            {
                                var delay = TimeSpan.FromSeconds(0);
                                // if an exception was thrown, this will be null
                                if (response.Result != null)
                                {
                                    if (!response.Result.Headers.TryGetValues("Retry-After", out IEnumerable<string> values))
                                        return delay;

                                    if (int.TryParse(values.First(), out int delayInSeconds))
                                        delay = TimeSpan.FromSeconds(delayInSeconds);
                                }
                                else
                                {
                                    var exponentialBackoff = Math.Pow(2, retryCount);
                                    var delayInSeconds = exponentialBackoff * 10000;
                                    delay = TimeSpan.FromMilliseconds(delayInSeconds);
                                }

                                return delay;
                            },
                            onRetryAsync: async (response, timespan, retryCount, context) =>
                            {
                            }
                          ).ExecuteAsync(async () =>
                          {
                              return await client.PostAsync($"https://graph.microsoft.com/v1.0/users/{users.Id}/teamwork/sendActivityNotification", data);
                          }
                        );
                    }
                });       
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