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
            var graphClient = SimpleGraphClient.GetGraphClient(taskDetails.access_token);
            var graphClientApp = SimpleGraphClient.GetGraphClientforApp(_configuration["AzureAd:MicrosoftAppId"], _configuration["AzureAd:MicrosoftAppPassword"], _configuration["AzureAd:TenantId"]);
            var user = await graphClient.Users[taskDetails.userName]
                      .Request()
                      .GetAsync();
            var installedApps = await graphClient.Users[user.Id].Teamwork.InstalledApps
                               .Request()
                               .Expand("teamsAppDefinition")
                               .GetAsync();
            var installationId = installedApps.Where(id => id.TeamsAppDefinition.DisplayName == "NotifyFeedApp").Select(x => x.Id);
            var userName = user.UserPrincipalName;

            if (taskDetails.taskInfoAction == "customTopic")
            {
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChannelMessage = await chatMessage.CreateChatMessageForChannel(taskDetails, taskDetails.access_token);
                var customTopic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.Text,
                    Value = "Deployment Approvals Channel",
                    WebUrl = getChannelMessage.WebUrl
                };

                var CustomActivityType = "deploymentApprovalRequired";

                var CustomPreviewText = new ItemBody
                {
                    Content = "New deployment requires your approval"
                };

                var CustomTemplateParameters = new List<Microsoft.Graph.KeyValuePair>()
                 {
                 new Microsoft.Graph.KeyValuePair
                 {
                   Name = "deploymentId",
                   Value ="6788662"
                  }
                };
                try
                {
                    await graphClientApp.Users[user.Id].Teamwork
                        .SendActivityNotification(customTopic, CustomActivityType, null, CustomPreviewText, CustomTemplateParameters)
                        .Request()
                        .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                ViewBag.taskID = new Guid();
                var topic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.EntityUrl,
                    Value = "https://graph.microsoft.com/beta/users/" + user.Id + "/teamwork/installedApps/" + installationId.ToList()[0]
                };

                var activityType = "taskCreated";

                var previewText = new ItemBody
                {
                    Content = "New Task Created"
                };

                var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
            {
            new Microsoft.Graph.KeyValuePair
            {
              Name = "taskName",
              Value =taskDetails.title
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
            }

            return View("Index");
        }

        [HttpPost]
        [Route("SendNotificationToGroupChat")]
        public async Task<ActionResult> SendNotificationToGroupChat(TaskDetails taskDetails)
        {
            TaskHelper.AddTaskToFeed(taskDetails);
            var graphClient = SimpleGraphClient.GetGraphClient(taskDetails.access_token);
            var graphClientApp = SimpleGraphClient.GetGraphClientforApp(_configuration["AzureAd:MicrosoftAppId"], _configuration["AzureAd:MicrosoftAppPassword"], _configuration["AzureAd:TenantId"]);
             
            if (taskDetails.taskInfoAction == "customTopic")
            {
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChatMessage = chatMessage.CreateGroupChatMessage(taskDetails, taskDetails.access_token);
                var customTopic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.EntityUrl,
                    Value = "https://graph.microsoft.com/beta/chats/" + taskDetails.chatId + "/messages/" + getChatMessage.Id
                };

                var CustomActivityType = "approvalRequired";

                var CustomPreviewText = new ItemBody
                {
                    Content = "Deployment requires your approval"
                }; 
                var customRecipient = new ChatMembersNotificationRecipient
                {
                    ChatId = taskDetails.chatId
                };

                var CustomTemplateParameters = new List<Microsoft.Graph.KeyValuePair>()
                 {
                 new Microsoft.Graph.KeyValuePair
                 {
                   Name = "approvalTaskId",
                   Value ="2020AAGGTAPP"
                  }
                };
                try
                {
                    await graphClientApp.Chats[taskDetails.chatId]
                          .SendActivityNotification(customTopic, CustomActivityType, null, CustomPreviewText, CustomTemplateParameters, customRecipient)
                          .Request()
                          .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                var topic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.EntityUrl,
                    Value = "https://graph.microsoft.com/beta/chats/" + taskDetails.chatId
                };

                var activityType = "taskCreated";

                var previewText = new ItemBody
                {
                    Content = "Hello:"
                }; 
                var recipient = new ChatMembersNotificationRecipient
                {
                    ChatId = taskDetails.chatId
                };

                var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
                {
                    new Microsoft.Graph.KeyValuePair
                    {
                      Name = "taskName",
                      Value =taskDetails.title
                     }
               };
                try
                {
                    await graphClientApp.Chats[taskDetails.chatId]
                         .SendActivityNotification(topic, activityType, null, previewText, templateParameters, recipient)
                         .Request()
                         .PostAsync();
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
            var graphClient = SimpleGraphClient.GetGraphClient(taskDetails.access_token);
            var graphClientApp = SimpleGraphClient.GetGraphClientforApp(_configuration["AzureAd:MicrosoftAppId"], _configuration["AzureAd:MicrosoftAppPassword"], _configuration["AzureAd:TenantId"]);
            if (taskDetails.taskInfoAction == "customTopic")
            {
                ChatMessageHelper chatMessage = new ChatMessageHelper(_configuration);
                var getChannelMessage = await chatMessage.CreateChatMessageForChannel(taskDetails, taskDetails.access_token);
                var customTopic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.Text,
                    Value = "Deployment Approvals Channel",
                    WebUrl = getChannelMessage.WebUrl
                };

                var CustomActivityType = "approvalRequired";

                var CustomPreviewText = new ItemBody
                {
                    Content = "New deployment requires your approval"
                };
                var customRecipient = new TeamMembersNotificationRecipient
                {
                    TeamId = taskDetails.teamId
                };
                var CustomTemplateParameters = new List<Microsoft.Graph.KeyValuePair>()
                 {
                 new Microsoft.Graph.KeyValuePair
                 {
                   Name = "approvalTaskId",
                   Value ="5654653"
                  }
                };
                try
                {
                    await graphClientApp.Teams[taskDetails.teamId]
                          .SendActivityNotification(customTopic, CustomActivityType, null, CustomPreviewText, CustomTemplateParameters, customRecipient)
                          .Request()
                          .PostAsync();
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
                .Request()
                .Expand("teamsApp")
                .GetAsync();
                var tabId = tabs.Where(a => a.DisplayName == "NotifyFeedApp").Select(x => x.Id).ToArray()[0];
                var topic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.EntityUrl,
                    Value = "https://graph.microsoft.com/beta/teams/" + taskDetails.teamId + "/channels/" + taskDetails.channelId + "/tabs/" + tabId
                };

                var activityType = "reservationUpdated";

                var previewText = new ItemBody
                {
                    Content = "Your Reservation Updated:"
                }; 
                var recipient = new TeamMembersNotificationRecipient
                {
                    TeamId = taskDetails.teamId
                };
                var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
                {
                new Microsoft.Graph.KeyValuePair
                {
                  Name = "reservationId",
                  Value =taskDetails.reservationId
                 },
                  new Microsoft.Graph.KeyValuePair
                {
                  Name = "currentSlot",
                  Value =taskDetails.currentSlot
                 }
               };
                try
                {
                    await graphClientApp.Teams[taskDetails.teamId]
                         .SendActivityNotification(topic, activityType, null, previewText, templateParameters, recipient)
                         .Request()
                         .PostAsync();
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

                var topic = new TeamworkActivityTopic
                {
                    Source = TeamworkActivityTopicSource.Text,
                    WebUrl = getChannelMessage.WebUrl,
                    Value = "Deep Link to Chat"
                };

                var activityType = "pendingFinanceApprovalRequests";
                var previewText = new ItemBody
                {
                    Content = "These are the count of pending request pending request:"
                };
                var recipient = new TeamMembersNotificationRecipient
                {
                    TeamId = taskDetails.teamId
                };
                var templateParameters = new List<Microsoft.Graph.KeyValuePair>()
            {
            new Microsoft.Graph.KeyValuePair
            {
              Name = "pendingRequestCount",
              Value ="5"
             }
           };
                try
                {
                    await graphClientApp.Teams[taskDetails.teamId]
                         .SendActivityNotification(topic, activityType, null, previewText, templateParameters, recipient)
                         .Request()
                         .PostAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return View("teamnotification");
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