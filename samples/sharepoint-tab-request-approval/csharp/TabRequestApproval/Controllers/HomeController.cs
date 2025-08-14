// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace TabRequestApproval.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Graph;
    using TabActivityFeed.Helpers;
    using TabActivityFeed.Models;
    using TabActivityFeed.Providers;
    using TabRequestApproval.Helpers;
    using TabRequestApproval.Model;
    using User = Microsoft.Graph.User;

    /// <summary>
    /// Sample app home controller.
    /// This is the main controller that responds to the interactions made on the client-side.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Represents the appsettings.json file details.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Represents the HTTP client factory.
        /// </summary>
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Represents the HTTP context accessor.
        /// </summary>
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Represents the request provider.
        /// </summary>
        private readonly IRequestProvider requestProvider;

        /// <summary>
        /// Represents the auth provider.
        /// </summary>
        private readonly IAuthProvider authProvider;

        /// <summary>
        /// Represents the container provider.
        /// </summary>
        private readonly IContainerProvider containerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configuration">Represents the appsettings.json file details.</param>
        /// <param name="httpClientFactory">Represents the HTTP client factory.</param>
        /// <param name="httpContextAccessor">Represents the HTTP context accessor.</param>
        /// <param name="authProvider">Represents the auth provider.</param>
        /// <param name="requestProvider">Represents the request provider (managing tasks).</param>
        /// <param name="containerProvider">Represents the storage container provider.</param>
        public HomeController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IAuthProvider authProvider,
            IRequestProvider requestProvider,
            IContainerProvider containerProvider)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            this.authProvider = authProvider ?? throw new ArgumentNullException(nameof(authProvider));
            this.requestProvider = requestProvider ?? throw new ArgumentNullException(nameof(requestProvider));
            this.containerProvider = containerProvider ?? throw new ArgumentNullException(nameof(containerProvider));
        }

        /// <summary>
        /// Retrieves the 'request' page in the views.
        /// </summary>
        /// <returns>The 'request' page. </returns>
        [Route("request")]
        public ActionResult Request()
        {
            this.ViewBag.clientId = this.configuration["AzureAd:MicrosoftAppId"];

            return this.View("Index");
        }

        /// <summary>
        /// Retrieves the 'TabAuth' page in the views.
        /// </summary>
        /// <returns>The 'TabAuth' page.</returns>
        [Route("TabAuth")]
        public ActionResult Auth()
        {
            return this.View("TabAuth");
        }

        /// <summary>
        /// Retrieves the 'config' page in the views.
        /// </summary>
        /// <returns>The 'config' page.</returns>
        [Route("config")]
        public ActionResult Config()
        {
            return this.View("config");
        }

        /// <summary>
        /// Retrieves the list of requests.
        /// </summary>
        /// <param name="channelId">Represents userId. </param>
        /// <param name="groupId">Represents the group id.</param>
        /// <param name="chatId">Represents the chat id.</param>
        /// <param name="tenantId">Represents the tenant id.</param>
        /// <param name="userId">Represents the user id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        [Route("GetRequestList")]
        public async Task<List<RequestInfo>> GetRequestListAsync(string channelId, string groupId, string chatId, string tenantId, string userId)
        {
            try
            {
                string teamsAppInstallationScopeId = TeamsAppInstallationScopeUtils.GetTeamsAppInstallationScopeId(chatId, channelId, groupId, tenantId, userId);

                // Retrieve the list of all requestInfo objects.
                IEnumerable<RequestInfo> requests = await this.requestProvider.GetRequestsAsync(teamsAppInstallationScopeId).ConfigureAwait(false);

                List<RequestInfo> requestsList = requests.ToList();

                return requestsList;
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error. Details: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the specified request.
        /// </summary>
        /// <param name="subEntityId">Represents the teams app installation scope and task id (request id).</param>
        /// <returns>The specified request.</returns>
        [HttpGet]
        [Route("RequestDetails")]
        public async Task<ActionResult> GetRequestByIdAsync(string subEntityId)
        {
            int lastIndex = subEntityId.LastIndexOf('$');

            // Split the input into two parts using the index
            string taskId = subEntityId.Substring(lastIndex + 1);
            string teamsAppInstallationScopeId = subEntityId.Substring(0, lastIndex);

            // Retrieve requestInfo object.
            RequestInfo requestInfo = await this.requestProvider.GetRequestAsync(taskId, teamsAppInstallationScopeId).ConfigureAwait(false);

            if (requestInfo == null)
            {
                this.ViewBag.Message = "No record found";
            }
            else
            {
                this.ViewBag.TaskDetails = requestInfo;
            }

            return this.View("Request");
        }

        /// <summary>
        /// Sends a request to the manager.
        /// </summary>
        /// <param name="requestInfo">Represents the request information that is being sent to the manager.</param>
        /// <param name="channelId">Represents userId. </param>
        /// <param name="groupId">Represents the group id.</param>
        /// <param name="chatId">Represents the chat id.</param>
        /// <param name="tenantId">Represents the tenant id.</param>
        /// <param name="userId">Represents the user id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Route("SendNotificationToManager")]
        public async Task<ActionResult> SendNotificationToManagerAsync(RequestInfo requestInfo, string channelId, string groupId, string chatId, string tenantId, string userId)
        {
            try
            {
                string teamsAppInstallationScopeId = TeamsAppInstallationScopeUtils.GetTeamsAppInstallationScopeId(chatId, channelId, groupId, tenantId, userId);

                // Update the requestInfo object to reflect the status change.
                requestInfo.taskId = Guid.NewGuid().ToString();
                requestInfo.teamsAppInstallationScopeId = teamsAppInstallationScopeId;
                requestInfo.status = "Pending";

                // Create the requestInfo object.
                await this.requestProvider.CreateRequestAsync(requestInfo, teamsAppInstallationScopeId).ConfigureAwait(false);

                // Retrieve the list of requestInfo objects to display.
                IEnumerable<RequestInfo> requests = await this.requestProvider.GetRequestsAsync(teamsAppInstallationScopeId).ConfigureAwait(false);

                List<RequestInfo> requestList = requests.ToList();

                // Provide list details to client.
                this.ViewBag.TaskList = requestList;

                // Retrieve access token for activity notification management tasks.
                string accessToken = await this.authProvider.GetGraphAccessTokenAsync().ConfigureAwait(false);

                GraphServiceClient graphClient = ServiceClients.GetGraphServiceClient(accessToken);

                // Retrieve user that created the request.
                try
                {
                    User user = await graphClient.Users[requestInfo.personaName]
                          .Request()
                          .GetAsync().ConfigureAwait(false);

                    // Assemble information required for activity notification.
                    var previewText = new ItemBody
                    {
                        Content = $"Request By: {requestInfo.userName}",
                    };

                    var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
                    {
                        new Microsoft.Graph.KeyValuePair
                        {
                            Name = "approvalTaskId",
                            Value = requestInfo.title,
                        },
                    };

                    var recipient = new AadUserNotificationRecipient
                    {
                        ODataType = "microsoft.graph.aadUserNotificationRecipient",
                        UserId = user.Id,
                    };

                    string userName = user.UserPrincipalName;

                    TeamsAppInstallationScope teamsAppInstallationScope = TeamsAppInstallationScopeUtils.GetTeamsAppInstallationScope(chatId, channelId, groupId, userId);

                    if (teamsAppInstallationScope == TeamsAppInstallationScope.ChatScope)
                    {
                        IChatInstalledAppsCollectionPage installedApps = await graphClient.Chats[chatId].InstalledApps.Request().Expand("teamsApp").GetAsync().ConfigureAwait(false);

                        IEnumerable<string> installationIds = installedApps.Where(app => app.TeamsApp.ExternalId == this.configuration["AzureAd:MicrosoftAppId"]).Select(x => x.TeamsApp.Id);

                        TeamworkActivityTopic topic = this.CreateTeamworkActivtyTopic(installationIds, requestInfo.taskId, requestInfo.title, requestInfo.teamsAppInstallationScopeId);

                        // Send activity notification for the member residing within the chat.
                        await graphClient.Chats[chatId]
                            .SendActivityNotification(topic, "approvalRequired", null, previewText, templateParameters, recipient)
                            .Request()
                            .PostAsync().ConfigureAwait(false);
                    }
                    else if (teamsAppInstallationScope == TeamsAppInstallationScope.TeamScope)
                    {
                        ITeamInstalledAppsCollectionPage installedApps = await graphClient.Teams[groupId].InstalledApps.Request().Expand("teamsApp").GetAsync().ConfigureAwait(false);

                        IEnumerable<string> installationIds = installedApps.Where(app => app.TeamsApp.ExternalId == this.configuration["AzureAd:MicrosoftAppId"]).Select(x => x.TeamsApp.Id);

                        TeamworkActivityTopic topic = this.CreateTeamworkActivtyTopic(installationIds, requestInfo.taskId, requestInfo.title, requestInfo.teamsAppInstallationScopeId);

                        // Send activity notification for the member residing within the team.
                        await graphClient.Teams[groupId]
                            .SendActivityNotification(topic, "approvalRequired", null, previewText, templateParameters, recipient)
                            .Request()
                            .PostAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        IUserTeamworkInstalledAppsCollectionPage installedApps = await graphClient.Users[userId].Teamwork.InstalledApps.Request().Expand("teamsApp").GetAsync().ConfigureAwait(false);

                        IEnumerable<string> installationIds = installedApps.Where(app => app.TeamsApp.ExternalId == this.configuration["AzureAd:MicrosoftAppId"]).Select(x => x.TeamsApp.Id);

                        TeamworkActivityTopic topic = this.CreateTeamworkActivtyTopic(installationIds, requestInfo.taskId, requestInfo.title, requestInfo.teamsAppInstallationScopeId);

                        await graphClient.Users[user.Id].Teamwork
                            .SendActivityNotification(topic, "approvalRequired", null, previewText, templateParameters)
                            .Request()
                            .PostAsync()
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    return this.View("Index");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to send notification. Reason: {ex.Message}");
            }

            return this.View("Index");
        }

        /// <summary>
        /// Responds to the requests made.
        /// Triggered when a user responds to an request and updates its status by approving or rejecting it.
        /// </summary>
        /// <param name="requestInfo">Represents the information about the request.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Route("RespondRequest")]
        public async Task<ActionResult> RespondRequestAsync(RequestInfo requestInfo)
        {
            // Retrieve requestInfo object. The only information being passed from the client is the taskId in the requestInfo object.
            RequestInfo requestInfoToUpdate = await this.requestProvider.GetRequestAsync(requestInfo.taskId, requestInfo.teamsAppInstallationScopeId).ConfigureAwait(false);

            // Update the status to indicate if it has been approved or rejected.
            requestInfoToUpdate.status = requestInfo.status;

            // Update the requestInfo object.
            await this.requestProvider.UpdateRequestAsync(requestInfo.taskId, requestInfoToUpdate, requestInfo.teamsAppInstallationScopeId).ConfigureAwait(false);

            // Retrieve all requestInfo objects.
            IEnumerable<RequestInfo> requests = await this.requestProvider.GetRequestsAsync(requestInfo.teamsAppInstallationScopeId).ConfigureAwait(false);

            // Provide list details to client.
            this.ViewBag.TaskList = requests.ToList();

            return this.View("Index");
        }

        /// <summary>
        /// Retrieves user access token.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet("/GetUserAccessToken")]
        public async Task<ActionResult<string>> GetUserAccessTokenAsync()
        {
            try
            {
                string accessToken = await AuthHelper.GetAccessTokenOnBehalfUserAsync(this.configuration, this.httpClientFactory, this.httpContextAccessor).ConfigureAwait(false);

                return accessToken;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates teamwork activity topic.
        /// </summary>
        /// <param name="installationIds">Represents installation ids.</param>
        /// <param name="requestInfoTaskId">Represents the task id of the request info object.</param>
        /// <param name="requestInfoTitle">Represents the title of the request info object.</param>
        /// <param name="teamsAppInstallationScopeId">Represents the teams app installation scope of the request info object.</param>
        /// <returns>A teamwork activity topic.</returns>
        private TeamworkActivityTopic CreateTeamworkActivtyTopic(IEnumerable<string> installationIds, string requestInfoTaskId, string requestInfoTitle, string teamsAppInstallationScopeId)
        {
            string teamsAppId = installationIds.First();

            // Provide installation scope and task id back to client, so when the request is responded to, these pieces of identifying information can be sent back.
            string subEntityId = $"{teamsAppInstallationScopeId}${requestInfoTaskId}";

            // Generate activity notification data to send.
            string url = "https://teams.microsoft.com/l/entity/" + teamsAppId + "/request?context={\"subEntityId\":\"" + subEntityId + "\"}";

            var topic = new TeamworkActivityTopic
            {
                Source = TeamworkActivityTopicSource.Text,
                Value = $"{requestInfoTitle}",
                WebUrl = url,
            };

            return topic;
        }
    }
}