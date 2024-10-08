// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All Rights Reserved.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TabRequestApproval.Helpers;
using TabRequestApproval.Model;

namespace TabRequestApproval.Controllers
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

        // Get request page.
        [Route("request")]
        public ActionResult Request()
        {
            ViewBag.clientId = _configuration["AzureAd:MicrosoftAppId"];

            return View("Index");
        }

        // Get request list.
        [HttpGet]
        [Route("GetRequestList")]
        public async Task<List<RequestInfo>> GetRequestList()
        {
            var currentTaskList = new List<RequestInfo>();
            _taskList.TryGetValue("taskList", out currentTaskList);

            return currentTaskList;
        }

        // Get reuest details by Id.
        [HttpGet]
        [Route("RequestDetails")]
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
            return View("Request");
        }

        // Send notification to manager about request.
        [HttpPost]
        [Route("SendNotificationToManager")]
        public async Task<ActionResult> SendNotificationToManager(RequestInfo taskInfo)
        {
            try
            {
                // Check if the access token is not null
                if (taskInfo.access_token != null)
                {
                    // Create lists to store task information
                    var currentTaskList = new List<RequestInfo>();
                    List<RequestInfo> taskList = new List<RequestInfo>();

                    // Try to get the current task list from the dictionary
                    _taskList.TryGetValue("taskList", out currentTaskList);

                    // Generate a new task ID and set the initial status to "Pending"
                    taskInfo.taskId = Guid.NewGuid();
                    taskInfo.status = "Pending";

                    // Check if the currentTaskList is null
                    if (currentTaskList == null)
                    {
                        // If it is null, create a new task list and add the taskInfo to it
                        taskList.Add(taskInfo);
                        _taskList.AddOrUpdate("taskList", taskList, (key, newValue) => taskList);
                        ViewBag.TaskList = taskList;
                    }
                    else
                    {
                        // If it is not null, add the taskInfo to the existing currentTaskList
                        currentTaskList.Add(taskInfo);
                        _taskList.AddOrUpdate("taskList", currentTaskList, (key, newValue) => currentTaskList);
                        ViewBag.TaskList = currentTaskList;
                    }

                    // Get a Microsoft Graph API client using the provided access token
                    var graphClient = SimpleGraphClient.GetGraphClient(taskInfo.access_token);

                    // Retrieve user information from Microsoft Graph API
                    var user = await graphClient.Users[taskInfo.personaName].GetAsync();

                    // Retrieve installed apps for the user from Microsoft Graph API
                    var installedApps = await graphClient.Users[user.UserPrincipalName].Teamwork.InstalledApps
                                       .GetAsync((requestConfiguration) =>
                                       {
                                           requestConfiguration.QueryParameters.Expand = new string[] { "teamsAppDefinition" };
                                       });

                    // Filter installed apps to find the one with DisplayName "Tab Request Approval"
                    var installationId = installedApps.Value.Where(id => id.TeamsAppDefinition.DisplayName == "Tab Request Approval").Select(x => x.TeamsAppDefinition.TeamsAppId);

                    // Check if there is at least one matching installationId
                    if (installationId.Any())
                    {
                        var requestBody = new Microsoft.Graph.Users.Item.Teamwork.SendActivityNotification.SendActivityNotificationPostRequestBody
                        {
                            Topic = new TeamworkActivityTopic
                            {
                                Source = TeamworkActivityTopicSource.Text,
                                Value = $"{taskInfo.title}",
                                WebUrl = "https://teams.microsoft.com/l/entity/" + _configuration["AzureAd:MicrosoftAppId"] + "/request?context={\"subEntityId\":\"" + taskInfo.taskId + "\"}"
                            },
                            ActivityType = "approvalRequired",
                            PreviewText = new ItemBody
                            {
                                Content = $"Request By: {taskInfo.userName}"
                            },
                            TemplateParameters = new List<Microsoft.Graph.Models.KeyValuePair>
                            {
                                new Microsoft.Graph.Models.KeyValuePair
                                {
                                    Name = "approvalTaskId",
                                    Value = taskInfo.title
                                },
                            },
                        };

                        await graphClient.Users[user.Id].Teamwork.SendActivityNotification.PostAsync(requestBody);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions by logging to the console
                Console.WriteLine(ex);
            }


            return View("Index");
        }

        // Respond to user request.
        [HttpPost]
        [Route("RespondRequest")]
        public async Task<ActionResult> RespondRequest(RequestInfo taskInfo)
        {
            var currentTaskList = new List<RequestInfo>();
            _taskList.TryGetValue("taskList", out currentTaskList);

            var requestUpdate = currentTaskList.FirstOrDefault(p => p.taskId == taskInfo.taskId);
            requestUpdate.status = taskInfo.status;
            _taskList.AddOrUpdate("taskList", currentTaskList, (key, newValue) => currentTaskList);
            ViewBag.TaskList = currentTaskList;

            return View("Index");
        }

        // Get user access token.
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