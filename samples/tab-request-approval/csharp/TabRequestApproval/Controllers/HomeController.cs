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

        // Redirect to auth page.
        [Route("TabAuth")]
        public ActionResult Auth()
        {
            return View("TabAuth");
        }

        // Send notification to manager about request.
        [HttpPost]
        [Route("SendNotificationToManager")]
        public async Task<ActionResult> SendNotificationToManager(RequestInfo taskInfo)
        {
            try
            {
                var currentTaskList = new List<RequestInfo>();
                List<RequestInfo> taskList = new List<RequestInfo>();
                _taskList.TryGetValue("taskList", out currentTaskList);

                taskInfo.taskId = Guid.NewGuid();
                taskInfo.status = "Pending";

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
                var user = await graphClient.Users[taskInfo.personaName]
                          .Request()
                          .GetAsync();

                var installedApps = await graphClient.Users[user.Id].Teamwork.InstalledApps
                                   .Request()
                                   .Expand("teamsApp")
                                   .GetAsync();

                var installationId = installedApps.Where(id => id.TeamsApp.DisplayName == "Tab Request Approval").Select(x => x.TeamsApp.Id);
                var userName = user.UserPrincipalName;
   
                var url = "https://teams.microsoft.com/l/entity/"+installationId.ToList()[0]+"/request?context={\"subEntityId\":\""+ taskInfo.taskId+"\"}";
                var topic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.Text,
                    Value = $"{taskInfo.title}",
                    WebUrl = url
                };

                var previewText = new ItemBody
                {
                    Content = $"Request By: {taskInfo.userName}"
                };

                var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
                {
                    new Microsoft.Graph.KeyValuePair
                    {
                        Name = "approvalTaskId",
                        Value = taskInfo.title
                    }
                };
            
                await graphClient.Users[user.Id].Teamwork
                    .SendActivityNotification(topic, "approvalRequired", null, previewText, templateParameters)
                    .Request()
                    .PostAsync();
            }
            catch (Exception ex)
            {
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