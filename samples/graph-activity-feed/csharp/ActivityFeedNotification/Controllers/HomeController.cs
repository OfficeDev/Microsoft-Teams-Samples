using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Beta.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TabActivityFeed.Helpers;
using TabActivityFeed.Model;
using TabActivityFeed.Repository;
using KeyValuePair = Microsoft.Graph.Beta.Models.KeyValuePair;
using TeamMembersNotificationRecipient = Microsoft.Graph.Beta.Models.TeamMembersNotificationRecipient;

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

        [Route("hello")]
        public ActionResult Hello()
        {
            TaskDetails idTask = new TaskDetails();
            idTask.taskId = Guid.NewGuid();
            ViewBag.taskId = idTask.taskId.ToString("N");

            return View("Index");
        }

        [Route("configure")]
        public ActionResult Configure()
        {
            return View("Configure");
        }

        [Route("groupchatnotification")]
        [HttpGet]
        public ActionResult GroupChatNotification()
        {
            return View();
        }

        [Route("details")]
        [HttpGet]
        [System.Web.Mvc.ChildActionOnly]
        public ActionResult Details()
        {
            return PartialView(FeedRepository.Tasks);
        }

        [Route("teamnotification")]
        public ActionResult TeamNotification()
        {
            return View();
        }

        [HttpPost]
        [Route("SendNotificationToUser")]
        public async Task<ActionResult> SendNotificationToUser(TaskDetails taskDetails)
        {
            TaskHelper.AddTaskToFeed(taskDetails);
            var graphClient = SimpleGraphClient.GetAuthenticatedClient(taskDetails.access_token);
            var graphClientApp = SimpleGraphClient.GetAuthenticatedClientforApp(_configuration["AzureAd:MicrosoftAppId"], _configuration["AzureAd:MicrosoftAppPassword"], _configuration["AzureAd:TenantId"]);
            try
            {
                var user = await graphClient.Users[taskDetails.userName]
                      .GetAsync();
                var userName = user.UserPrincipalName;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            
            var installationId = "";
            

            if (taskDetails.taskInfoAction == "customTopic")
            {
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                //var getChannelMessage = await chatMessage.CreateChatMessageForChannel(taskDetails, taskDetails.access_token);
                var requestBody = new Microsoft.Graph.Beta.Users.Item.Teamwork.SendActivityNotification.SendActivityNotificationPostRequestBody
                {
                    Topic = new TeamworkActivityTopic
                    {
                        Source = TeamworkActivityTopicSource.Text,
                        Value = "Deployment Approvals Channel",
                        WebUrl = "https://teams.microsoft.com/l/entity/" + _configuration["AzureAd:MicrosoftAppId"]
                    },
                    ActivityType = "taskCreated",
                    PreviewText = new ItemBody
                    {
                        Content = "New Task Created",
                    },
                    TemplateParameters = new List<Microsoft.Graph.Beta.Models.KeyValuePair>
                    {
                        new Microsoft.Graph.Beta.Models.KeyValuePair
                        {
                            Name = "taskName",
                            Value = taskDetails.taskName ?? "New Task",
                        },
                    },
                    IconId = "taskCreatedId"
                };
                try
                {
                    await graphClient.Users[_configuration["AzureAd:UserId"]].Teamwork
                        .SendActivityNotification.PostAsync(requestBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                ViewBag.taskID = new Guid();
                var requestBody = new Microsoft.Graph.Beta.Users.Item.Teamwork.SendActivityNotification.SendActivityNotificationPostRequestBody
                { 
                    Topic = new TeamworkActivityTopic
                    {
                        Source = TeamworkActivityTopicSource.Text,
                        Value = "Loop Thread",
                        WebUrl = "https://teams.microsoft.com/l/entity/" + _configuration["AzureAd:MicrosoftAppId"]
                    },
                    ActivityType = "taskCreated",
                    PreviewText = new ItemBody
                    {
                        Content = "New Task Created",
                    },
                    TemplateParameters = new List<KeyValuePair>
                    {
                        new KeyValuePair
                        {
                            Name = "taskName",
                            Value = taskDetails.taskName ?? "New Task",
                        },
                    },
                    IconId = "taskCreatedId"
                };
                try
                {
                    await graphClient.Users[_configuration["AzureAd:UserId"]].Teamwork
                        .SendActivityNotification.PostAsync(requestBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return View("Index");
        }

        [HttpPost]
        [Route("SendNotificationToGroupChat")]
        public async Task<ActionResult> SendNotificationToGroupChat(TaskDetails taskDetails)
        {
            TaskHelper.AddTaskToFeed(taskDetails);
            var graphClient = SimpleGraphClient.GetAuthenticatedClient(taskDetails.access_token);
            var graphClientApp = SimpleGraphClient.GetAuthenticatedClientforApp(_configuration["AzureAd:MicrosoftAppId"], _configuration["AzureAd:MicrosoftAppPassword"], _configuration["AzureAd:TenantId"]);

            if (taskDetails.taskInfoAction == "customTopic")
            {
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChatMessage = chatMessage.CreateGroupChatMessage(taskDetails, taskDetails.access_token);
                var requestBody = new Microsoft.Graph.Beta.Chats.Item.SendActivityNotification.SendActivityNotificationPostRequestBody
                {
                    Topic = new TeamworkActivityTopic
                    {
                        Source = TeamworkActivityTopicSource.EntityUrl,
                        Value = "https://graph.microsoft.com/beta/chats/" + taskDetails.chatId + "/messages/" + getChatMessage.Id
                    },
                    ActivityType = "taskCreated",
                    PreviewText = new ItemBody
                    {
                        Content = "New task created",
                    },
                    Recipient = new AadUserNotificationRecipient
                    {
                        OdataType = "microsoft.graph.aadUserNotificationRecipient",
                        UserId = _configuration["AzureAd:UserId"],
                    },
                    TemplateParameters = new List<KeyValuePair>
                    {
                        new KeyValuePair
                        {
                            Name = "taskName",
                            Value = "2020AAGGTAPP",
                        },
                    },
                    IconId = "taskCreatedId"
                };
                try
                {
                    await graphClient.Chats[taskDetails.chatId]
                          .SendActivityNotification.PostAsync(requestBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                var requestBody = new Microsoft.Graph.Beta.Chats.Item.SendActivityNotification.SendActivityNotificationPostRequestBody
                {
                    Topic = new TeamworkActivityTopic
                    {
                        Source = TeamworkActivityTopicSource.EntityUrl,
                        Value = "https://graph.microsoft.com/v1.0/chats/" + taskDetails.chatId,
                    },
                    ActivityType = "taskCreated",
                    PreviewText = new ItemBody
                    {
                        Content = "New Task Created",
                    },
                    Recipient = new AadUserNotificationRecipient
                    {
                        OdataType = "microsoft.graph.aadUserNotificationRecipient",
                        UserId = _configuration["AzureAd:UserId"],
                    },
                    TemplateParameters = new List<KeyValuePair>
                    {
                        new KeyValuePair
                        {
                            Name = "taskName",
                            Value = "12322",
                        },
                    },
                    IconId = "taskCreatedId"
                };
                try
                {
                    await graphClient.Chats[taskDetails.chatId]
                         .SendActivityNotification.PostAsync(requestBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return View("groupchatnotification");
        }

        [HttpPost]
        [Route("sendNotificationToTeam")]
        public async Task<ActionResult> sendNotificationToTeam(TaskDetails taskDetails)
        {
            TaskHelper.AddTaskToFeed(taskDetails);
            var graphClient = SimpleGraphClient.GetAuthenticatedClient(taskDetails.access_token);
            var graphClientApp = SimpleGraphClient.GetAuthenticatedClientforApp(_configuration["AzureAd:MicrosoftAppId"], _configuration["AzureAd:MicrosoftAppPassword"], _configuration["AzureAd:TenantId"]);
            if (taskDetails.taskInfoAction == "customTopic")
            {
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChannelMessage = await chatMessage.CreateChatMessageForChannel(taskDetails, taskDetails.access_token);
                var requestBody = new Microsoft.Graph.Beta.Teams.Item.SendActivityNotification.SendActivityNotificationPostRequestBody
                {
                    Topic = new TeamworkActivityTopic
                    {
                        Source = TeamworkActivityTopicSource.Text,
                        Value = "Deployment Approvals Channel",
                        WebUrl = getChannelMessage.WebUrl
                    },
                    ActivityType = "approvalRequired",
                    PreviewText = new ItemBody
                    {
                        Content = "New deployment requires your approval",
                    },
                    Recipient = new TeamMembersNotificationRecipient
                    {
                        TeamId = taskDetails.teamId
                    },
                    TemplateParameters = new List<KeyValuePair>
                    {
                        new KeyValuePair
                        {
                            Name = "approvalTaskId",
                            Value = "5654653",
                        },
                    },
                };
                try
                {
                    await graphClientApp.Teams[taskDetails.teamId]
                          .SendActivityNotification.PostAsync(requestBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else if (taskDetails.taskInfoAction == "channelTab")
            {
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChannelMessage = chatMessage.CreateChannelMessageAdaptiveCard(taskDetails, taskDetails.access_token);

                var tabs = await graphClient.Teams[taskDetails.teamId].Channels[taskDetails.channelId].Tabs
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Expand = new[] { "teamsApp" };
                });
                var tabId = tabs.Value.Where(a => a.DisplayName == "NotifyFeedApp").Select(x => x.Id).ToArray()[0];
                var requestBody = new Microsoft.Graph.Beta.Teams.Item.SendActivityNotification.SendActivityNotificationPostRequestBody
                {
                    Topic = new TeamworkActivityTopic
                    {
                        Source = TeamworkActivityTopicSource.EntityUrl,
                        Value = "https://graph.microsoft.com/beta/teams/" + taskDetails.teamId + "/channels/" + taskDetails.channelId + "/tabs/" + tabId
                    },
                    ActivityType = "reservationUpdated",
                    PreviewText = new ItemBody
                    {
                        Content = "Your Reservation Updated:",
                    },
                    Recipient = new TeamMembersNotificationRecipient
                    {
                        TeamId = taskDetails.teamId
                    },
                    TemplateParameters = new List<KeyValuePair>
                    {
                        new KeyValuePair
                        {
                            Name = "reservationId",
                            Value = taskDetails.reservationId
                        },
                        new KeyValuePair
                        {
                            Name = "currentSlot",
                            Value =taskDetails.currentSlot
                        }
                    },
                };

                try
                {
                    await graphClientApp.Teams[taskDetails.teamId]
                         .SendActivityNotification.PostAsync(requestBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChannelMessage = await chatMessage.CreatePendingFinanceRequestCard(taskDetails, taskDetails.access_token);

                var requestBody = new Microsoft.Graph.Beta.Teams.Item.SendActivityNotification.SendActivityNotificationPostRequestBody
                {
                    Topic = new TeamworkActivityTopic
                    {
                        Source = TeamworkActivityTopicSource.EntityUrl,
                        WebUrl = getChannelMessage.WebUrl,
                        Value = "https://graph.microsoft.com/beta/teams/" + taskDetails.teamId
                    },
                    ActivityType = "pendingFinanceApprovalRequests",
                    PreviewText = new ItemBody
                    {
                        Content = "These are the count of pending request pending request:",
                    },
                    Recipient = new AadUserNotificationRecipient
                    {
                        OdataType = "microsoft.graph.aadUserNotificationRecipient",
                        UserId = _configuration["AzureAd:UserId"],
                    },
                    TemplateParameters = new List<KeyValuePair>
                    {
                        new KeyValuePair
                        {
                            Name = "pendingRequestCount",
                            Value = "5"
                        }
                    },
                    IconId = "pendingFinanceApprovalRequestsId"
                };

                try
                {
                    await graphClient.Teams[taskDetails.teamId]
                         .SendActivityNotification.PostAsync(requestBody);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return View("teamnotification");
        }

       
        [HttpPost]
        [Route("sendDefaultNotifications")]
        public async Task<ActionResult> sendDefaultNotifications(TaskDetails taskDetails)
        {
            TaskHelper.AddTaskToFeed(taskDetails);
            var graphClient = SimpleGraphClient.GetAuthenticatedClient(taskDetails.access_token);
            var graphClientApp = SimpleGraphClient.GetAuthenticatedClientforApp(_configuration["AzureAd:MicrosoftAppId"], _configuration["AzureAd:MicrosoftAppPassword"], _configuration["AzureAd:TenantId"]);

            try
            {
                var tabs = await graphClient.Teams[taskDetails.teamId].Channels[taskDetails.channelId].Tabs
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Expand = new[] { "teamsApp" };
                });

                var tabId = tabs.Value.Where(a => a.DisplayName == "NotifyFeedApp").Select(x => x.Id).ToArray()[0];

                var requestBody = new Microsoft.Graph.Beta.Teams.Item.SendActivityNotification.SendActivityNotificationPostRequestBody
                {
                    Topic = new TeamworkActivityTopic
                    {
                        Source = TeamworkActivityTopicSource.EntityUrl,
                        Value = "https://graph.microsoft.com/beta/teams/" + taskDetails.teamId + "/channels/" + taskDetails.channelId
                    },
                    ActivityType = "systemDefault",
                    PreviewText = new ItemBody
                    {
                        Content = "Default feed notification sent to channel",
                    },
                    Recipient = new TeamMembersNotificationRecipient
                    {
                        TeamId = taskDetails.teamId
                    },
                    TemplateParameters = new List<KeyValuePair>
                    {
                        new KeyValuePair
                        {
                            Name = "systemDefaultText",
                            Value = "Default feed notification sent to channel"
                        }
                    }
                };

                await graphClientApp.Teams[taskDetails.teamId]
                    .SendActivityNotification.PostAsync(requestBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return View("teamnotification");
        }

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